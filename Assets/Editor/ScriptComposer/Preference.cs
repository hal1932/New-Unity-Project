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
        public static AssemblyNameSelection AssemblyNameSelection { get; private set; }

        [PreferenceItem("Script Composer")]
        public static void PreferenceEditor()
        {
            AssemblyNameSelection = (AssemblyNameSelection)EditorGUILayout.EnumPopup(
                "アセンブリ名の決定方法", AssemblyNameSelection);
        }
    }
}
