using EditorUtil;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ScriptComposer
{
    public class BuildSettings : ScriptableObject
    {
        public string AssemblyRoot;
        public string SourceCodeCacheRoot;

        public static BuildSettings FindOrCreateInstance()
        {
            var thisPath = AssetUtil.FindScriptPath<BuildSettings>();
            var settingsPath = string.Format("{0}/settings.asset", Path.GetDirectoryName(thisPath));

            BuildSettings item;
            if (!ScriptableObjectUtil.FindOrCreateObject(out item, settingsPath))
            {
                item.AssemblyRoot = "Assets/Plugins";
                item.SourceCodeCacheRoot = "Assets/Plugins/sources";

                AssetDatabase.CreateAsset(item, settingsPath);
                AssetDatabase.SaveAssets();

            }

            Selection.activeObject = item;
            return item;
        }

        public void OnValidate()
        {
            AssemblyRoot = AssemblyRoot.Trim('/');
            SourceCodeCacheRoot = SourceCodeCacheRoot.Trim('/');
        }
    }

    [CustomEditor(typeof(BuildSettings))]
    public class BuildSettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            GUILayout.Label("Edit/Preference/Script ComposerにMonoディレクトリを設定してください");
            base.OnInspectorGUI();
        }
    }
}
