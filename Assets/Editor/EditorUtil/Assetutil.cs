﻿using System;
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

        public static bool AssetExists(string path)
        {
            return !string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(path));
        }

        public static void CreateAssetDirectory(string path)
        {
            var dirs =path.Split('/');

            var current = dirs[0];
            if (!AssetExists(current))
            {
                AssetDatabase.CreateFolder(string.Empty, current);
            }

            for (var i = 1; i < dirs.Length; ++i)
            {
                current += '/' + dirs[i];

                if (!AssetExists(current))
                {
                    AssetDatabase.CreateFolder(
                        Path.GetDirectoryName(current),
                        Path.GetFileName(current));
                }
            }
        }
    }
}