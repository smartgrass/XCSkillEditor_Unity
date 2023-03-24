// CompilingFinishedCallback.cs
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
/// <summary>
/// 编译完成的回调
/// </summary>
public static class CompilingFinishedCallback
{
    private static string TmpMethodNamesKey = "CompilingFinishedCallback_TmpMethodNames";

    public static void Set<T>(string methodName, string arg = null)
    {
        var type = typeof(T);
        string methoedName = type.FullName + "." + methodName + "(" + arg + ")";
        AddTmpMethodName(methoedName);
    }

    static string[] GetTmpMethodNameArr()
    {
        return GetTmpMethodNames().Split(';').Where(value => value != null && value.Trim() != "").ToArray();
    }

    static void AddTmpMethodName(string value)
    {
        SetTmpMethodNames(GetTmpMethodNames() + ";" + value);
    }

    static string GetTmpMethodNames()
    {
        if (EditorPrefs.HasKey(TmpMethodNamesKey))
            return EditorPrefs.GetString(TmpMethodNamesKey);
        return "";
    }

    static void SetTmpMethodNames(string value)
    {
        EditorPrefs.SetString(TmpMethodNamesKey, value);
    }

    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded()
    {
        var methods = GetTmpMethodNameArr();
        foreach (var method in methods)
        {
            var pointIndex = method.LastIndexOf(".");
            var left = method.LastIndexOf("(");
            var right = method.LastIndexOf(")");
            string className = method.Substring(0, pointIndex);
            string methodName = method.Substring(pointIndex + 1, left - pointIndex - 1);
            string argName = method.Substring(left + 1, right - left - 1);
            Type type = Type.GetType(className);
            if (type == null)
            {
                Debug.LogError("type == null");
                continue;
            }
            var method1 = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, null, new Type[] { }, null);
            var method2 = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, null, new Type[] { typeof(string) }, null);
            if (method1 == null && method2 == null)
            {
                Debug.LogError("method1 == null && method2 == null");
                continue;
            }
            if (method1 != null && argName.Trim() == "")
                method1.Invoke(null, null);
            if (method2 != null && argName.Trim() != "")
                method2.Invoke(null, new object[] { argName });
        }
        SetTmpMethodNames("");
    }


    //示例使用
    public class TestEditorCallBack
    {

        [MenuItem("Tools/TestEditorCallBack")]
        public static void Start()
        {
            CompilingFinishedCallback.Set<TestEditorCallBack>("Log");
            CompilingFinishedCallback.Set<TestEditorCallBack>("Log", "arg");
        }

        private static void Log()
        {
            Debug.Log("log");
        }

        private static void Log(string arg)
        {
            Debug.Log(arg);
        }
    }
}


