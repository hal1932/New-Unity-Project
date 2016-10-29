using ScriptImporter;
using System.IO;
using UnityEditor;
using UnityEngine;

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
                        Debug.LogError(err);
                    }
                }

                if (!context.BuildDirectory(@"test.dll", Path.Combine(Application.dataPath, ".."), SearchOption.TopDirectoryOnly))
                {
                    foreach (var err in context.CompileErros)
                    {
                        Debug.LogError(err);
                    }
                }
            }
        }
    }
}
