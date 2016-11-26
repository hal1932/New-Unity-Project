using EditorUtil;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public static AssemblyNameSelection AssemblyNameSelection { get; private set; }
        public static string ExportDirectory { get; private set; }

        [InitializeOnLoadMethod]
        public static void Initialize()
        {
            AssemblyNameSelection = (AssemblyNameSelection)PlayerPrefs.GetInt("ScriptComposer_AssemblyNameSelection");
            ExportDirectory = PlayerPrefs.GetString("ScriptComposer_ExportDirectory");
        }

        [PreferenceItem("Script Composer")]
        public static void PreferenceEditor()
        {
            using (new ChangeCheckScope(() => PlayerPrefs.SetInt("ScriptComposer_AssemblyNameSelection", (int)AssemblyNameSelection)))
            {
                AssemblyNameSelection = (AssemblyNameSelection)EditorGUILayout.EnumPopup(
                    "アセンブリ名の決定方法", AssemblyNameSelection);
            }

            using (new ChangeCheckScope(() => PlayerPrefs.SetString("ScriptComposer_ExportDirectory", ExportDirectory)))
            {
                ExportDirectory = EditorGUILayout.TextField("エクスポート先", ExportDirectory);
            }
        }

        public static string GetAssemblyName(IEnumerable<string> files)
        {
            switch (AssemblyNameSelection)
            {
                case AssemblyNameSelection.FirstSelection:
                    return Path.GetFileNameWithoutExtension(files.First());

                case AssemblyNameSelection.LastSelection:
                    return Path.GetFileNameWithoutExtension(files.Last());

                default:
                    throw new System.Exception("invalid Preference.AssemblyNameSelection");
            }
        }
    }
}
