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

            var projectPath = AssetUtil.GetProjectRoot();

            // スクリプトのパスをいちいち指定してたらコマンドラインが文字数オーバーするかもしれないから
            // 一旦テンポラリに全部コピーして *.cs で指定できるようにする。
            var tmpScriptsDir = string.Format("{0}/{1}", Application.temporaryCachePath, assemblyName);
            foreach (var script in scripts)
            {
                var source = string.Format("{0}/{1}", projectPath, script);
                var dest = string.Format("{0}/{1}", tmpScriptsDir, Path.GetFileName(script));
                EditorUtil.AssetUtil.CopyFile(source, dest, true);
            }

            // 既にビルド済みアセンブリがいたら消す
            var outputPath = string.Format("{0}/{1}.dll", _settings.AssemblyRoot, assemblyName);
            if (!AssetUtil.AssetExists(outputPath))
            {
                AssetDatabase.DeleteAsset(outputPath);
            }
            AssetUtil.CreateAssetDirectory(Path.GetDirectoryName(outputPath));

            var outputAssemblyPath = string.Format("{0}/{1}", projectPath, outputPath);

            var error = false;

            // ビルド
            var managedDllDir = Preference.MonoDirectory + "/../Managed";
            using (var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
            {
#if UNITY_EDITOR_WIN
                FileName = Preference.MonoDirectory + "/bin/smcs.bat",
#elif UNITY_EDITOR_OSX
                FileName = Preference.MonoDirectory + "/bin/smcs",
#endif
                Arguments = string.Join(
                    " ",
                    new[]
                    {
                        string.Format("-r:\"{0}/UnityEngine.dll\"", managedDllDir),
                        string.Format("-r:\"{0}/UnityEditor.dll\"", managedDllDir),
                        "-target:library",
                        string.Format("-out:\"{0}\"", outputAssemblyPath),
                        string.Format("\"{0}/*.cs\"", tmpScriptsDir),
                    }),
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            }))
            {
                while (true)
                {
                    if (process.StandardOutput.EndOfStream && process.StandardError.EndOfStream)
                    {
                        break;
                    }

                    if (!process.StandardOutput.EndOfStream)
                    {
                        var output = process.StandardOutput.ReadLine().Trim();
                        if (!string.IsNullOrEmpty(output))
                        {
                            Debug.Log(output);
                        }
                    }
                    if (!process.StandardError.EndOfStream)
                    {
                        Debug.LogError(process.StandardError.ReadLine());
                        error = true;
                    }
                }

                process.WaitForExit();
            }

            if(error)
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
                var destPath = source.FilePath;
                AssetUtil.CopyFile(scriptPath, destPath, true);
                AssetDatabase.ImportAsset(destPath);
            }

            // アセンブリを消す
            AssetDatabase.DeleteAsset(info.Assembly);

            info.DeleteCaches();

            return info.Sources.Select(x => x.FilePath).ToArray();
        }

        private BuildSettings _settings;
    }
}
