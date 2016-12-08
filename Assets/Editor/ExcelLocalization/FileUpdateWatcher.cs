using UnityEngine;
using System.Collections;
using UnityEditor;
using EditorUtil;
using System.Linq;

namespace ExcelLocalization
{
    public class FileUpdateWatcher : AssetPostprocessor
    {
        public static void OnPostprocessAllAssets(string[] importedAssets, string[] _1, string[] _2, string[] _3)
        {
            if (importedAssets.Length == 0)
            {
                return;
            }

            var settings = FileImportSettings.Find();
            if (settings == null)
            {
                return;
            }

            var excelPath = importedAssets.FirstOrDefault(file => settings.IsEnabledFile(file));
            if (excelPath == null)
            {
                return;
            }

            Debug.Log(excelPath);
        }
    }
}