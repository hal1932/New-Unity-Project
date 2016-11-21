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
    class Composer
    {
        public Composer(BuildSettings settings)
        {
            _settings = settings;
        }

        public string BuildScripts(string[] scripts)
        {
            if (scripts.Length == 0)
            {
                return null;
            }

            var assemblyName = Path.GetFileNameWithoutExtension(scripts.First());

            // スクリプトのパスをいちいち指定してたらコマンドラインが文字数オーバーするかもしれないから
            // 一旦テンポラリに全部コピーして *.cs で指定できるようにする。
            var tmpScriptsDir = AssetUtil.CombinePath(Application.temporaryCachePath, assemblyName);
            foreach (var script in scripts)
            {
                var source = AssetUtil.CombinePath(ProjectInfo.RootPath, script);
                var dest = AssetUtil.CombinePath(tmpScriptsDir, Path.GetFileName(script));
                AssetUtil.CopyFile(source, dest, true);
            }

            // 既にビルド済みアセンブリがいたら消す
            var outputPath = AssetUtil.CombinePath(_settings.AssemblyRoot, assemblyName + ".dll");
            if (!AssetUtil.AssetExists(outputPath))
            {
                AssetDatabase.DeleteAsset(outputPath);
            }
            AssetUtil.CreateAssetDirectory(Path.GetDirectoryName(outputPath));

            var outputAssemblyPath = AssetUtil.CombinePath(ProjectInfo.RootPath, outputPath);

            // ビルド
            outputAssemblyPath = ScriptBuilder.Build(
                outputAssemblyPath,
                new[] { tmpScriptsDir + "/*.cs" },
                output => Debug.Log(output),
                error => Debug.LogError(error));
            if (string.IsNullOrEmpty(outputAssemblyPath))
            {
                return null;
            }

            // ビルドしたアセンブリをインポートして、ソースコードを退避
            if (File.Exists(outputAssemblyPath))
            {
                AssetDatabase.ImportAsset(outputPath);

                var info = new AssemblyInfo(_settings, outputPath, scripts);
                info.SaveToFile();
                info.StashScripts();

                return outputPath;
            }
            return null;
        }

        public string[] RevertToScripts(BuildSettings settings, string assemblyName)
        {
            var info = AssemblyInfo.LoadFromFile(settings, assemblyName);

            var error = false;

            // ソースコードのタイムスタンプチェック
            foreach (var source in info.Sources)
            {
                var scriptPath = source.GetCachedScriptPath();

                var fileInfo = new FileInfo(scriptPath);
                if (!fileInfo.Exists)
                {
                    Debug.LogErrorFormat("the CONFLICT is found at {0}", scriptPath);
                    error = true;
                }
            }

            if (error)
            {
                return null;
            }

            // ソースコードを書き戻す
            foreach (var source in info.Sources)
            {
                var scriptPath = source.GetCachedScriptPath();
                var destAssetPath = source.AssetPath;
                var destPath = AssetUtil.CombinePath(ProjectInfo.RootPath, destAssetPath);

                AssetUtil.CopyFile(scriptPath, destPath, true);
                AssetDatabase.ImportAsset(destAssetPath);
            }

            // アセンブリを消す
            AssetDatabase.DeleteAsset(info.Assembly);

            info.DeleteCaches();

            return info.Sources.Select(x => x.AssetPath).ToArray();
        }

        private BuildSettings _settings;
    }
}
