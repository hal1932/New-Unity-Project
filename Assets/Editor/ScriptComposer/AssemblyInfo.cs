﻿using EditorUtil;
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ScriptComposer
{
    [Serializable]
    public class SourceInfo
    {
        public string AssetPath;

        public SourceInfo(string source, BuildSettings settings, string assemblyName)
        {
            AssetPath = source;

            _settings = settings;
            _assemblyName = assemblyName;
        }

        public string GetCachedScriptPath()
        {
            return AssetUtil.CombinePath(
                    ProjectInfo.RootPath,
                    _settings.SourceCodeCacheRoot,
                    _assemblyName,
                    AssetPath);
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

            _destRoot = AssetUtil.CombinePath(
                ProjectInfo.RootPath,
                _settings.SourceCodeCacheRoot,
                Path.GetFileNameWithoutExtension(assembly));
        }

        public static AssemblyInfo LoadFromFile(BuildSettings settings, string assemblyName)
        {
            var destRoot = AssetUtil.CombinePath(
                ProjectInfo.RootPath,
                settings.SourceCodeCacheRoot,
                assemblyName);

            var configPath = AssetUtil.CombinePath(destRoot, assemblyName + ".json");
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
            CleanupCache();
            Directory.CreateDirectory(_destRoot);

            var configPath = AssetUtil.CombinePath(
                _destRoot,
                Path.GetFileNameWithoutExtension(Assembly) + ".json");
            var jsonStr = JsonUtility.ToJson(this);
            File.WriteAllText(configPath, jsonStr);
        }

        public void StashScripts()
        {
            foreach (var source in Sources)
            {
                var sourcePath = AssetUtil.CombinePath(ProjectInfo.RootPath, source.AssetPath);
                var destPath = AssetUtil.CombinePath(_destRoot, source.AssetPath);
                AssetUtil.CopyFile(sourcePath, destPath, true);
            }

            foreach (var source in Sources)
            {
                AssetDatabase.DeleteAsset(source.AssetPath);
            }

            var directories = Sources.Select(x => Path.GetDirectoryName(x.AssetPath)).Distinct();
            foreach (var directory in directories)
            {
                var files = AssetDatabase.FindAssets(string.Empty, new[] { directory });
                if (!files.Any())
                {
                    AssetDatabase.DeleteAsset(directory);
                }
            }
        }

        public void CleanupCache()
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
