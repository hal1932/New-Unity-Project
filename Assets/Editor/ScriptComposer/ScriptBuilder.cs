using EditorUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ScriptComposer
{
    static class ScriptBuilder
    {
        public static string Build(string outputPath, string[] scripts, Action<string> onOutputReceived, Action<string> onErrorReceived)
        {
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
                        string.Format("-r:\"{0}\"", AssetUtil.CombinePath(ProjectInfo.UnityAssemblyRoot, "UnityEngine.dll")),
                        string.Format("-r:\"{0}\"", AssetUtil.CombinePath(ProjectInfo.UnityAssemblyRoot, "UnityEditor.dll")),
                        "-target:library",
                        string.Format("-out:\"{0}\"", outputPath),
                        string.Join(" ", scripts.Select(x => string.Format("\"{0}\"", x)).ToArray()),
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
                            if (onOutputReceived != null)
                            {
                                onOutputReceived(output);
                            }
                        }
                    }
                    if (!process.StandardError.EndOfStream)
                    {
                        var error = process.StandardError.ReadLine();
                        if (!string.IsNullOrEmpty(error))
                        {
                            if (onErrorReceived != null)
                            {
                                onErrorReceived(error);
                            }
                        }
                        outputPath = null;
                    }
                }

                process.WaitForExit();
            }

            return outputPath;
        }
    }
}
