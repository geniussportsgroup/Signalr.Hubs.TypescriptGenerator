[![Build status](https://ci.appveyor.com/api/projects/status/uwhg9nwo62kx2fif?svg=true)](https://ci.appveyor.com/project/cjbhaines/signalr-hubs-typescriptgenerator)

# Signalr.Hubs.TypeScriptGenerator
Utility library for generating typescript definitions for Signalr Hubs. This is a fork of [yetanotherchris/SignalrTypeScriptGenerator](https://github.com/yetanotherchris/SignalrTypeScriptGenerator "SignalrTypeScriptGenerator") by [yetanotherchris](https://github.com/yetanotherchris "yetanotherchris"). I have split the packages up into a referenced library and a console app.

Our usage at Genius Sports is to generate the Hub proxies at build time using our [geniussportsgroup/SignalR.ProxyGenerator](https://github.com/geniussportsgroup/SignalR.ProxyGenerator "Proxy Generator") publishing them to our internal NPM feed. We then use this tool to generate TypeScript definitions our those proxies again publishing them to our internal NPM feed. This allows our UI developers to get strongly typed Hub APIs and allows us to do proper Continous Integrtaion between the back end and front end. Move quickly and break fast.

## Features
- Interfaces generated for all data contracts used in the Hub methods arguments.
- For types not used in any Hub method signature, interface is generated if the type is declared with 
  *KnownTypeAttribute* in any of directly used data contracts.
- *@deprecated* JSDoc comment added for members marked with ObsoleteAttribute. Properties and eumeration members
  included, although might be not supported by JSDoc.
- Optional data members can be generated in interface.
- Strong type declarations can be generated to support TypeScript 2 compiled with *--strictTypeChecks* option.
- *Enum* types generated also in the separate *.exports.ts* file to enable using those in client code.

## Installation - Nuget

- [Signalr.Hubs.TypeScriptGenerator](https://www.nuget.org/packages/Signalr.Hubs.TypeScriptGenerator "Signalr.Hubs.TypeScriptGenerator")
- [Signalr.Hubs.TypeScriptGenerator.Console](https://www.nuget.org/packages/Signalr.Hubs.TypeScriptGenerator.Console "Signalr.Hubs.TypeScriptGenerator.Console")

## Usage

### Signalr.Hubs.TypeScriptGenerator
The utility library is simple to use, load any assemblies required and then create a HubTypeScriptGenerator and call 
the *Generate* method:

    var generator = new HubTypeScriptGenerator();
    var typeScript = generator.Generate(TypeScriptGeneratorOptions.Default);

The *Generate* method takes on input generator options (described in details below) and returns Tuple<string, string> 
with both items containing generated TypeScript. The first item contains generated declarations, while the second 
contains exported code. 

**NOTE:** Specifiying a specific assembly to scan is not supported, since the SignalR DefaultHubManager 
is used which looks up for Hub implementations in all loaded assemblies.

#### TypeScriptGeneratorOptions

Several options are provided via *TypeScriptGeneratorOptions* class to enable more control over generated code:
 - ReferencePaths
 - OptionalMemberGenerationMode
 - GenerateStrictTypes
 - NotNullableTypeDiscovery
 - IncludedTypesDiscovery
 - EnumMemberNameMappingMode

The static *Default* property returns the instance having all options initialized with default values. It also 
provides set of customization methods designed for *fluent* coding style. The below example should be quite 
self-describing: 

    var options = TypeScriptGeneratorOptions.Default
        .WithReferencePaths(
            "../signalr/index.d.ts", 
            "../jquery/index.d.ts")
        .WithStrictTypes(
            NotNullableTypeDiscovery.UseRequiredAttribute)
        .WithOptionalMembers(
            OptionalMemberGenerationMode.UseDataMemberAttribute);

Below, each option is described in details.

##### ReferencePaths
Optional collection of file paths, that are inserted into generated code as **\<reference \/\>** instructions. 
The above usage example will add the following references:

    /// <reference path="../signalr/index.d.ts" />
    /// <reference path="../jquery/index.d.ts" />

##### OptionalMemberGenerationMode
Indicates if and when contract interface members shall be generated as optional (having the member name decorated 
with **?** suffix).
- OptionalMemberGenerationMode.None - Instructs not to generate optional members (the default value).
- OptionalMemberGenerationMode.UseDataMemberAttribute - Instructs to generate members attributed by 
     *System.Runtime.Serialization.DataMemberAttribute* with the *IsRequired* proeprty set to **false**, as optional.
     Note, if the *IsRequired* property initializer is omitted, it defaults to **false**. 

Example C# code: 

    [DataContract]
    class SampleDto
    {
        [DataMember]
        public string PhoneNumber1 { get; set; }

        [DataMember(IsRequired=false)]
        public string PhoneNumber2 { get; set; }

        [DataMember (IsRequired=true)]
        public string PhoneNumber3 { get; set; }

        // [DataMember] attribute is omitted
        public string PhoneNumber4 { get; set; }
    }


When generated using *OptionalMemberGenerationMode.None*:

    interface SampleDto {
        phoneNumber1 : string;
        phoneNumber2 : string;
        phoneNumber3 : string;
        phoneNumber4 : string;
    }
     
When generated using *OptionalMemberGenerationMode.UseDataMemberAttribute*:

    interface SampleDto {
        phoneNumber1? : string;
        phoneNumber2? : string;
        phoneNumber3 : string;
        phoneNumber4 : string;
    }

##### GenerateStrictTypes
If **true** specified, instructs to generate strict type definitions by explicitly adding *null* type to the member 
type declaration for nullable members. The default value is **false**. 

This option shall only be used when generated script is expected to be compiled
with TypeScript 2.0 compiler using the *strictNullChecks* compiler option. In such case types of nullable members are 
generated as *union* types, containing primary type, plus *null* type. All the *Nullable<>* and reference types are 
generated as nullable (see also the option *NotNullableTypeDiscovery* below).
 
Example C# code: 

    class SampleDto
    {
        public string Address { get; set; }
    }

When generated using *GenerateStrictTypes*=**false**:

    interface SampleDto {
        address : string;
    }
     
When generated using *GenerateStrictTypes*=**true**::

    interface SampleDto {
        address : string | null;
    }

##### NotNullableTypeDiscovery
Identifies the method used to discover members that should not be declared as nullable, where otherwise they would be.
This option is only applicable if the *GenerateStrictTypes* option is set to **true**. Otherwise, it is ignored.

- *NotNullableTypeDiscovery.None* - use default approach (All *Nullable<>* and reference types generated as nullable).
- *NotNullableTypeDiscovery.UseRequiredAttribute* - treat members attrobiuted with 
  *System.ComponentModel.DataAnnotations.RequiredAttribute* as non-nullable.

Example C# code: 

    class SampleDto
    {
        [Required]
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public int?   ClientCode   { get; set; }
    }

When generated using *NotNullableTypeDiscovery.None*:

    interface SampleDto {
        addressLine1 : string;
        addressLine2 : string;
        clientCode : number;
    }
     
When generated using *NotNullableTypeDiscovery.UseRequiredAttribute*:

    interface SampleDto {
        addressLine1 : string;
        addressLine2 : string | null;
        clientCode : number | null;
    }

##### IncludedTypesDiscovery
Defines methods to discover types that should be included in the generated type script (when not used directly in the 
Hub interface method arguments). This is useful for example, to generate interfaces for derived types, when base type 
references are passed to Hub methods.

- *IncludedTypesDiscovery.None* - Do not include any types other than directly used in Hub interface methods. (Default.)
- *IncludedTypesDiscovery.UseKnownTypeAttribute* - Generate interfaces for types declared in the [KnownType] attribute 
  in any of types directly used in Hub methods.

Example C# code: 

    [DataContract]
    [KnownType(typeof(InheritedSomethingDto))]
    public class SomethingDto
    {
        [DataMember]
        public string Property1 { get; set; }
    }

    [DataContract]
    public class InheritedSomethingDto : SomethingDto
    {
        [DataMember]
        public int OptionalInteger { get; set; }
    }

    public interface IHubAClient
    {
        void TakeThis(SomethingDto somethingDto);
    }


When generated using *IncludedTypesDiscovery.None*, typings will contain defintiions for

    interface SomethingDto { ... }
    interface IHubAClient { ... }

When generated using *NotNullableTypeDiscovery.UseKnownTypeAttribute*, typings will contain defintiions for

    interface SomethingDto { ... }
    interface InheritedSomethingDto { ... }
    interface IHubAClient { ... }

##### EnumMemberNameMappingMode
Defines supported methods for mapping .NET model enum member name to name of enum member in generated typings.

- *MemberName* - use enum member name as defined (Default).
- *MemberNameCamelCase* - use enum member name with first letter in lower case (camel-case).
- *MemberNameLowerCase* - use enum member name converted to lower case.
- *MemberNameUpperCase* - use enum member name converted to upper case.
- *EnumMemberAttributeValue* - use the value of the *System.Runtime.Serialization.EnumMemberAttribute* if applied to enum meber.
  If the attribute is not defined, fall back to *MemberName* option.

Example C# code:

    public enum SomethingEnum
    {
        One = 101,
        Two = 202,
        [EnumMember(Value = "oneAndTwo")]
        Three = One + Two
    }

When generated using *EnumMemberNameMappingMode.MemberName*:

    enum SomethingEnum {
        One = 101,
        Two = 202,
        Three = 303
    }

When generated using *EnumMemberNameMappingMode.EnumMemberAttributeValue*:

    enum SomethingEnum {
        One = 101,
        Two = 202,
        oneAndTwo = 303
    }

### Signalr.Hubs.TypeScriptGenerator.Console
Can be used to generate TypeScipt by supplying input from command line.

#### Command line options
Options can be specified using short or long names. The short names are single character prepended with hyphen ('**-**').
The long name must be prepended by double hyphen ('*--*'). Below is the list of supported options.

| Option                  | Description |
|-------------------------|-----------------------------------------------------------------|
| -a, --assembly          | **Required**. The path to the assembly (.dll/.exe) |
| -o, --output            | The path to the generated file containing declarations code. If this is empty, the output file name is written to stdout. If it ends with directory separator, the path is treated as directory and the output file name is generated from supplied assembly file name and written to the folder specified by output path. Exports file name is always derived from the declarations file. |
| -r, --references        | Optional. List of reference file paths, delimited by semicolon. The **"/// \<reference\/\>** instruction is added for each file. |
| -p, --optionalMembers   | Default: *None*. Specifies method to discover members treated as optional. Supported values:<br>*None* - don't generate optional members.<br>*DataMemberAttribute* - use [DataMember(IsRequired)] attribute. |
| -s, --strictTypes       | Default: *False*. If true, union definitions with *null* are generated for nullable types. |
| -n, --notNullableTypes  | Default: *None*. Specifies method to discover members treated as not-nullable. Supported values:<br>*None* - don't generate optional members.<br>*RequiredAttribute* - use [Required] attribute. |
| -i, --includeTypes      | Default: *None*. Specifies methods to discover additional types to be included. Supported values:<br>*None* - don't look for additional types. <br>*KnownTypeAttribute* - include classes declared with [KnownType] attribute in data contracts included. |
| --help                  | Display help screen.

If the output file is not specified the result is written to standard out.

**NOTE:** Exports file will be automatically added as *reference* in decalrations file.

#### Usage examples:

For brevity, executable name used in examples is *generate.exe*.

    generate.exe -a "c:\myapp\hubs.dll" -o "c:\temp\myapp.hubs.d.ts"

Generates declarations and exports content from the specified assembly into *myapp.hubs.d.ts* and 
*myapp.hubs.exports.ts* files, respectively, in the "c:\temp\" directory. Default options will be used.

    generate.exe -a "c:\myapp\hubs.dll" -o "c:\temp\"

The output path ending with directory separator will cause output file generated from the input assembly name.
Declarations and exports will be written to *hubs.d.ts* and *hubs.exports.ts*, in the *c:\\temp\\* directory.

    generate.exe -a "c:\myapp\hubs.dll" -o "c:\temp\" -p DataMemberAttribute  -s -n RequiredAttribute -r "../signalr/index.d.ts;../jquery/index.d.ts"

Declarations and exports file names are derived from input assembly file name. Output scripts will contains two 
reference instructions. Interface members will be generated as optional depending on the *[DataMemberAttribute]* 
applied to data contract properties. Reference and *Nullable<>* types will be generated as union types, explicitly 
including *null* type except for interface members having *[Required]* attribute.

 
**# ONLY COMBATIBLE WITH SIGNALR VERSIONS AT 2.2.1.0 OR EARLIER #**

We have compiled this at verison 2.2.1.0 so in order for the HubManager to recognise your hubs we are using an assembly redirect. If Microsoft release a new version we will need to update this.
 
    <dependentAssembly>
        <assemblyIdentity name="Microsoft.AspNet.SignalR.Core" publicKeyToken="31bf3856ad364e35" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-2.2.1.0" newVersion="2.2.1.0" />
      </dependentAssembly>

### Data Contract Property Names
Sometimes the serialized name of your data contract properties are changed from the actual C# property name. This is done through the DataMember property:

    [DataContract]
    public class SomethingDto 
    {
        [DataMember(Name = "iChangedTheName")]
        public string Property1 { get; set; }

        [DataMember]
        public Guid Property2 { get; set; }
    }

This library will respect the DataMember name and use this as the TypeScript property name:
    
    declare module GeniusSports.Signalr.Hubs.TypeScriptGenerator.SampleUsage.DataContracts {
        interface SomethingDto {
            iChangedTheName : string;
            Property2 : System.Guid;
        }
    }

## Example Output

The below output is generated from the assembly produced by the SampleUsage project (part of the solution) using the 
following code:

    var hubTypeScriptGenerator = new HubTypeScriptGenerator();
    var options = TypeScriptGeneratorOptions.Default
        .WithReferencePaths(
            @"../signalr/index.d.ts",
            @"../jquery/index.d.ts")
        .WithStrictTypes(NotNullableTypeDiscovery.UseRequiredAttribute)
        .WithOptionalMembers(OptionalMemberGenerationMode.UseDataMemberAttribute)
        .WithIncludedTypes(IncludedTypesDiscovery.UseKnownTypeAttribute)
        .WithEnumMemberNameMappingMode(EnumMemberNameMappingMode.EnumMemberAttributeValue);
    var typeScript = hubTypeScriptGenerator.Generate(options);



### Generated Declarations

    //------------------------------------------------------------------------------
    // <auto-generated>
    //
    // This code was generated by a tool.
    //
    // Changes to this file may cause incorrect behavior and will be lost if
    // the code is regenerated.
    //
    // 2017-04-27 09:25:08Z
    // https://github.com/geniussportsgroup/Signalr.Hubs.TypeScriptGenerator
    //
    // </auto-generated>
    //------------------------------------------------------------------------------

    /// <reference path="../signalr/index.d.ts" />
    /// <reference path="../jquery/index.d.ts" />

    // Hubs

    interface SignalR {
        /*
         * @deprecated Superseded by HubB.
         */
        hubA : GeniusSports.Signalr.Hubs.TypeScriptGenerator.SampleUsage.Hubs.HubA;
        hubC : GeniusSports.Signalr.Hubs.TypeScriptGenerator.SampleUsage.Hubs.HubC;
        hubB : GeniusSports.Signalr.Hubs.TypeScriptGenerator.SampleUsage.Hubs.HubB;
    }

    // Service contracts

    declare module GeniusSports.Signalr.Hubs.TypeScriptGenerator.SampleUsage.Hubs {
        /*
         * @deprecated Superseded by HubB.
         */
        interface HubA {
            server : GeniusSports.Signalr.Hubs.TypeScriptGenerator.SampleUsage.Hubs.HubAServer;
            client : GeniusSports.Signalr.Hubs.TypeScriptGenerator.SampleUsage.Hubs.IHubAClient;
        }

        /*
         * @deprecated Superseded by HubB.
         */
        interface HubAServer {
            /*
             * @deprecated
             */
            getSomething() : JQueryPromise<GeniusSports.Signalr.Hubs.TypeScriptGenerator.SampleUsage.DataContracts.SomethingDto>;
            /*
             * @deprecated for testing reasons.
             */
            ping() : JQueryPromise<void>;
        }

        interface HubC {
            server : GeniusSports.Signalr.Hubs.TypeScriptGenerator.SampleUsage.Hubs.HubCServer;
             // TODO: Hub does not have a Client Interface as a generic argument - it is recommended to add one.
            client : any;
        }

        interface HubCServer {
            aServerSideMethod() : JQueryPromise<void>;
        }

        interface HubB {
            server : GeniusSports.Signalr.Hubs.TypeScriptGenerator.SampleUsage.Hubs.HubBServer;
            client : GeniusSports.Signalr.Hubs.TypeScriptGenerator.SampleUsage.Hubs.IHubBClient;
        }

        interface HubBServer {
            getOtherSomething() : JQueryPromise<GeniusSports.Signalr.Hubs.TypeScriptGenerator.SampleUsage.DataContracts.OtherSomethingDto>;
            doOtherSomethingElse() : JQueryPromise<void>;
        }

    }

    // Clients

    declare module GeniusSports.Signalr.Hubs.TypeScriptGenerator.SampleUsage.Hubs {
        interface IHubAClient {
            /*
             * @deprecated
             */
            pong : () => void;
            /*
             * @deprecated for testing reasons.
             */
            takeThis : (somethingDto : GeniusSports.Signalr.Hubs.TypeScriptGenerator.SampleUsage.DataContracts.SomethingDto) => void;
        }

        interface IHubBClient {
            takeOtherThis : (otherSomethingDto : GeniusSports.Signalr.Hubs.TypeScriptGenerator.SampleUsage.DataContracts.OtherSomethingDto) => void;
        }

    }

    // Data contracts
    declare module GeniusSports.Signalr.Hubs.TypeScriptGenerator.SampleUsage.DataContracts {
        interface SomethingDto {
            iChangedTheName? : string | null;
            requiredString : string | null;
            OptionalGuid? : string | null;
            RequiredGuid : string;
            NullableRequiredGuid : string;
            OptionalInnerSomething? : GeniusSports.Signalr.Hubs.TypeScriptGenerator.SampleUsage.DataContracts.InnerSomethingDto;
        }

        interface OtherSomethingDto {
            RequiredString : string | null;
            OptionalDateTime? : Date;
            OptionalInnerSomething? : GeniusSports.Signalr.Hubs.TypeScriptGenerator.SampleUsage.DataContracts.InnerSomethingDto;
            RequiredInnerSomething : GeniusSports.Signalr.Hubs.TypeScriptGenerator.SampleUsage.DataContracts.InnerSomethingDto;
        }

        interface InnerSomethingDto {
            InnerPropertyInt? : number;
            innerProperty2? : Date;
            /*
             * @deprecated Do not use properties with crazy names.
             */
            innerProperty3WithCrazyCustomName? : GeniusSports.Signalr.Hubs.TypeScriptGenerator.SampleUsage.DataContracts.SomethingEnum;
            inner123? : any;
        }

        /*
         * @deprecated Will be removed in beta version.
         */
        interface InheritedSomethingDto extends GeniusSports.Signalr.Hubs.TypeScriptGenerator.SampleUsage.DataContracts.SomethingDto {
            OptionalInteger? : number;
            NullableInteger : number | null;
            /*
             * @deprecated This might be removed in next version.
             */
            OptionalNullableInteger? : number | null;
        }

        interface InnerSomethingDto1 {
            Dto2? : GeniusSports.Signalr.Hubs.TypeScriptGenerator.SampleUsage.DataContracts.InnerSomethingDto4;
        }

        interface InnerSomethingDto2 {
            Dto4? : GeniusSports.Signalr.Hubs.TypeScriptGenerator.SampleUsage.DataContracts.InnerSomethingDto4;
        }

        interface InnerSomethingDto3 {
            InnerProperty3? : number;
        }

        interface InnerSomethingDto4 {
            Dto5? : GeniusSports.Signalr.Hubs.TypeScriptGenerator.SampleUsage.DataContracts.InnerSomethingDto5;
        }

        interface InnerSomethingDto5 {
            Dto3? : GeniusSports.Signalr.Hubs.TypeScriptGenerator.SampleUsage.DataContracts.InnerSomethingDto3;
            Enum5 : GeniusSports.Signalr.Hubs.TypeScriptGenerator.SampleUsage.DataContracts.Enum5;
        }

    }

    // Enums

    declare module GeniusSports.Signalr.Hubs.TypeScriptGenerator.SampleUsage.DataContracts {
        /*
         * @deprecated
         */
        enum SomethingEnum {
            One = 101,
            Two = 202,
            /*
             * @deprecated Do not use this value. Defined for backward compatibility.
             */
            oneAndTwo = 303,
        }

        enum Enum5 {
            None = 0,
            Five = 1,
        }

    }


### Generated Exports

    //------------------------------------------------------------------------------
    // <auto-generated>
    //
    // This code was generated by a tool.
    //
    // Changes to this file may cause incorrect behavior and will be lost if
    // the code is regenerated.
    //
    // 2017-04-27 09:25:08Z
    // https://github.com/geniussportsgroup/Signalr.Hubs.TypeScriptGenerator
    //
    // </auto-generated>
    //------------------------------------------------------------------------------

    // Enums

    export module GeniusSports.Signalr.Hubs.TypeScriptGenerator.SampleUsage.DataContracts {
        /*
         * @deprecated
         */
        export enum SomethingEnum {
            One = 101,
            Two = 202,
            /*
             * @deprecated Do not use this value. Defined for backward compatibility.
             */
            oneAndTwo = 303,
        }

        export enum Enum5 {
            None = 0,
            Five = 1,
        }

    }
