using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ScriptImporter
{
    class PreprocessResult
    {
        public string[] SourceFiles { get; private set; }
        public string[] References { get; private set; }

        public PreprocessResult(string[] sources, string[] references)
        {
            SourceFiles = sources;
            References = references;
        }
    }

    static class Preprocessor
    {
        public static PreprocessResult Preprocess(params string[] sourceFiles)
        {
            var files = new List<string>(sourceFiles);
            var references = new List<string>();
            foreach (var sourceFile in sourceFiles)
            {
                PreprocessRec(sourceFile, files, references);
            }

            return new PreprocessResult(files.ToArray(), references.ToArray());
        }

        private static void PreprocessRec(string sourceFile, List<string> files, List<string> references)
        {
            if (files.Contains(sourceFile))
            {
                return;
            }

            var rootPath = Path.GetDirectoryName(sourceFile);

            using (var reader = new StreamReader(sourceFile))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (string.IsNullOrEmpty(line))
                    {
                        continue;
                    }

                    if (!line.StartsWith("///#"))
                    {
                        break;
                    }

                    string directive;
                    var filepath = ParseDirective(out directive, line, rootPath);
                    if (filepath == null)
                    {
                        continue;
                    }

                    switch(directive)
                    {
                        case "r":
                            if (!references.Contains(filepath))
                            {
                                references.Add(filepath);
                            }
                            break;

                        case "load":
                            if (!files.Contains(filepath))
                            {
                                files.Add(filepath);
                            }
                            break;

                        default:
                            break;
                    }
                }
            }
        }

        private static string ParseDirective(out string directive, string line, string rootPath)
        {
            var items = line.Split('"');
            if (items.Length < 2)
            {
                directive = null;
                return null;
            }

            directive = items[0].TrimStart('/', '#');
            var path = items[1];
            return Path.GetFullPath(
                Path.IsPathRooted(path) ? Path.Combine(rootPath, path) : path);
        }
    }
}
