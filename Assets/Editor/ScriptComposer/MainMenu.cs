using EditorUtil;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

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
            var paths = Selection.objects.Select(obj => AssetDatabase.GetAssetPath(obj));

            var directories = paths.Where(path => AssetDatabase.IsValidFolder(path)).ToArray();
            var scripts = paths.Where(x => x.EndsWith("*.cs"))
                .Union(
                    AssetDatabase.FindAssets("t:script", directories)
                        .Select(guid => AssetDatabase.GUIDToAssetPath(guid)))
                .ToArray();

            var assemblyName = Path.GetFileNameWithoutExtension(paths.First());

            string assemblyPath;
            using (new LockReloadAssemblyScope())
            using (new LockReloadAssetScope())
            {
                var settings = BuildSettings.FindOrCreateInstance();
                var composer = new Composer(settings);
                assemblyPath = composer.BuildScripts(scripts, assemblyName);

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
        public static bool CanApplyAssembleMenu()
        {
            var targets = Selection.objects;

            if (targets == null)
            {
                return false;
            }

            var paths = targets.Select(obj => AssetDatabase.GetAssetPath(obj))
                .ToArray();

            if (paths.Any(path => AssetDatabase.IsValidFolder(path)))
            {
                return true;
            }

            return paths.Any(path => path.EndsWith(".cs"));
        }

        [MenuItem(cItemName_Settings, true)]
        [MenuItem(cItemName_Disassemble, true)]
        public static bool CanApplyDisassembleMenu()
        {
            var target = Selection.activeObject;

            if (target == null)
            {
                return false;
            }

            return AssetDatabase.GetAssetPath(target).EndsWith(".dll");
        }
    }
}
