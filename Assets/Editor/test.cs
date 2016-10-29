using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Reflection;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ScriptImporter;

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
            using (var context = new ScriptContext())
            {
                if (context.LoadDirectory(Path.Combine(Application.dataPath, ".."), SearchOption.TopDirectoryOnly))
                {
                    context.CompiledAssembly.InvokeMethod("Class1", "Test", null);
                }
                else
                {
                    foreach (var err in context.CompileErros)
                    {
                        Debug.Log(err);
                    }
                }
            }
        }
    }
}
