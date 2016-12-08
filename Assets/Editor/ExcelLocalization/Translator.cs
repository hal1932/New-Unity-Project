using EditorUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ExcelLocalization
{
    public class Translator
    {
        public static void Execute(string inputFile, string outputDirectory)
        {
            // とりえあずエクセルをそのまま読む
            var sheets = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
            using (var reader = new ExcelReader(inputFile))
            {
                foreach (var sheet in reader.EnumerateSheets())
                {
                    sheets[sheet.Name] = sheet.ToDictionary(cell => cell.Header, cell => cell.StringValue);
                }
            }

            var pages = sheets.Keys.ToArray();
            if (pages.Length == 0)
            {
                return;
            }

            // エクセルの中身を言語ごとにばらす
            using (new LockReloadAssetScope())
            {
                var dicSets = new Dictionary<string, DictionarySet>();
                var languages = sheets.First().Value.Values.First().Keys.ToArray();

                // 言語ごとの辞書をつくる
                foreach (var lang in languages)
                {
                    var path = AssetUtil.CombinePath(outputDirectory, lang + ".asset");
                    var dicSet = DictionarySet.Create(path, pages);
                    dicSets[lang] = dicSet;
                }

                // 辞書に中身を詰める
                foreach (var sheet in sheets)
                {
                    foreach (var itemsDic in sheet.Value)
                    {
                        var itemName = itemsDic.Key;
                        var items = itemsDic.Value;

                        foreach (var dicSetItem in dicSets)
                        {
                            var dicLang = dicSetItem.Key;
                            var dicSet = dicSetItem.Value;
                            dicSet.SetText(sheet.Key, itemName, items[dicLang]);
                            //Debug.LogFormat("{0} {1} {2} {3}", dicLang, sheet.Key, itemName, items[dicLang]);
                        }
                    }
                }

                AssetDatabase.SaveAssets();
            }
        }
    }
}
