using System;
using System.IO;
using CommandLine;
using CommandLine.Text;

namespace GeniusSports.Signalr.Hubs.TypeScriptGenerator.Console
{
	public class CommandLineOptions
	{
		[Option('a', "assembly", HelpText = "The path to the assembly (.dll/.exe)", Required = true)]
		public string AssemblyPath { get; set; }

		[Option('o', "output", HelpText = "The path to the file to generate. If this is empty, the output is written to stdout.")]
		public string Output { get; set; }

		[Option('r', "references", HelpText = "Optional. List of file paths, delimited by semicolon to be added as the <reference/> instruction.", Required = false)]
		public string References { get; set; }

		[Option('p', "optionalMembers", HelpText = "Specifies method to discover members treated as optional: None - don't generate optional members; DataMemberAttribute - use [DataMember(IsRequired)] attribute.", Required = false, DefaultValue = "None")]
		public string OptionalMembers { get; set; }

		[Option('s', "strictTypes", HelpText = "If true, union definitions with 'null' are generated for nullable types.", Required = false)]
		public bool StrictTypes { get; set; }

		[Option('n', "notNullableTypes", HelpText = "Specifies method to discover members treated as not-nullable: RequiredAttribute: - use [Required] attribute.", Required = false)]
		public string NotNullableTypes { get; set; }

		[Option('i', "includeTypes", HelpText = "Specifies methods to discover additional types to be included. KnownTypeAttribute - include classes declared with [KnownType] attribute.", Required = false)]
		public string IncludeTypes { get; set; }

        [Option('d', "includeAllDataContracts", HelpText = "Optional. If specified will include all types marked with [DataContract] attribute.", Required = false)]
        public bool IncludeAllDataContracts { get; set; }

        [Option('e', "enumMemberNames", HelpText = "If specified,indicates method to map enum member names. Valid values: Default,CamelCase,LowerCase,UpperCase,EnumMemberAttribute.", Required = false, DefaultValue= "Default")]
		public string EnumMemberNameMapMode { get; set; }

		[HelpOption]
		public string GetUsage()
		{
			return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
		}

		public OptionalMemberGenerationMode GetOptionalMemberGenerationMode()
		{
			if (string.IsNullOrEmpty(OptionalMembers))
				return OptionalMemberGenerationMode.None;

			switch (OptionalMembers.ToLowerInvariant())
			{
				case "none":
					return OptionalMemberGenerationMode.None;

				case "datamemberattribute":
					return OptionalMemberGenerationMode.UseDataMemberAttribute;

				default:
					throw new NotSupportedException($"Specified required member discovery option is not supported: {OptionalMembers}");
			}
		}

		public NotNullableTypeDiscovery GetNotNullableTypeDiscovery()
		{
			if (string.IsNullOrEmpty(NotNullableTypes))
				return NotNullableTypeDiscovery.None;

			switch (NotNullableTypes.ToLowerInvariant())
			{
				case "none":
					return NotNullableTypeDiscovery.None;

				case "requiredattribute":
					return NotNullableTypeDiscovery.UseRequiredAttribute;

				default:
					throw new NotSupportedException($"Specified required member discovery option is not supported: {NotNullableTypes}");
			}
		}

		public IncludedTypesDiscovery GetIncludedTypesDiscovery()
		{
			if (string.IsNullOrEmpty(IncludeTypes))
				return IncludedTypesDiscovery.None;

			switch (IncludeTypes.ToLowerInvariant())
			{
				case "none":
					return IncludedTypesDiscovery.None;

				case "knowntypeattribute":
					return IncludedTypesDiscovery.UseKnownTypeAttribute;

                default:
					throw new NotSupportedException($"Specified included types discovery option is not supported: {IncludeTypes}");
			}
		}

		public EnumMemberNameMappingMode GetEnumMemberNameMappingMode()
		{
			if (string.IsNullOrEmpty(EnumMemberNameMapMode))
				return EnumMemberNameMappingMode.MemberName;

			switch (EnumMemberNameMapMode.ToLowerInvariant())
			{
				case "default":
					return EnumMemberNameMappingMode.MemberName;
				case "camelcase":
					return EnumMemberNameMappingMode.MemberNameCamelCase;
				case "lowercase":
					return EnumMemberNameMappingMode.MemberNameLowerCase;
				case "uppercase":
					return EnumMemberNameMappingMode.MemberNameUpperCase;
				case "enummemberattribute":
					return EnumMemberNameMappingMode.EnumMemberAttributeValue;
				default:
					throw new NotSupportedException($"Specified enum member name mapping mode option is not supported: {EnumMemberNameMapMode}");
			}
		}

		/// <summary>
		/// Analyzes specified output path, and returnes adjusted/generateed output file path.
		/// </summary>
		public string GetOutputPath()
		{
			if (string.IsNullOrWhiteSpace(Output) || Output[Output.Length - 1] != Path.DirectorySeparatorChar)
				return Output;

			// Treat output path as directory, and generate output file name.

			var assemblyFileName = Path.GetFileNameWithoutExtension(AssemblyPath);
			return Path.Combine(Output, assemblyFileName + ".d.ts");
		}
	}
}