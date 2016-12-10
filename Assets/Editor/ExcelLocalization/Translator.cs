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
                    foreach (var itemsDic in sheet.Value)
                    {
                        var itemName = itemsDic.Key;
                        var items = itemsDic.Value;

                        // TODO: 以下、もっとループ回数減らせる気がする

                        var currentItems = new List<string>();
                        foreach (var dicSetItem in dicSets)
                        {
                            var dicLang = dicSetItem.Key;
                            var dicSet = dicSetItem.Value;
                            updated |= dicSet.SetText(sheet.Key, itemName, items[dicLang]);
                            currentItems.Add(itemName);
                        }

                        foreach (var dicSetItem in dicSets)
                        {
                            var dicSet = dicSetItem.Value;
                            var removedKeys = dicSet.EnumerateKeys(sheet.Key)
                                .Except(currentItems);
                            foreach (var removedKey in removedKeys)
                            {
                                dicSet.RemoveText(sheet.Key, removedKey);
                                updated = true;
                            }
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
