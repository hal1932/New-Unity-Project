using EditorUtil;
using System.IO;
using UnityEngine;

namespace ScriptComposer
{
    static class ProjectInfo
    {
        public static string RootPath { get; private set; }

        public static string UnityAssemblyRoot
        {
            get { return AssetUtil.CombinePath(Preference.MonoDirectory, "..", "Managed"); }
        }

        static ProjectInfo()
        {
            RootPath = Path.GetDirectoryName(Application.dataPath);
        }
    }
}
