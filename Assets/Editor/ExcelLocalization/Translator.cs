using EditorUtil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ExcelLocalization
{
    public class Translator
    {
        public static bool UpdateDictionaries(string inputFile, string outputDirectory)
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
                return false;
            }

            // エクセルの中身を言語ごとにばらす
            using (new LockReloadAssetScope())
            {
                var dicSets = new Dictionary<string, DictionarySet>();

                string[] languages;
                {
                    var firstSheet = sheets.First().Value;
                    var firstRow = firstSheet.Values.First();
                    languages = firstRow.Keys.ToArray();
                }

                var updated = false;

                // 言語ごとの辞書をつくる
                foreach (var lang in languages)
                {
                    var path = AssetUtil.CombinePath(outputDirectory, lang + ".asset");

                    DictionarySet dicSet;
                    if (File.Exists(path))
                    {
                        dicSet = AssetDatabase.LoadAssetAtPath<DictionarySet>(path);
                    }
                    else
                    {
                        dicSet = DictionarySet.Create(path, pages);
                        updated = true;
                    }
                    dicSets[lang] = dicSet;
                }

                // 辞書ごとに差分更新する
                foreach (var sheet in sheets)
                {
                    var page = sheet.Key;
                    var itemDics = sheet.Value;

                    var currentItems = new List<string>();

                    foreach (var itemsDic in itemDics)
                    {
                        var itemName = itemsDic.Key; // テキスト名
                        var items = itemsDic.Value;  // 全言語分のテキスト

                        foreach (var dicSetItem in dicSets)
                        {
                            var dicLang = dicSetItem.Key;
                            var dicSet = dicSetItem.Value;
                            updated |= dicSet.SetText(page, itemName, items[dicLang]);
                            currentItems.Add(itemName);
                        }
                    }

                    currentItems.Distinct();

                    foreach (var dicSetItem in dicSets)
                    {
                        var dicSet = dicSetItem.Value;
                        var removedKeys = dicSet.EnumerateKeys(page)
                            .Except(currentItems)
                            .ToArray();
                        foreach (var removedKey in removedKeys)
                        {
                            dicSet.RemoveText(sheet.Key, removedKey);
                            updated = true;
                        }
                    }
                }

                if (updated)
                {
                    AssetDatabase.SaveAssets();
                }
                return updated;
            }
        }
    }
}
