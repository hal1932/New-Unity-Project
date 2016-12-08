using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace EditorUtil
{
    public static class AssetUtil
    {
        public static string CombinePath(params string[] paths)
        {
            return string.Join("/", paths.Select(x => x.Trim('/')).ToArray());
        }

        public static bool CopyFile(string source, string dest, bool overwrite)
        {
            if(!overwrite && File.Exists(dest))
            {
                return false;
            }

            var destDir = Path.GetDirectoryName(dest);
            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }
            File.Copy(source, dest, overwrite);

            return true;
        }

        public static void DeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Decrypt(path);
            }
        }

        public static bool AssetExists(string path)
        {
            return !string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(path));
        }

        public static void CleanupDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
                Directory.CreateDirectory(path);
            }
            else
            {
                Directory.CreateDirectory(path);
            }
        }

        public static void DeleteAsset(string path)
        {
            if (!AssetUtil.AssetExists(path))
            {
                AssetDatabase.DeleteAsset(path);
            }
        }

        public static void CreateAssetDirectory(string path)
        {
            var dirs = path.Split('/');

            var current = dirs[0];
            if (!AssetDatabase.IsValidFolder(current))
            {
                AssetDatabase.CreateFolder(string.Empty, current);
            }

            for (var i = 1; i < dirs.Length; ++i)
            {
                current += '/' + dirs[i];

                if (!AssetDatabase.IsValidFolder(current))
                {
                    AssetDatabase.CreateFolder(
                        Path.GetDirectoryName(current),
                        Path.GetFileName(current));
                }
            }
        }

        public static void CleanupAssetDirectory(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.DeleteAsset(path);
            }
            CreateAssetDirectory(path);
        }

        public static string FindScriptPath<T>()
        {
            return AssetDatabase.FindAssets("t:script " + typeof(T).Name)
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .FirstOrDefault();
        }
    }
}
