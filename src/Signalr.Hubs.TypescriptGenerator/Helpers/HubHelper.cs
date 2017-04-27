using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using GeniusSports.Signalr.Hubs.TypeScriptGenerator.Models;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace GeniusSports.Signalr.Hubs.TypeScriptGenerator.Helpers
{
	internal class HubHelper
	{
		private readonly DefaultHubManager hubmanager;
		private readonly TypeHelper typeHelper;


		public HubHelper(TypeScriptGeneratorOptions options)
		{
			typeHelper = new TypeHelper(options);

			var defaultDependencyResolver = new DefaultDependencyResolver();
			hubmanager = new DefaultHubManager(defaultDependencyResolver);
		}

		public List<MemberTypeInfo> GetHubs()
		{
			return hubmanager
				.GetHubs()
				.Select(hub =>
				{
					string reasonDeprecated;
					var isDeprecated = hub.HubType.IsDeprecated(out reasonDeprecated);
					return new MemberTypeInfo(
						hub.NameSpecified ? hub.Name : typeHelper.FirstCharLowered(hub.Name),
						isDeprecated, reasonDeprecated, hub.HubType.FullName, false);
				}).ToList();
	}

		public List<ServiceInfo> GetServiceContracts()
		{
			var list = new List<ServiceInfo>();

			foreach (var hub in hubmanager.GetHubs())
			{
				var hubMethods = hubmanager.GetHubMethods(hub.Name).Select(GetServiceMethod).ToList();

				var hubType = hub.HubType;
				var clientType = typeHelper.ClientType(hubType);

				string reasonDeprecated;
				var isDeprecated = hub.HubType.IsDeprecated(out reasonDeprecated);

				list.Add(new ServiceInfo(
					hubType.Name, isDeprecated, reasonDeprecated, hubType.Namespace,
					clientType != null ? clientType.FullName : "any",
					hubType.Name + "Server", hubMethods));
			}

			return list;
		}

		private Models.MethodInfo GetServiceMethod(MethodDescriptor method)
		{
			var methodParametersString = string.Join(", ", method.Parameters
				.Select(typeHelper.GetParameterInfo)
				.Select(x => $"{x.DeclaredName} : {x.TypeScriptType}"));

			string reasonDeprecated;
			var isDeprecated = method.IsDeprecated(out reasonDeprecated);

			return new Models.MethodInfo(
				typeHelper.FirstCharLowered(method.Name), isDeprecated, reasonDeprecated,
				$"({methodParametersString})",
				$"JQueryPromise<{typeHelper.GetTypeContractName(method.ReturnType)}>");
		}

		public List<ClientInfo> GetClients()
		{
			return (from hub in hubmanager.GetHubs()
					select hub.HubType into hubType
					let clientType = typeHelper.ClientType(hubType)
					where clientType != null
					select new ClientInfo(clientType.Name, clientType.Namespace, typeHelper.GetClientMethods(hubType))).ToList();
		}

		public List<DataContractInfo> GetDataContracts()
		{
			var list = new List<DataContractInfo>();

			while (typeHelper.InterfaceTypes.Count != 0)
			{
				var type = typeHelper.InterfaceTypes.Dequeue();
				var baseType = type.BaseType;
				var isBaseTypeDefined = baseType != null && !typeHelper.IsRootBaseType(baseType);

				var declaredProperties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
				if (isBaseTypeDefined)
				{
					var derivedProperties = baseType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
					var derivedPropertyNames = derivedProperties.Select(p => p.Name).ToList();
					declaredProperties = declaredProperties.Where(p => !derivedPropertyNames.Contains(p.Name)).ToArray();
				}

				var properties = declaredProperties.Select(prop => typeHelper.GetPropertyInfo(prop)).ToList();
				var bases = isBaseTypeDefined ? new[] { typeHelper.GenericSpecificName(baseType, true) } : new string[0];

				string reasonDeprecated;
				var isDeprecated = type.IsDeprecated(out reasonDeprecated);
				list.Add(new DataContractInfo(typeHelper.GenericSpecificName(type, false), isDeprecated, reasonDeprecated, type.Namespace, bases, properties));
				typeHelper.DiscoverAdditionalTypes(type);
			}

			return list;
		}

		public List<EnumInfo> GetEnums()
		{
			var list = new List<EnumInfo>();

			foreach (var type in typeHelper.EnumTypes)
			{
				var enumProperties = Enum.GetNames(type).Select(memberName => GetEnumMemberInfo(type, memberName)).ToList();

				string reasonDeprecated;
				var isDeprecated = type.IsDeprecated(out reasonDeprecated);

				list.Add(new EnumInfo(
					typeHelper.GenericSpecificName(type, false), isDeprecated, reasonDeprecated, type.Namespace, 
					enumProperties));
			}

			return list;
		}

		private EnumMemberInfo GetEnumMemberInfo(Type enumType, string memberName)
		{
			var enumMember = enumType.GetField(memberName);

			string reasonDeprecated;
			var isDeprecated = enumMember.IsDeprecated(out reasonDeprecated);

			switch (typeHelper.Options.EnumMemberNameMappingMode)
			{
				case EnumMemberNameMappingMode.MemberName:
					// Take memberName as is.
					break;

				case EnumMemberNameMappingMode.MemberNameCamelCase:
					memberName = char.ToLowerInvariant(memberName[0]) + memberName.Substring(1);
					break;

				case EnumMemberNameMappingMode.MemberNameLowerCase:
					memberName = memberName.ToLowerInvariant();
					break;

				case EnumMemberNameMappingMode.MemberNameUpperCase:
					memberName = memberName.ToUpperInvariant();
					break;

				case EnumMemberNameMappingMode.EnumMemberAttributeValue:
				{
					var attr = enumMember.GetCustomAttribute<EnumMemberAttribute>();
					if (!string.IsNullOrWhiteSpace(attr?.Value))
						memberName = attr.Value;
					break;
				}
			}

			return new EnumMemberInfo(
				memberName, isDeprecated, reasonDeprecated,
				enumMember.GetRawConstantValue());
		}
	}
}