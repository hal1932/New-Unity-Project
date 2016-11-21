using ScriptComposer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace EditorUtil
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
