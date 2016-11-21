using System.IO;
using UnityEditor;
using UnityEngine;

namespace ScriptComposer
{
    public enum AssemblyNameSelection
    {
        FirstSelection,
        LastSelection,
    }

    static class Preference
    {
        public static string MonoDirectory { get; private set; }
        public static AssemblyNameSelection AssemblyNameSelection { get; private set; }

        static Preference()
        {
            MonoDirectory = EditorUserSettings.GetConfigValue("MONO_DIR");
        }

        [PreferenceItem("Script Composer")]
        public static void PreferenceEditor()
        {
            var monoDir = MonoDirectory;

            using (new EditorGUILayout.HorizontalScope())
            {
                monoDir = EditorGUILayout.TextField("Mono Directory", monoDir);
                if (GUILayout.Button("選択", GUILayout.Width(35)))
                {
                    var defaultDir = string.Empty;
                    var defaultName = string.Empty;
                    if (Directory.Exists(monoDir))
                    {
                        defaultDir = Path.GetDirectoryName(monoDir);
                        defaultName = Path.GetFileName(monoDir);
                    }

                    var tmp = EditorUtility.OpenFolderPanel("Monoフォルダを選択してください", defaultDir, defaultName);
                    if (monoDir != tmp && !string.IsNullOrEmpty(tmp))
                    {
                        monoDir = tmp;
                    }
                }
            }

            if (MonoDirectory != monoDir)
            {
                MonoDirectory = monoDir.TrimEnd('/');
                EditorUserSettings.SetConfigValue("MONO_DIR", MonoDirectory);
            }

            AssemblyNameSelection = (AssemblyNameSelection)EditorGUILayout.EnumPopup(
                "アセンブリ名の決定方法", AssemblyNameSelection);
        }
    }
}
