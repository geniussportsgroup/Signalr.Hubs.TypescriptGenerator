using NUnit.Framework;

namespace GeniusSports.Signalr.Hubs.TypeScriptGenerator.SampleUsage
{
	[TestFixture]
	public class GenerateHubTypeScript
	{
		[Test]
		public void GenerateWithStrictTypesAndOptionalMembersAndTypeDiscovery()
		{
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

			System.Console.WriteLine(typeScript.Item1);
			System.Console.WriteLine(typeScript.Item2);
			Assert.Pass();
		}
	}
}