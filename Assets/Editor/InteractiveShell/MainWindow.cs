using UnityEngine;
using System.Collections;
using UnityEditor;
using Mono.CSharp;
using System.Reflection;
using System.IO;
using System.Linq;
using System;
using System.Text;

public class MainWindow : EditorWindow
{
    [MenuItem("Shell/Interactive")]
    public static void OpenWindow()
    {
        GetWindow<MainWindow>().Show();
    }

    [InitializeOnLoadMethod]
    public static void Initialize()
    {
        EditorApplication.update += Update;
    }

    private static void Update()
    {
        if (EditorApplication.isCompiling)
        {
            return;
        }

        // https://github.com/hecomi/uREPL/blob/master/Assets/uREPL/Scripts/Core/Evaluator.cs
        //Evaluator.Init(new string[] { });
        for (var i = 0; i < 2; ++i)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly != null)
                {
                    Evaluator.ReferenceAssembly(assembly);
                }
            }
            Evaluator.Evaluate("null;");
        }

        Evaluator.Run("using UnityEngine; using UnityEditor; using System.Linq;");

        EditorApplication.update -= Update;
    }

    public void OnGUI()
    {
        var height = position.height - 10;
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(height));
        _sourceCode = EditorGUILayout.TextArea(_sourceCode, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();

        if (Event.current.type == EventType.KeyDown)
        {
            if (Event.current.control && Event.current.keyCode == KeyCode.Return)
            {
                Evaluate();
            }
        }
    }

    private void Evaluate()
    {
        if (string.IsNullOrEmpty(_sourceCode))
        {
            return;
        }

        var sourceCode = new StringBuilder();
        foreach (var line in _sourceCode.Split('\n').Select(x => x.Trim()))
        {
            if (line.StartsWith("using"))
            {
                Evaluator.Run(line);
            }
            else
            {
                sourceCode.AppendLine(line);
            }
        }
        sourceCode.Append(";");

        Evaluator.MessageOutput = new StringWriter();

        object result;
        bool isResultSet;
        Evaluator.Evaluate(sourceCode.ToString(), out result, out isResultSet);

        if (isResultSet)
        {
            var type = result.GetType();
            if (type.IsArray)
            {
                foreach (var item in (object[])result)
                {
                    Debug.Log(item);
                }
            }
            else if (type.DeclaringType == typeof(Enumerable))
            {
                foreach (var item in (IEnumerable)result)
                {
                    Debug.Log(item);
                }
            }
            else
            {
                Debug.Log(result);
            }
        }

        Evaluator.MessageOutput.Flush();
        var output = Evaluator.MessageOutput.ToString();
        if (!string.IsNullOrEmpty(output))
        {
            Debug.LogError(output);
        }
    }

    private string _sourceCode;
    private Vector2 _scrollPosition;
}
