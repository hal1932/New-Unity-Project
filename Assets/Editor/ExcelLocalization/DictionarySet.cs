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
    [Serializable]
    public class DictionarySet : ScriptableObject
    {
        public static DictionarySet Create(string path, params string[] pages)
        {
            AssetUtil.CreateAssetDirectory(Path.GetDirectoryName(path));
            AssetUtil.DeleteAsset(path);

            var instance = ScriptableObject.CreateInstance<DictionarySet>();
            AssetDatabase.CreateAsset(instance, path);

            instance._pageNumbers = new string[pages.Length];

            var items = new List<LocalizedDictionary>();
            for (var i = 0; i < pages.Length; ++i)
            {
                var item = ScriptableObject.CreateInstance<LocalizedDictionary>();
                item.name = pages[i];
                items.Add(item);

                instance._pageNumbers[i] = pages[i];

                AssetDatabase.AddObjectToAsset(item, path);
            }
            instance._dictionaries = items.ToArray();

            AssetDatabase.ImportAsset(path);

            return instance;
        }

        public IEnumerable<string> EnumerateKeys(string page)
        {
            var index = Array.IndexOf(_pageNumbers, page);
            return _dictionaries[index].EnumerateKeys();
        }

        public bool SetText(string page, string key, string value)
        {
            var index = Array.IndexOf(_pageNumbers, page);
            if (index >= 0 && string.IsNullOrEmpty(value))
            {
                _dictionaries[index].RemoveItem(key);
                return true;
            }
            else
            {
                return _dictionaries[index].SetItem(key, value);
            }
        }

        public void RemoveText(string page, string key)
        {
            SetText(page, key, null);
        }

        public bool TryGetText(string page, string key, out string value)
        {
            var index = Array.IndexOf(_pageNumbers, page);
            if (index < 0)
            {
                value = null;
                return false;
            }
            return _dictionaries[index].TryGetString(key, out value);
        }

        public string GetText(string page, string key)
        {
            var index = Array.IndexOf(_pageNumbers, page);
            return _dictionaries[index][key];
        }

        [SerializeField]
        private LocalizedDictionary[] _dictionaries;

        [SerializeField]
        private string[] _pageNumbers;
    }
}
