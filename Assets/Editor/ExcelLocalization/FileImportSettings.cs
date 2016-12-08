using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using EditorUtil;
using System.IO;
using System;

namespace ExcelLocalization
{
    [Serializable]
    public class _Item
    {
        [SerializeField]
        public string FileName;

        [SerializeField]
        public bool Enabled;
    }

    [Serializable]
    public class FileImportSettings : ScriptableObject
    {
        [SerializeField]
        public List<_Item> _items;

        public static FileImportSettings Find()
        {
            var settingsPath = AssetUtil.CombinePath(
                Path.GetDirectoryName(AssetUtil.FindScriptPath<FileImportSettings>()),
                "settings.asset");
            return File.Exists(settingsPath) ? AssetDatabase.LoadAssetAtPath<FileImportSettings>(settingsPath) : null;
        }

        public static FileImportSettings FindOrCreate()
        {
            FileImportSettings settings;

            var settingsPath = AssetUtil.CombinePath(
                Path.GetDirectoryName(AssetUtil.FindScriptPath<FileImportSettings>()),
                "settings.asset");
            if (!ScriptableObjectUtil.FindOrCreateObject(out settings, settingsPath))
            {
                settings._items = new List<_Item>();
            }

            return settings;
        }

        public void SetEnable(string filename, bool enabled = true)
        {
            if (_items == null)
            {
                return;
            }

            var found = _items.FirstOrDefault(item => item.FileName == filename);
            if (found != null)
            {
                if (found.Enabled == enabled)
                {
                    return;
                }
                found.Enabled = enabled;
            }
            else
            {
                _items.Add(new _Item() { FileName = filename, Enabled = enabled });
            }

            EditorUtility.SetDirty(this);
        }

        public void RemoveFile(string filename)
        {
            if (_items == null)
            {
                return;
            }

            var found = _items.FirstOrDefault(item => item.FileName == filename);
            if (found != null)
            {
                _items.Remove(found);
                EditorUtility.SetDirty(this);
            }
        }

        public bool IsEnabledFile(string filename)
        {
            if (_items == null)
            {
                return false;
            }

            var found = _items.FirstOrDefault(item => item.FileName == filename);
            return (found != null && found.Enabled);
        }
    }

    [CustomEditor(typeof(FileImportSettings))]
    public class FileImportSettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var settings = (FileImportSettings)target;

            var items = settings._items;
            if (items == null || items.Count == 0)
            {
                return;
            }

            foreach (var item in items)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    item.Enabled = EditorGUILayout.Toggle(item.Enabled);
                    EditorGUILayout.LabelField(item.FileName);
                }
            }
        }
    }
}