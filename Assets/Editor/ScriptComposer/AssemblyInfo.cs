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

        public SourceInfo(string source, BuildSettings settings, string assemblyName)
        {
            FilePath = source;

            _settings = settings;
            _assemblyName = assemblyName;
        }

        public string GetCachedScriptPath()
        {
            var projectRoot = AssetUtil.GetProjectRoot();
            return string.Format(
                    "{0}/{1}/{2}/{3}",
                    projectRoot,
                    _settings.SourceCodeCacheRoot,
                    _assemblyName,
                    FilePath);
        }

        // TODO: 汚い
        internal void SetupInternal(BuildSettings settings, string assemblyName)
        {
            _settings = settings;
            _assemblyName = assemblyName;
        }

        private BuildSettings _settings;
        private string _assemblyName;
    }

    [Serializable]
    public class AssemblyInfo
    {
        public string Assembly;
        public SourceInfo[] Sources;

        public AssemblyInfo(BuildSettings settings, string assembly, string[] sources)
        {
            Assembly = assembly;

            var assemblyName = Path.GetFileNameWithoutExtension(Assembly);
            Sources = sources.Select(x => new SourceInfo(x, settings, assemblyName))
                .ToArray();

            _settings = settings;

            _destRoot = string.Format(
                "{0}/{1}/{2}",
                AssetUtil.GetProjectRoot(),
                _settings.SourceCodeCacheRoot,
                Path.GetFileNameWithoutExtension(assembly));
        }

        public static AssemblyInfo LoadFromFile(BuildSettings settings, string assemblyName)
        {
            var destRoot = string.Format(
                "{0}/{1}/{2}",
                AssetUtil.GetProjectRoot(),
                settings.SourceCodeCacheRoot,
                assemblyName);

            var configPath = string.Format(
                "{0}/{1}.json",
                destRoot, assemblyName);
            var jsonStr = File.ReadAllText(configPath);
            var instance = JsonUtility.FromJson<AssemblyInfo>(jsonStr);

            instance._settings = settings;
            instance._destRoot = destRoot;
            foreach (var source in instance.Sources)
            {
                source.SetupInternal(settings, assemblyName);
            }

            return instance;
        }

        public void SaveToFile()
        {
            DeleteCaches();
            Directory.CreateDirectory(_destRoot);

            var configPath = string.Format(
                "{0}/{1}.json",
                _destRoot, Path.GetFileNameWithoutExtension(Assembly));
            var jsonStr = JsonUtility.ToJson(this);
            File.WriteAllText(configPath, jsonStr);
        }

        public void StashScripts()
        {
            var projectRoot = AssetUtil.GetProjectRoot();

            foreach (var source in Sources)
            {
                var sourcePath = string.Format("{0}/{1}", projectRoot, source.FilePath);
                var destPath = string.Format("{0}/{1}", _destRoot, source.FilePath);
                AssetUtil.CopyFile(sourcePath, destPath, true);
            }

            foreach (var source in Sources)
            {
                AssetDatabase.DeleteAsset(source.FilePath);
            }
        }

        public void DeleteCaches()
        {
            if (Directory.Exists(_destRoot))
            {
                Directory.Delete(_destRoot, true);
            }
        }

        private BuildSettings _settings;
        private string _destRoot;
    }
}
