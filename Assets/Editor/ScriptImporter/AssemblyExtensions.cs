using System;
using System.Linq;
using System.Reflection;

namespace ScriptImporter
{
    public static class AssemblyExtensions
    {
        public static object ConstructInstance(this Assembly assembly, string typeName, params object[] args)
        {
            var argTypes = (args.Length > 0) ? args.Select(x => x.GetType()).ToArray() : Type.EmptyTypes;
            return assembly.GetType(typeName)
                .GetConstructor(argTypes)
                .Invoke(args);
        }

        public static object InvokeMethod(this Assembly assembly, string typeName, string methodName, params object[] args)
        {
            var type = assembly.GetType(typeName);
            var instance = assembly.ConstructInstance(typeName);
            return type.GetMethod(methodName)
                .Invoke(instance, args);
        }

        public static object InvokeStaticMethod(this Assembly assembly, string typeName, string methodName, params object[] args)
        {
            return assembly.GetType(typeName)
                .GetMethod(methodName)
                .Invoke(null, args);
        }
    }
}
