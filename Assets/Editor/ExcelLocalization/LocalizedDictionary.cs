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
                return GetString(key);
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

        public bool TryGetString(string key, out string value)
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

        public void RemoveItem(string key)
        {
            var hash = GetItemHash(key);
            if (_buckets[hash] == null || _buckets[hash].Count == 0)
            {
                return;
            }

            var keyHash = key.GetHashCode();

            var index = 0;
            for (; index < _buckets[hash].Count; ++index)
            {
                if (_buckets[hash][index].KeyHash == keyHash)
                {
                    _buckets[hash].RemoveAt(index);
                    break;
                }
            }

            if (_buckets[hash].Count == 0)
            {
                _buckets[hash] = null;
            }
            else
            {
                for (; index < _buckets[hash].Count - 1; ++index)
                {
                    _buckets[hash][index] = _buckets[hash][index + 1];
                }
            }
        }

        public IEnumerable<string> EnumerateKeys()
        {
            foreach (var bucket in _buckets.Where(x => x != null))
            {
                foreach (var item in bucket)
                {
                    yield return item.Key;
                }
            }
        }

        public bool ContainsKey(string key)
        {
            var hash = GetItemHash(key);
            if (_buckets[hash] == null || _buckets[hash].Count == 0)
            {
                return false;
            }
            return _buckets[hash].Any(item => item.KeyHash == key.GetHashCode());
        }

        public bool SetItem(string key, string value)
        {
            var hash = GetItemHash(key);
            if (_buckets[hash] == null)
            {
                _buckets[hash] = new List<LocalizedItem>();
            }

            var keyHash = key.GetHashCode();
            var item = _buckets[hash].FirstOrDefault(x => x.KeyHash == keyHash);
            if (item == null)
            {
                _buckets[hash].Add(new LocalizedItem(key.GetHashCode(), key, value));
                return true;
            }
            else
            {
                if (item.Value != value)
                {
                    item.Value = value;
                    return true;
                }
                return false;
            }
        }

        private string GetString(string key)
        {
            string value;
            if (TryGetString(key, out value))
            {
                return value;
            }
            throw new KeyNotFoundException(key.ToString());
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
