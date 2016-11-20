using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

public class ResourceLocker : EditorWindow
{
    [MenuItem("Window/Resource Locker")]
    public static void OnOpenedWindow()
    {
        GetWindow<ResourceLocker>().Show();
    }

    private void OnGUI()
    {
        using (new GUILayout.HorizontalScope())
        {
            if (GUILayout.Button("start lock"))
            {
                EditorApplication.LockReloadAssemblies();
                //AssetDatabase.StartAssetEditing();
            }

            if (GUILayout.Button("end lock"))
            {
                //AssetDatabase.StopAssetEditing();
                EditorApplication.UnlockReloadAssemblies();
            }
        }

        if (GUILayout.Button("build"))
        {
            Debug.Log("build");
        }
    }

    //[DidReloadScripts]
    //public static void aaa()
    //{
    //    Debug.Log("hoge");
    //}
}
