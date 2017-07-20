namespace GeniusSports.Signalr.Hubs.TypeScriptGenerator
{
	/// <summary>
	/// Encapsulates options controlling valrious aspects of Type Script code generation.
	/// </summary>
	/// <remarks>
	/// To construct the instance, use the <see cref="Default"/> member to access the instance with default options.
	/// Use fluent members to construct customized settings.
	/// </remarks>
	public sealed class TypeScriptGeneratorOptions
	{
		/// <summary>
		/// Semicolon-delimited file paths added as <c>&lt;reference /&gt;</c> instructions to the declarations file.
		/// </summary>
		public string[] ReferencePaths { get; }

        /// <summary>
        /// Path of assembly to generate types from.
        /// </summary>
        public string AssemblyPath { get; }

        /// <summary>
        /// Indicates if and how optional contract interface members shall be generated.
        /// </summary>
        public OptionalMemberGenerationMode OptionalMemberGenerationMode { get; }

		/// <summary>
		/// Instructs to generate strict type definitions by explicitly adding <c>null</c> type to the member type 
		/// declaration for nullable members. 
		/// <para>
		/// This option is useful when TypeScipt is expected to be compiled using the <b>"--strictNullChecks"</b> 
		/// compiler option.
		/// </para>
		/// </summary>
		public bool GenerateStrictTypes { get; }

		/// <summary>
		/// Identifies the method used to discover members that should not be declared as <i>nullable</i>. 
		/// This option is only applicable when <see cref="GenerateStrictTypes"/> is <see langword="true"/>; 
		/// ignored otherwise.
		/// </summary>
		public NotNullableTypeDiscovery NotNullableTypeDiscovery { get; }

		/// <summary>
		/// Specifies the method used to discover additional types to be included in the generated TypeScript
		/// which are not directly used in Hub methods.
		/// </summary>
		public IncludedTypesDiscovery IncludedTypesDiscovery { get; }

        /// <summary>
        /// Indicates to include all types marked with [DataContract] attribute.
        /// </summary>
        public bool IncludeAllDataContracts { get; }

        /// <summary>
        /// Specifies the method used to map .NET model enum member names to names of enum members in generated typings.
        /// </summary>
        public EnumMemberNameMappingMode EnumMemberNameMappingMode { get; }

		private TypeScriptGeneratorOptions()
		{
		}

		private TypeScriptGeneratorOptions(
			string[] referencePaths,
			OptionalMemberGenerationMode optionalMemberGenerationMode, 
			bool generateStrictTypes, 
			NotNullableTypeDiscovery notNullableTypeDiscovery,
			IncludedTypesDiscovery includedTypesDiscovery,
			EnumMemberNameMappingMode enumMemberNameMappingMode,
            bool includeAllDataContracts,
            string assemblyPath)
		{
			ReferencePaths = referencePaths;
			OptionalMemberGenerationMode = optionalMemberGenerationMode;
			GenerateStrictTypes = generateStrictTypes;
			NotNullableTypeDiscovery = notNullableTypeDiscovery;
			IncludedTypesDiscovery = includedTypesDiscovery;
			EnumMemberNameMappingMode = enumMemberNameMappingMode;
		    IncludeAllDataContracts = includeAllDataContracts;
		    AssemblyPath = assemblyPath;
		}

		/// <summary>
		/// Returns the instance of <see cref="TypeScriptGeneratorOptions"/> initialized with default options.
		/// By defualt, strict nullable types are not generated, and interface members are never declared as optional.
		/// </summary>
		public static readonly TypeScriptGeneratorOptions Default = new TypeScriptGeneratorOptions();

		public TypeScriptGeneratorOptions WithReferencePaths(params string [] referencePaths)
		{
			return new TypeScriptGeneratorOptions(
				referencePaths,
				OptionalMemberGenerationMode,
				GenerateStrictTypes,
				NotNullableTypeDiscovery,
				IncludedTypesDiscovery,
				EnumMemberNameMappingMode,
                IncludeAllDataContracts,
                AssemblyPath);
		}

		public TypeScriptGeneratorOptions WithOptionalMembers(
			OptionalMemberGenerationMode requiredMemberDiscovery)
		{
			return new TypeScriptGeneratorOptions(
				ReferencePaths,
				requiredMemberDiscovery,
				GenerateStrictTypes,
				NotNullableTypeDiscovery,
				IncludedTypesDiscovery,
				EnumMemberNameMappingMode,
                IncludeAllDataContracts,
                AssemblyPath);
		}

		public TypeScriptGeneratorOptions WithStrictTypes(
			NotNullableTypeDiscovery notNullableTypeDiscovery = NotNullableTypeDiscovery.None)
		{
			return new TypeScriptGeneratorOptions(
				ReferencePaths,
				OptionalMemberGenerationMode,
				true,
				notNullableTypeDiscovery,
				IncludedTypesDiscovery,
				EnumMemberNameMappingMode,
                IncludeAllDataContracts,
                AssemblyPath);
		}

		public TypeScriptGeneratorOptions WithIncludedTypes(
			IncludedTypesDiscovery includedTypesDiscovery)
		{
			return new TypeScriptGeneratorOptions(
				ReferencePaths,
				OptionalMemberGenerationMode,
				GenerateStrictTypes,
				NotNullableTypeDiscovery,
				includedTypesDiscovery,
				EnumMemberNameMappingMode,
                IncludeAllDataContracts,
                AssemblyPath);
		}
		public TypeScriptGeneratorOptions WithEnumMemberNameMappingMode(
			EnumMemberNameMappingMode enumMemberNameMappingMode)
		{
			return new TypeScriptGeneratorOptions(
				ReferencePaths,
				OptionalMemberGenerationMode,
				GenerateStrictTypes,
				NotNullableTypeDiscovery,
				IncludedTypesDiscovery,
				enumMemberNameMappingMode,
                IncludeAllDataContracts, 
                AssemblyPath);
		}

        public TypeScriptGeneratorOptions WithAllDataContracts()
        {
            return new TypeScriptGeneratorOptions(
                ReferencePaths,
                OptionalMemberGenerationMode,
                GenerateStrictTypes,
                NotNullableTypeDiscovery,
                IncludedTypesDiscovery,
                EnumMemberNameMappingMode,
                true,
                AssemblyPath);
        }

        public TypeScriptGeneratorOptions WithAssemblyPath(string assemblyPath)
        {
            return new TypeScriptGeneratorOptions(
                ReferencePaths,
                OptionalMemberGenerationMode,
                GenerateStrictTypes,
                NotNullableTypeDiscovery,
                IncludedTypesDiscovery,
                EnumMemberNameMappingMode,
                IncludeAllDataContracts,
                assemblyPath);
        }
    }
}
