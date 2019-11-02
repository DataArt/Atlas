using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DataArt.Atlas.Core.Extensions
{
    /// <summary>
    ///     Extension to manipulate type and assembly information.
    /// </summary>
    public static class ReflectionExtensions
    {
        public static IEnumerable<Type> GetBaseTypes(this Type type)
        {
            var typeInfo = type.GetTypeInfo();

            foreach (var implementedInterface in typeInfo.ImplementedInterfaces)
            {
                yield return implementedInterface;
            }

            var baseType = typeInfo.BaseType;

            while (baseType != null)
            {
                var baseTypeInfo = baseType.GetTypeInfo();

                yield return baseType;

                baseType = baseTypeInfo.BaseType;
            }
        }

        public static Type GetRegistrationType(this Type interfaceType, TypeInfo typeInfo)
        {
            if (typeInfo.IsGenericTypeDefinition)
            {
                var interfaceTypeInfo = interfaceType.GetTypeInfo();

                if (interfaceTypeInfo.IsGenericType)
                {
                    return interfaceType.GetGenericTypeDefinition();
                }
            }

            return interfaceType;
        }

        public static bool HasAttribute(this Type type, Type attributeType)
        {
            return type.GetTypeInfo().IsDefined(attributeType, true);
        }

        public static bool HasAttribute<T>(this Type type, Func<T, bool> predicate)
            where T : Attribute
        {
            return type.GetTypeInfo().GetCustomAttributes<T>(true).Any(predicate);
        }

        public static bool HasMatchingGenericArity(this Type interfaceType, TypeInfo typeInfo)
        {
            if (typeInfo.IsGenericType)
            {
                var interfaceTypeInfo = interfaceType.GetTypeInfo();

                if (interfaceTypeInfo.IsGenericType)
                {
                    var argumentCount = interfaceType.GenericTypeArguments.Length;
                    var parameterCount = typeInfo.GenericTypeParameters.Length;

                    return argumentCount == parameterCount;
                }

                return false;
            }

            return true;
        }

        public static bool IsAssignableTo(this Type type, Type otherType)
        {
            var typeInfo = type.GetTypeInfo();
            var otherTypeInfo = otherType.GetTypeInfo();

            if (otherTypeInfo.IsGenericTypeDefinition)
            {
                return typeInfo.IsAssignableToGenericTypeDefinition(otherTypeInfo);
            }

            return otherTypeInfo.IsAssignableFrom(typeInfo);
        }

        public static bool IsAssignableToAny(this Type type, params Type[] otherTypes)
        {
            return otherTypes.Select(type.IsAssignableTo).Any();
        }

        public static bool IsInExactNamespace(this Type type, string @namespace)
        {
            return string.Equals(type.Namespace, @namespace, StringComparison.Ordinal);
        }

        public static bool IsInNamespace(this Type type, string @namespace)
        {
            var typeNamespace = type.Namespace ?? string.Empty;

            if (@namespace.Length > typeNamespace.Length)
            {
                return false;
            }

            var typeSubNamespace = typeNamespace.Substring(0, @namespace.Length);

            if (typeSubNamespace.Equals(@namespace, StringComparison.Ordinal))
            {
                if (typeNamespace.Length == @namespace.Length)
                {
                    // exactly the same
                    return true;
                }

                return typeNamespace[@namespace.Length] == '.';
            }

            return false;
        }

        public static bool IsNonAbstractClass(this Type type, bool publicOnly)
        {
            var typeInfo = type.GetTypeInfo();

            if (typeInfo.IsClass && !typeInfo.IsAbstract)
            {
                if (typeInfo.IsDefined(typeof(CompilerGeneratedAttribute), true))
                {
                    return false;
                }

                if (publicOnly)
                {
                    return typeInfo.IsPublic || typeInfo.IsNestedPublic;
                }

                return true;
            }

            return false;
        }

        public static bool IsOpenGeneric(this Type type)
        {
            return type.GetTypeInfo().IsGenericTypeDefinition;
        }

        public static IEnumerable<Type> Scan(this Assembly asm, Func<Type, bool> filter)
        {
            return asm.GetExportedTypes().Where(filter);
        }

        public static IEnumerable<Type> ScanAssembly(this Type type, Func<Type, bool> filter)
        {
            return type.Assembly.Scan(filter);
        }

        private static IEnumerable<Type> FilterMatchingGenericInterfaces(TypeInfo typeInfo)
        {
            var genericTypeParameters = typeInfo.GenericTypeParameters;

            foreach (var current in typeInfo.ImplementedInterfaces)
            {
                var currentTypeInfo = current.GetTypeInfo();

                if (currentTypeInfo.IsGenericType && currentTypeInfo.ContainsGenericParameters
                                                  && GenericParametersMatch(genericTypeParameters, currentTypeInfo.GenericTypeArguments))
                {
                    yield return currentTypeInfo.GetGenericTypeDefinition();
                }
            }
        }

        private static bool GenericParametersMatch(IReadOnlyList<Type> parameters, IReadOnlyList<Type> interfaceArguments)
        {
            if (parameters.Count != interfaceArguments.Count)
            {
                return false;
            }

            for (var i = 0; i < parameters.Count; i++)
            {
                if (parameters[i] != interfaceArguments[i])
                {
                    return false;
                }
            }

            return true;
        }

        private static IEnumerable<Type> GetImplementedInterfacesToMap(TypeInfo typeInfo)
        {
            if (!typeInfo.IsGenericType)
            {
                return typeInfo.ImplementedInterfaces;
            }

            if (!typeInfo.IsGenericTypeDefinition)
            {
                return typeInfo.ImplementedInterfaces;
            }

            return FilterMatchingGenericInterfaces(typeInfo);
        }

        private static bool IsAssignableToGenericTypeDefinition(this TypeInfo typeInfo, TypeInfo genericTypeInfo)
        {
            while (true)
            {
                var interfaceTypes = typeInfo.ImplementedInterfaces.Select(t => t.GetTypeInfo());

                foreach (var interfaceType in interfaceTypes)
                {
                    if (!interfaceType.IsGenericType)
                    {
                        continue;
                    }

                    var typeDefinitionTypeInfo = interfaceType
                                                 .GetGenericTypeDefinition()
                                                 .GetTypeInfo();

                    if (typeDefinitionTypeInfo.Equals(genericTypeInfo))
                    {
                        return true;
                    }
                }

                if (typeInfo.IsGenericType)
                {
                    var typeDefinitionTypeInfo = typeInfo
                                                 .GetGenericTypeDefinition()
                                                 .GetTypeInfo();

                    if (typeDefinitionTypeInfo.Equals(genericTypeInfo))
                    {
                        return true;
                    }
                }

                var baseTypeInfo = typeInfo.BaseType?.GetTypeInfo();

                if (baseTypeInfo is null)
                {
                    return false;
                }

                typeInfo = baseTypeInfo;
            }
        }
    }
}