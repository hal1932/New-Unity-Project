///#load "Class2.cs"
using UnityEngine;

public class Class1
{
    public void Test()
    {
        Debug.Log("hoge1");
        new Class2().Test();
    }
}
