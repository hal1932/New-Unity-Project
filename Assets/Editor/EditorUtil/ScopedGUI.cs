using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;

namespace EditorUtil
{
    public class ChangeCheckScope : Disposable
    {
        public ChangeCheckScope(Action onChanged)
        {
            EditorGUI.BeginChangeCheck();
            SetDisposingAction(() =>
            {
                if (EditorGUI.EndChangeCheck())
                {
                    onChanged();
                }
            });
        }
    }

    public class LockReloadAssemblyScope : Disposable
    {
        public LockReloadAssemblyScope()
        {
            EditorApplication.LockReloadAssemblies();
            SetDisposingAction(() => EditorApplication.UnlockReloadAssemblies());
        }
    }

    public class LockReloadAssetScope : Disposable
    {
        public LockReloadAssetScope()
        {
            AssetDatabase.StartAssetEditing();
            SetDisposingAction(() => AssetDatabase.StopAssetEditing());
        }
    }
}
