namespace GeniusSports.Signalr.Hubs.TypeScriptGenerator
{
	/// <summary>
	/// Defines supported methods for mapping .NET model enum member name to name of enum member in generated typings.
	/// </summary>
	public enum EnumMemberNameMappingMode
	{
		/// <summary>
		/// Use enum member name as defined (Default).
		/// </summary>
		MemberName,

		/// <summary>
		/// Use enum member name with first letter in lower case (camel-case).
		/// </summary>
		MemberNameCamelCase,

		/// <summary>
		/// Use enum member name converted to lower case.
		/// </summary>
		MemberNameLowerCase,

		/// <summary>
		/// Use enum member name converted to upper case.
		/// </summary>
		MemberNameUpperCase,

		/// <summary>
		/// Use the value of the <see cref="System.Runtime.Serialization.EnumMemberAttribute"/> if applied to enum meber.
		/// If the attribute is not defined, fall back to <see cref="MemberName"/> option.
		/// </summary>
		EnumMemberAttributeValue
	}
}