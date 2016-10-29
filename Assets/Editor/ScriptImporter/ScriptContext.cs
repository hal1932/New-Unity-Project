using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ScriptImporter
{
    public class ScriptContext : IDisposable
    {
        public string CompilerVersion { get; set; }
        public string CompilerOptions { get; set; }
        public int WarningLevel { get; set; }
        public bool IncludeDebugInformation { get; set; }
        public bool EnablePreprocess { get; set; }

        public string[] Scripts { get; private set; }

        public Assembly CompiledAssembly
        {
            get { return (_compilerResults != null) ? _compilerResults.CompiledAssembly : null; }
        }

        public IEnumerable<CompilerError> CompileErros
        {
            get { return (_compilerResults != null) ?
                    _compilerResults.Errors.Cast<CompilerError>() : Enumerable.Empty<CompilerError>(); }
        }

        #region IDisposable
        ~ScriptContext()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            lock (_disposingLock)
            {
                if (_disposed)
                {
                    return;
                }
                _disposed = true;

                if (disposing)
                {
                }
            }
        }

        private bool _disposed;
        private object _disposingLock = new object();
        #endregion

        public ScriptContext()
        {
            CompilerVersion = "v2.0";
            CompilerOptions = "/optimize";
            WarningLevel = 4;
            IncludeDebugInformation = false;
            EnablePreprocess = true;
        }

        public bool LoadDirectory(string path, SearchOption searchOption = SearchOption.AllDirectories)
        {
            Scripts = Directory.GetFiles(Path.GetFullPath(path), "*.cs", searchOption);
            return LoadImpl();
        }

        public bool LoadFiles(params string[] paths)
        {
            Scripts = paths.Select(path => Path.GetFullPath(path)).ToArray();
            return LoadImpl();
        }

        public bool BuildDirectory(string outputPath, string path, SearchOption searchOption = SearchOption.AllDirectories)
        {
            Scripts = Directory.GetFiles(Path.GetFullPath(path), "*.cs", searchOption);
            return BuildImpl(outputPath);
        }

        public bool BuildFiles(string outputPath, params string[] paths)
        {
            Scripts = paths.Select(path => Path.GetFullPath(path)).ToArray();
            return BuildImpl(outputPath);
        }

        private bool LoadImpl()
        {
            _compilerResults = CompileImpl();
            return _compilerResults.Errors.Count == 0;
        }

        private bool BuildImpl(string outputPath)
        {
            _compilerResults = CompileImpl(outputPath);
            return _compilerResults.Errors.Count == 0;
        }

        private CompilerResults CompileImpl(string outputAssembly = null)
        {
            var unityLibRoot = string.Join(
                Path.DirectorySeparatorChar.ToString(),
                new[] { Application.dataPath, "..", "Library", "UnityAssemblies" });
            
            var references = new[]
            {
                Path.Combine(unityLibRoot, @"UnityEngine.dll"),
                Path.Combine(unityLibRoot, @"UnityEditor.dll"),
            };

            var options = new CompilerParameters(references)
            {
                GenerateExecutable = false,
                IncludeDebugInformation = IncludeDebugInformation,
                WarningLevel = WarningLevel,
                CompilerOptions = CompilerOptions,

                GenerateInMemory = (outputAssembly == null),
                OutputAssembly = outputAssembly,
            };


            var provider = new CSharpCodeProvider(
                new Dictionary<string, string>() { { "CompilerVersion", CompilerVersion } });

            if (EnablePreprocess)
            {
                var result = Preprocessor.Preprocess(Scripts);
                options.ReferencedAssemblies.AddRange(result.References);
                Scripts = result.SourceFiles;
            }
            return provider.CompileAssemblyFromFile(options, Scripts);
        }

        private CompilerResults _compilerResults;
    }
}
