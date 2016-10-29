using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ScriptImporter
{
    public static class AssemblyExtensions
    {
        public static object ConstructInstance(this Assembly assembly, string className, params object[] args)
        {
            var argTypes = (args.Length > 0) ? args.Select(x => x.GetType()).ToArray() : Type.EmptyTypes;
            return assembly.GetType(className)
                .GetConstructor(argTypes)
                .Invoke(args);
        }

        public static object InvokeMethod(this Assembly assembly, string className, string methodName, params object[] args)
        {
            var type = assembly.GetType(className);
            var instance = assembly.ConstructInstance(className);
            return type.GetMethod(methodName)
                .Invoke(instance, args);
        }

        public static object InvokeStaticMethod(this Assembly assembly, string className, string methodName, params object[] args)
        {
            return assembly.GetType(className)
                .GetMethod(methodName)
                .Invoke(null, args);
        }
    }
}
