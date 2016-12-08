using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace EditorUtil
{
    public static class ScriptableObjectUtil
    {
        public static bool FindOrCreateObject<T>(out T item, string path)
            where T : ScriptableObject
        {
            if (File.Exists(path))
            {
                item = AssetDatabase.LoadAssetAtPath<T>(path);
                return true;
            }

            item = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(item, path);
            AssetDatabase.SaveAssets();

            return false;
        }
    }
}
