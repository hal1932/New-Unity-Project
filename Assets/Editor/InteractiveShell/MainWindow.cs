using UnityEngine;
using System.Collections;
using UnityEditor;
using Mono.CSharp;
using System.Reflection;
using System.IO;
using System.Linq;

public class MainWindow : EditorWindow
{
    [MenuItem("Shell/Interactive")]
    public static void OpenWindow()
    {
        GetWindow<MainWindow>().Show();
    }

    public void Awake()
    {
        ClearContext();
    }

    public void OnGUI()
    {
        if (GUILayout.Button("clear", EditorStyles.toolbarButton))
        {
            ClearContext();
            _sourceCode = string.Empty;
        }

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

    private void ClearContext()
    {
        Evaluator.Init(new string[] { });
        Evaluator.ReferenceAssembly(typeof(UnityEngine.Object).Assembly);
        Evaluator.ReferenceAssembly(typeof(UnityEditor.Editor).Assembly);
    }

    private void Evaluate()
    {
        if (string.IsNullOrEmpty(_sourceCode))
        {
            return;
        }

        Evaluator.MessageOutput = new StringWriter();

        Evaluator.Run("using UnityEngine; using UnityEditor; using System.Linq;");
        var result = Evaluator.Evaluate(_sourceCode);
        if (result != null)
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

        var output = Evaluator.MessageOutput.ToString();
        if (!string.IsNullOrEmpty(output))
        {
            Debug.LogError(output);
        }
    }

    private string _sourceCode;
    private Vector2 _scrollPosition;
}
