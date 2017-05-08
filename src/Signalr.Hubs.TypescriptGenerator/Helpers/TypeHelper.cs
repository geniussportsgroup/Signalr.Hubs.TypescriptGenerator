using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GeniusSports.Signalr.Hubs.TypeScriptGenerator.Models;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using MemberInfo = System.Reflection.MemberInfo;

namespace GeniusSports.Signalr.Hubs.TypeScriptGenerator.Helpers
{
	internal class TypeHelper
	{
		private readonly HashSet<Type> doneTypes;
		public readonly Queue<Type> InterfaceTypes;
		public readonly HashSet<Type> EnumTypes;
		public readonly TypeScriptGeneratorOptions Options;

		public TypeHelper(TypeScriptGeneratorOptions options)
		{
			Options = options;
			doneTypes = new HashSet<Type>();
			InterfaceTypes = new Queue<Type>();
			EnumTypes = new HashSet<Type>();
		}

		public List<Models.MethodInfo> GetClientMethods(Type hubType)
		{
			var list = new List<Models.MethodInfo>();

			var clientType = ClientType(hubType);
			if (clientType != null)
			{
				foreach (var method in clientType.GetMethods())
				{
					var ps = method.GetParameters().Select(x => x.Name + " : " + GetTypeContractName(x.ParameterType));
					var functionName = FirstCharLowered(method.Name);
					var functionArgs = "(" + string.Join(", ", ps) + ")";

					string reasonDeprecated;
					var isDeprecated = method.IsDeprecated(out reasonDeprecated);

					list.Add(new Models.MethodInfo(functionName, isDeprecated, reasonDeprecated, functionArgs));
				}
			}

			return list;
		}

		public string FirstCharLowered(string s)
		{
			return Regex.Replace(s, "^.", x => x.Value.ToLowerInvariant());
		}

		public Type ClientType(Type hubType)
		{
			while (hubType != null && hubType != typeof(Hub))
			{
				if (hubType.IsGenericType && hubType.GetGenericTypeDefinition() == typeof(Hub<>))
				{
					return hubType.GetGenericArguments().Single();
				}
				hubType = hubType.BaseType;
			}
			return null;
		}

		/// <summary>
		/// This method formats the TypeScript name for the specified <see cref="Type"/>.
		/// If the type is recognized as nullable and the <see cref="TypeScriptGeneratorOptions.GenerateStrictTypes"/>
		/// option is set, the resulting type will be represented as union of the type and <c>null</c>.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public string GetTypeContractName(Type type)
		{
			return GetTypeContractInfo(type).Name;
		}

	    /// <summary>
	    /// This method formats the TypeScript name for the specified <see cref="Type"/>.
	    /// <note type="note">Nullable type is not appended with <c>|null</c>.</note>
	    /// </summary>
	    /// <param name="type"></param>
	    /// <param name="forceNotNullable">Instructs to ignore "nullability" of the specified <paramref name="type"/>.
	    /// </param>
	    /// <returns></returns>
	    private TypeInfo GetTypeContractInfo(Type type, bool forceNotNullable = false)
	    {
	        while (true)
	        {
	            if (type == typeof(Task))
	            {
	                return TypeInfo.Void;
	            }

	            if (type.IsArray)
	            {
	                var elementType = GetTypeContractInfo(type.GetElementType());
	                var arrayType = TypeInfo.Array(elementType);
	                return Nullable(arrayType, forceNotNullable);
	            }

	            if (type.IsGenericType)
	            {
	                if (typeof(Task<>).IsAssignableFrom(type.GetGenericTypeDefinition()))
	                {
	                    type = type.GetGenericArguments()[0];
	                    continue;
	                }

	                if (typeof(Nullable<>).IsAssignableFrom(type.GetGenericTypeDefinition()))
	                {
	                    var nestedType = GetTypeContractInfo(type.GetGenericArguments()[0]);
	                    return Nullable(nestedType, forceNotNullable);
	                }

	                if (typeof(IEnumerable<>).IsAssignableFrom(type.GetGenericTypeDefinition()))
	                {
	                    var elementType = GetTypeContractInfo(type.GetGenericArguments()[0]);
	                    var arrayType = TypeInfo.Array(elementType);
	                    return Nullable(arrayType, forceNotNullable);
	                }
	            }

	            if (type.Namespace == "System")
	            {
	                var typeNameLowerCase = type.Name.ToLowerInvariant();
	                // ReSharper disable once SwitchStatementMissingSomeCases
	                switch (typeNameLowerCase)
	                {
	                    case "datetime":
	                        return TypeInfo.Date;

	                    case "int16":
	                    case "int32":
	                    case "int64":
	                    case "single":
	                    case "double":
	                        return TypeInfo.Number;

	                    case "boolean":
	                        return TypeInfo.Boolean;

	                    case "void":
	                        return TypeInfo.Void;

	                    case "string":
	                        return Nullable(TypeInfo.String, forceNotNullable);

	                    case "guid":
	                        return TypeInfo.String;

	                    case "object":
	                        return TypeInfo.Any;
	                }
	            }

                if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
                {
                    return Nullable(TypeInfo.Array(TypeInfo.Any), forceNotNullable);
                }

                AddCustomType(type);
	            return TypeInfo.Simple(GenericSpecificName(type, true));
	        }
	    }

	    public void AddCustomType(Type type)
	    {
	        while (true)
	        {
	            if (type == null || type == typeof(ValueType) || type == typeof(object))
	                return;

	            if (!doneTypes.Add(type))
	                return; // Already handled.

	            if (type.IsEnum)
	            {
	                EnumTypes.Add(type);
	            }
	            else
	            {
	                InterfaceTypes.Enqueue(type);
	                type = type.BaseType;
	                continue;
	            }
	            break;
	        }
	    }

	    public void DiscoverAdditionalTypes(Type dataContractType)
		{
			switch (Options.IncludedTypesDiscovery)
			{
				case IncludedTypesDiscovery.None:
					break;

				case IncludedTypesDiscovery.UseKnownTypeAttribute:
					foreach (var knownType in GetKnownTypes(dataContractType))
					{
						AddCustomType(knownType);
					}
					break;

				default:
					throw new NotSupportedException($"Specified additional types discovery method is not supported: {Options.IncludedTypesDiscovery}.");
			}
		}

		private static IEnumerable<Type> GetKnownTypes(Type type)
		{
			foreach (var attribute in type.GetCustomAttributes<KnownTypeAttribute>(false))
			{
				if (!string.IsNullOrEmpty(attribute.MethodName))
				{
					var methodInfo = type.GetMethod(attribute.MethodName, 
						BindingFlags.Public|
						BindingFlags.NonPublic|
						BindingFlags.Static |
						BindingFlags.DeclaredOnly, 
						null, new Type[0], null);
					if (methodInfo == null)
						continue;

					Type[] knownTypes = null;
					try
					{
						knownTypes = methodInfo.Invoke(null, null) as Type[];
					}
					catch (Exception)
					{
						// Eat it.
					}
					if (knownTypes == null)
						continue;

					foreach (var knownType in knownTypes)
						yield return knownType;
				}
				else
				{
					yield return attribute.Type;
				}
			}
		}

		/// <summary>
		/// Returns <see langword="true"/> if specified <paramref name="type"/> is exactly <see cref="ValueType"/>
		/// or <see cref="Object"/>.
		/// </summary>
		/// <param name="type">Type reference.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">The <paramref name="type"/> is <see langword="null"/>.</exception>
		public bool IsRootBaseType(Type type)
		{
			if (type == null)
				throw new ArgumentNullException(nameof(type));
			return type == typeof(object) || type == typeof(ValueType);
		}

		/// <summary>
		/// If <see cref="TypeScriptGeneratorOptions.GenerateStrictTypes"/> option is specified and <paramref name="forceNotNullable"/> 
		/// is <see langword="false"/>, convers passed type into union of itself and <see cref="TypeInfo.Null"/>.
		/// </summary>
		/// <param name="typeInfo"></param>
		/// <param name="forceNotNullable"></param>
		/// <returns></returns>
		private TypeInfo Nullable(TypeInfo typeInfo, bool forceNotNullable)
		{
			return Options.GenerateStrictTypes && !forceNotNullable
				? TypeInfo.Union(typeInfo, TypeInfo.Null)
				: typeInfo;
		}

		public string GenericSpecificName(Type type, bool referencing)
		{
            var name = (referencing ? type.FullName : type.Name).Split('`').First();
            name = name.Replace("+", "."); // nested classes have + instead of .
            if (type.IsGenericType)
			{
				name += "_" + string.Join("_", type.GenericTypeArguments.Select(a => GenericSpecificName(a, false))) + "_";
			}
			return name;
		}

		public MemberTypeInfo GetPropertyInfo(PropertyInfo prop)
		{
			var propType = prop.PropertyType;
			var dataMemberAttribute = prop.GetCustomAttribute<DataMemberAttribute>();
			var modelName = string.IsNullOrWhiteSpace(dataMemberAttribute?.Name) ? prop.Name : dataMemberAttribute.Name;
			var modelType = GetTypeContractInfo(propType, IsNotNullableProperty(prop));

			string reasonDeprecated;
			var isDeprecated = prop.IsDeprecated(out reasonDeprecated);

			return new MemberTypeInfo(modelName, isDeprecated, reasonDeprecated, modelType.Name, IsOptionalProperty(prop));
		}

		private bool IsOptionalProperty(MemberInfo prop)
		{
			switch (Options.OptionalMemberGenerationMode)
			{
				case OptionalMemberGenerationMode.None:
					return false;

				case OptionalMemberGenerationMode.UseDataMemberAttribute:
					var dataMemberAttribute = prop.GetCustomAttribute<DataMemberAttribute>(true);
					return dataMemberAttribute != null && !dataMemberAttribute.IsRequired;

				default:
					throw new NotSupportedException($"Specified required member discovery option is not supported: {Options.OptionalMemberGenerationMode}.");
			}
		}

		private bool IsNotNullableProperty(PropertyInfo prop)
		{
			if (!Options.GenerateStrictTypes)
				return false;

			switch (Options.NotNullableTypeDiscovery)
			{
				case NotNullableTypeDiscovery.None:
					return false;

				case NotNullableTypeDiscovery.UseRequiredAttribute:
					return prop.IsDefined(typeof(RequiredAttribute));

				default:
					throw new NotSupportedException($"NotNullable types discovery method is not supported: {Options.NotNullableTypeDiscovery}");
			}
		}

		public MemberTypeInfo GetParameterInfo(ParameterDescriptor parameter)
		{
			var modelType = GetTypeContractInfo(parameter.ParameterType);
			return new MemberTypeInfo(parameter.Name, modelType.Name);
		}
	}
}