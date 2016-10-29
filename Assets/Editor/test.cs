using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Reflection;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System;
using System.Collections.Generic;
using System.IO;

public class test : EditorWindow
{
    [MenuItem("test/aaa")]
    public static void Open()
    {
        GetWindow<test>();
    }

    private void OnGUI()
    {
        if (GUILayout.Button("aaa"))
        {
#if UNITY_EDITOR_WIN
            var references = new[]
            {
                @"D:\Program Files\Unity\Editor\Data\Managed\UnityEngine.dll",
                @"D:\Program Files\Unity\Editor\Data\Managed\UnityEditor.dll",
            };
#elif UNITY_EDITOR_OSX
#endif

            var param = new CompilerParameters(references)
            {
                GenerateExecutable = false,
                GenerateInMemory = true,
                IncludeDebugInformation = false,
                WarningLevel = 4,
            };

            var provider = new CSharpCodeProvider(
                new Dictionary<string, string>() { { "CompilerVersion", "v3.5" } });

            var sources = new[]
            {
                string.Join(Path.DirectorySeparatorChar.ToString(), new[] { Application.dataPath, "..", "Class1.cs" }),
            };
            var result = provider.CompileAssemblyFromFile(param, sources);
            if (result.Errors.Count > 0)
            {
                foreach (var err in result.Errors)
                {
                    Debug.LogError(err);
                }
            }
            else
            {
                var class1 = result.CompiledAssembly.GetType("Class1");
                var instance = class1.GetConstructor(Type.EmptyTypes).Invoke(null);
                class1.GetMethod("Test").Invoke(instance, null);
            }
        }
    }
}
