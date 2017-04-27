
using System;
using System.Runtime.Serialization;

namespace GeniusSports.Signalr.Hubs.TypeScriptGenerator.SampleUsage.DataContracts
{
	[Obsolete]
	[DataContract]
	public enum SomethingEnum
	{
		[EnumMember]
		One = 101,

		[EnumMember]
		Two = 202,

		[Obsolete("Do not use this value. Defined for backward compatibility.")]
		[EnumMember(Value = "oneAndTwo")]
		Three = One + Two
	}
}