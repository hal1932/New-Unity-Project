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

            string assemblyPath;
            using (new LockReloadAssemblyScope())
            using (new LockReloadAssetScope())
            {
                var settings = BuildSettings.FindOrCreateInstance();
                var composer = new Composer(settings);
                assemblyPath = composer.BuildScripts(scripts);

                AssetDatabase.SaveAssets();
            }

            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(assemblyPath));
        }

        [MenuItem(cItemName_Disassemble)]
        public static void Disassemble()
        {
            var assemblyName = Path.GetFileNameWithoutExtension(
                AssetDatabase.GetAssetPath(Selection.activeObject));

            string[] scriptPaths;
            using (new LockReloadAssemblyScope())
            using (new LockReloadAssetScope())
            {
                var settings = BuildSettings.FindOrCreateInstance();
                var composer = new Composer(settings);
                scriptPaths = composer.RevertToScripts(settings, assemblyName);

                AssetDatabase.SaveAssets();
            }

            if (scriptPaths.Length == 1)
            {
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(scriptPaths[0]));
            }
            else
            {
                Selection.objects = scriptPaths.Select(x => AssetDatabase.LoadAssetAtPath<Object>(x))
                    .ToArray();
            }
        }

        [MenuItem(cItemName_Settings, true)]
        [MenuItem(cItemName_Assemble, true)]
        public static bool CanApplyScriptMenu()
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

        [MenuItem(cItemName_Settings, true)]
        [MenuItem(cItemName_Disassemble, true)]
        public static bool CanApplyAssemblyMenu()
        {
            var target = Selection.activeObject;

            if (target == null)
            {
                return false;
            }

            if (target.GetType() != typeof(DefaultAsset))
            {
                return false;
            }

            return AssetDatabase.GetAssetPath(target).EndsWith(".dll");
        }
    }
}
