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
            var thisType = typeof(BuildSettings);

            var settingsPath = AssetDatabase.FindAssets("t:" + thisType.FullName)
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .FirstOrDefault();

            if (settingsPath == null)
            {
                var targetPath = string.Format(thisType.FullName.Replace('.', '/') + ".cs");
                var thisPath = AssetDatabase.FindAssets("t:Script " + thisType.Name)
                    .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                    .First(path => path.EndsWith(targetPath));

                settingsPath = string.Format("{0}/settings.asset", Path.GetDirectoryName(thisPath));

                var item = CreateInstance<BuildSettings>();
                item.AssemblyRoot = "Assets/Plugins";
                item.SourceCodeCacheRoot = "Assets/Plugins/sources";

                AssetDatabase.CreateAsset(item, settingsPath);
                AssetDatabase.SaveAssets();

                Selection.activeObject = item;
            }

            return AssetDatabase.LoadAssetAtPath<BuildSettings>(settingsPath);
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
