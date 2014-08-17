using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TechFellow.InitializableModule
{
    // TODO: sounds like this should be extracted into TechFellow.Common.Metadata or smth
    public class TypeHelper
    {
        public static T GetAttribute<T>(Type type)
        {
            return FilterAttribute<T>(type.GetCustomAttributes(true), a => type.IsInstanceOfType(a)).FirstOrDefault();
        }

        public static IEnumerable<TAttr> GetAttributes<TAttr>(Type type)
        {
            return FilterAttribute<TAttr>(type.GetCustomAttributes(true), _ => true);
        }

        public static IEnumerable<Type> GetTypesChildOf<T>()
        {
            var allTypes = new List<Type>();
            foreach (var assembly in GetAssemblies())
            {
                allTypes.AddRange(GetTypesChildOfInAssembly(typeof (T), assembly));
            }

            return allTypes;
        }

        public static IEnumerable<Type> GetTypesImplementingInterface<T>()
        {
            var allTypes = new List<Type>();
            foreach (var assembly in GetAssemblies())
            {
                allTypes.AddRange(GetTypesImplementingInterfaceInAssembly(typeof (T), assembly));
            }

            return allTypes;
        }

        public static IEnumerable<KeyValuePair<Type, T>> GetTypesWithAttribute<T>()
        {
            var allTypes = new List<KeyValuePair<Type, T>>();
            foreach (var assembly in GetAssemblies())
            {
                allTypes.AddRange(GetTypesWithAttributeInAssembly<T>(assembly));
            }

            return allTypes;
        }

        public static bool TypeHasAttribute(Type type, Type attributeType)
        {
            return type.GetCustomAttributes(true).Any(attributeType.IsInstanceOfType);
        }

        private static IEnumerable<T> FilterAttribute<T>(IEnumerable<object> attributes, Func<T, bool> filter)
        {
            return attributes.OfType<T>().Where(filter);
        }

        private static IEnumerable<Assembly> GetAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies();
        }

        private static IEnumerable<Type> GetTypesChildOfInAssembly(Type type, Assembly assembly)
        {
            return SelectTypes(assembly, t => t.IsSubclassOf(type) && !t.IsAbstract);
        }

        private static IEnumerable<Type> GetTypesImplementingInterfaceInAssembly(Type @interface, Assembly assembly)
        {
            return SelectTypes(assembly, t => !t.IsAbstract && t.GetInterfaces().AsEnumerable().Contains(@interface));
        }

        private static IEnumerable<KeyValuePair<Type, T>> GetTypesWithAttributeInAssembly<T>(Assembly assembly)
        {
            return SelectTypes(assembly,
                t => TypeHasAttribute(t, typeof (T)) && !t.IsAbstract,
                t => new KeyValuePair<Type, T>(t, GetAttribute<T>(t)));
        }

        private static IEnumerable<Type> SelectTypes(Assembly assembly, Func<Type, bool> filter)
        {
            try
            {
                return assembly.GetTypes().Where(filter);
            }
            catch (Exception)
            {
                // there could be situations when type could not be loaded
                // this may happen if we are visiting *all* loaded assemblies in app domain
                return new List<Type>();
            }
        }

        private static IEnumerable<KeyValuePair<Type, T>> SelectTypes<T>(
            Assembly assembly,
            Func<Type, bool> filter,
            Func<Type, KeyValuePair<Type, T>> projection)
        {
            try
            {
                return
                    assembly.GetTypes().Where(filter).Select(projection);
            }
            catch (Exception)
            {
                // there could be situations when type could not be loaded
                // this may happen if we are visiting *all* loaded assemblies in app domain
                return new List<KeyValuePair<Type, T>>();
            }
        }
    }
}
