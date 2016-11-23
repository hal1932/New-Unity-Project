using EditorUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ScriptComposer
{
    static class ScriptBuilder
    {
        public static bool Build(string outputPath, string[] scripts, Action<string> onOutputReceived, Action<string> onErrorReceived)
        {
            var result = true;

            AssetUtil.DeleteFile(outputPath);

            using (var process = Process.Start(new ProcessStartInfo()
            {
#if UNITY_EDITOR_WIN
                FileName = EditorUtil.Preference.MonoDirectory + "/bin/smcs.bat",
#elif UNITY_EDITOR_OSX
                FileName = EditorUtil.Preference.MonoDirectory + "/bin/smcs",
#endif
                Arguments = string.Join(
                    " ",
                    new[]
                    {
                        string.Format("-r:\"{0}\"", AssetUtil.CombinePath(ProjectInfo.UnityAssemblyRoot, "UnityEngine.dll")),
                        string.Format("-r:\"{0}\"", AssetUtil.CombinePath(ProjectInfo.UnityAssemblyRoot, "UnityEditor.dll")),
                        "-target:library",
                        "-warnaserror+",
                        string.Format("-out:\"{0}\"", outputPath),
                        string.Join(" ", scripts.Select(x => string.Format("\"{0}\"", x)).ToArray()),
                    }),
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            }))
            {
                if (onOutputReceived != null)
                {
                    process.OutputDataReceived += (_, e) => onOutputReceived(e.Data);
                }
                if (onErrorReceived != null)
                {
                    process.ErrorDataReceived += (_, e) =>
                    {
                        result = false;
                        onErrorReceived(e.Data);
                    };
                }

                process.WaitForExit();
            }

            return result && File.Exists(outputPath);
        }
    }
}
