using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using EditorUtil;
using System.Collections.Generic;
using System.Linq;

namespace ExcelLocalization
{
    public class MainMenu
    {
        const string cItemName_ImportExcel = "Assets/エクセルファイルをインポート";
        const string cItemName_BeginWatchExcel = "Assets/エクセルファイルの監視を開始する";
        const string cItemName_EndWatchExcel = "Assets/エクセルファイルの監視を停止する";

        [MenuItem(cItemName_ImportExcel)]
        public static void ImportExcel()
        {
            var inputFile = AssetDatabase.GetAssetPath(Selection.activeObject);
            var outputDirectory = AssetUtil.CombinePath(
                Path.GetDirectoryName(inputFile),
                Path.GetFileNameWithoutExtension(inputFile));

            Translator.Execute(inputFile, outputDirectory);
        }

        [MenuItem(cItemName_BeginWatchExcel)]
        public static void BeginWatchExcel()
        {
            var selected = Selection.activeObject;
            if (selected == null)
            {
                return;
            }

            var selectedPath = AssetDatabase.GetAssetPath(selected);

            var settings = FileImportSettings.FindOrCreate();
            settings.SetEnable(selectedPath);

            AssetDatabase.SaveAssets();
        }

        [MenuItem(cItemName_EndWatchExcel)]
        public static void EndWatchExcel()
        {
            var selected = Selection.activeObject;
            if (selected == null)
            {
                return;
            }

            var selectedPath = AssetDatabase.GetAssetPath(selected);

            var settings = FileImportSettings.FindOrCreate();
            settings.RemoveFile(selectedPath);

            AssetDatabase.SaveAssets();
        }



        [MenuItem(cItemName_ImportExcel, true)]
        [MenuItem(cItemName_BeginWatchExcel, true)]
        [MenuItem(cItemName_EndWatchExcel, true)]
        public static bool CanApplyImportExcel()
        {
            if (Selection.activeObject == null)
            {
                return false;
            }

            var inputPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            return Path.GetExtension(inputPath) == ".xlsx";
        }

        [MenuItem("Test/aaa")]
        public static void Test()
        {
            var languages = new[] { "ja-JP", "en-US" };
            var pages = new[] { "test1", "test2" };

            foreach (var lang in languages)
            {
                var set = DictionarySet.Create(string.Format("Assets/Resources/{0}.asset", lang), pages);
                foreach (var page in pages)
                {
                    set.SetText(page, "aaa", "aaa" + page + lang);
                    set.SetText(page, "bbb", "bbb" + page + lang);
                }
            }

            AssetDatabase.SaveAssets();
        }

        [MenuItem("Test/bbb")]
        public static void Test1()
        {
            var languages = new[] { "ja-JP", "en-US" };
            var pages = new[] { "test1", "test2" };

            foreach (var lang in languages)
            {
                var set = AssetDatabase.LoadAssetAtPath<DictionarySet>(string.Format("Assets/Resources/{0}.asset", lang));
                foreach (var page in pages)
                {
                    Debug.Log(set.GetText(page, "aaa"));
                    Debug.Log(set.GetText(page, "bbb"));
                }
            }
        }
    }
}
