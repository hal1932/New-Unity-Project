using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ExcelLocalization
{
    [Serializable]
    public class LocalizedItem : UnityEngine.Object
    {
        [SerializeField]
        public int KeyHash;
        [SerializeField]
        public string Key;
        [SerializeField]
        public string Value;

        public LocalizedItem(int keyHash, string key, string value)
        {
            KeyHash = keyHash;
            Key = key;
            Value = value;
        }
    }

    [Serializable]
    public class LocalizedDictionary : ScriptableObject
    {
        public string this[string key]
        {
            get
            {
                return Gestring(key);
            }

            set
            {
                SetItem(key, value);
            }
        }

        public List<LocalizedItem>[] Buckets { get { return _buckets; } }

        public LocalizedDictionary()
        {
            var hashSize = 100;
            _buckets = new List<LocalizedItem>[hashSize];
        }

        public bool TryGestring(string key, out string value)
        {
            value = default(string);

            var hash = GetItemHash(key);
            if (_buckets[hash] == null || _buckets[hash].Count == 0)
            {
                return false;
            }

            var keyHash = key.GetHashCode();
            foreach (var item in _buckets[hash])
            {
                if (item.KeyHash == keyHash)
                {
                    value = item.Value;
                    return true;
                }
            }

            return false;
        }

        private string Gestring(string key)
        {
            string value;
            if (TryGestring(key, out value))
            {
                return value;
            }
            throw new KeyNotFoundException(key.ToString());
        }

        private void SetItem(string key, string value)
        {
            var hash = GetItemHash(key);
            if (_buckets[hash] == null)
            {
                _buckets[hash] = new List<LocalizedItem>();
            }
            _buckets[hash].Add(new LocalizedItem(key.GetHashCode(), key, value));
        }

        private int GetItemHash(string key)
        {
            return Math.Abs(key.GetHashCode() % _buckets.Length);
        }

        [SerializeField]
        private List<LocalizedItem>[] _buckets;
    }

    [CustomEditor(typeof(LocalizedDictionary))]
    public class LocalizedDictionaryEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var dic = (LocalizedDictionary)target;
            foreach (var bucket in dic.Buckets)
            {
                if (bucket == null || bucket.Count == 0)
                {
                    continue;
                }

                foreach (var item in bucket)
                {
                    EditorGUILayout.TextField(item.Key, item.Value);
                }
            }
        }
    }
}
