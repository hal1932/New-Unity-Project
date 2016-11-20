using EditorUtil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ScriptComposer
{
    [Serializable]
    public class SourceInfo
    {
        public string FilePath;
        public long FileTime;

        public DateTime LastUpdated { get { return DateTime.FromFileTimeUtc(FileTime); } }

        public SourceInfo(string source)
        {
            FilePath = source;

            var fullpath = string.Format("{0}/{1}", AssetUtil.GetProjectRoot(), source);
            FileTime = new FileInfo(fullpath).LastWriteTime.ToFileTimeUtc();
        }
    }

    [Serializable]
    public class AssemblyInfo
    {
        public string Assembly;
        public SourceInfo[] Sources;

        public AssemblyInfo(BuildSettings settings, string assembly, string[] sources)
        {
            Assembly = assembly;
            Sources = sources.Select(x => new SourceInfo(x)).ToArray();

            _settings = settings;

            _destRoot = string.Format(
                "{0}/{1}/{2}",
                AssetUtil.GetProjectRoot(),
                _settings.SourceCodeCacheRoot,
                Path.GetFileNameWithoutExtension(assembly));
        }

        public static AssemblyInfo LoadFromFile(string assembly)
        {
            return null;
        }

        public bool SaveToFile(bool force = false)
        {
            if (force)
            {
                ClearFiles();
            }

            var projectRoot = AssetUtil.GetProjectRoot();

            foreach (var source in Sources)
            {
                var sourcePath = string.Format("{0}/{1}", projectRoot, source.FilePath);
                var destPath = string.Format("{0}/{1}", _destRoot, source.FilePath);
                AssetUtil.CopyFile(sourcePath, destPath, true);
            }

            var configPath = string.Format(
                "{0}/{1}.json",
                _destRoot, Path.GetFileNameWithoutExtension(Assembly));
            var jsonStr = JsonUtility.ToJson(this);
            File.WriteAllText(configPath, jsonStr);

            return true;
        }

        public void ClearFiles()
        {
            if (Directory.Exists(_destRoot))
            {
                Directory.Delete(_destRoot, true);
            }
        }

        public void ClearSourceAssets()
        {
            foreach (var source in Sources)
            {
                AssetDatabase.DeleteAsset(source.FilePath);
            }
        }

        private BuildSettings _settings;
        private string _destRoot;
    }
}
