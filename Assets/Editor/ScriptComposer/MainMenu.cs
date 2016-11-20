using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Linq;
using System.IO;
using EditorUtil;

namespace ScriptComposer
{
    public class MainMenu
    {
        //const string cItemName_Settings = "Assets/ScriptComposer/設定";
        //const string cItemName_Assemble = "Assets/ScriptComposer/スクリプトをビルド";
        //const string cItemName_Disassemble = "Assets/ScriptComposer/DLLをスクリプトに戻す";
        const string cItemName_Settings = "Assets/ScriptComposer_Settings";
        const string cItemName_Assemble = "Assets/ScriptComposer_Assemble";
        const string cItemName_Disassemble = "Assets/ScriptComposer_Disassemble";

        [MenuItem(cItemName_Settings)]
        public static void SelectSettingsObject()
        {
            Selection.activeObject = BuildSettings.FindOrCreateInstance();
        }

        [MenuItem(cItemName_Assemble)]
        public static void Assemble()
        {
            var scripts = Selection.objects.Select(obj => AssetDatabase.GetAssetPath(obj))
                .ToArray();

            using (new LockReloadAssemblyScope())
            using (new LockReloadAssetScope())
            {
                var settings = BuildSettings.FindOrCreateInstance();
                var composer = new Composer(settings);
                var outputPath = composer.BuildScripts(scripts);
                Debug.Log(outputPath);

                AssetDatabase.SaveAssets();
            }
        }

        [MenuItem(cItemName_Disassemble)]
        public static void Disassemble()
        {
        }

        [MenuItem(cItemName_Settings, true)]
        [MenuItem(cItemName_Assemble, true)]
        [MenuItem(cItemName_Disassemble, true)]
        public static bool CanApplyMenu()
        {
            var targets = Selection.objects;

            if (targets == null)
            {
                return false;
            }

            var scripts = targets.Where(obj => obj.GetType() == typeof(MonoScript))
                .Select(obj => AssetDatabase.GetAssetPath(obj))
                .ToArray();
            if (scripts.Length == 0)
            {
                return false;
            }

            return true;
        }

        private static string _monoPath;
    }
}
