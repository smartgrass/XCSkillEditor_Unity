using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;



public class EditorExtend 
{
    // Start is called before the first frame update


    [MenuItem("Tools/配置/打开技能配置")]
    public static void OpenSkillSetting()
    {
        string path = Application.dataPath.RemoveEnd("Assets") + "Tools/Config/Datas/SkillSetting.xlsx";
        EditorUtility.OpenWithDefaultApp(path);
        Debug.Log($"打开1");
    }

    [MenuItem("Tools/配置/打开技能配置位置")]
    public static void OpenSkillDir()
    {
        string path = Application.dataPath.RemoveEnd("Assets") + "Tools/Config/Datas";
        FileTool.OpenDir(path);
        Debug.Log($"打开1");
    }

    //ctrl + shift + ↑
    [MenuItem("Tools/配置/生成技能配置 %#_UP")]
    public static void SavaSkillSetting()
    {

        string path = Application.dataPath.RemoveEnd("Assets") + "Tools/";
        EdtUtil.RunBat("gen_code_json.bat", path);
        Debug.Log($"打开1");
    }    
    [MenuItem("Tools/配置/生成技能配置(debug)")]
    public static void SavaSkillSettingDebug()
    {
        string path = Application.dataPath.RemoveEnd("Assets") + "Tools/";
        EdtUtil.RunBat("gen_code_json-debug.bat", path);
        Debug.Log($"打开1");
    }




    class EdtUtil
    {
        public static System.Diagnostics.Process CreateShellExProcess(string cmd, string args, string workingDir = "")
        {
            var pStartInfo = new System.Diagnostics.ProcessStartInfo(cmd);
            pStartInfo.Arguments = args;
            pStartInfo.CreateNoWindow = false;
            pStartInfo.UseShellExecute = true;
            pStartInfo.RedirectStandardError = false;
            pStartInfo.RedirectStandardInput = false;
            pStartInfo.RedirectStandardOutput = false;
            if (!string.IsNullOrEmpty(workingDir))
                pStartInfo.WorkingDirectory = workingDir;
            return System.Diagnostics.Process.Start(pStartInfo);
        }

        public static void RunBat(string batFile, string workingDir)
        {
            string path = Path.Combine(workingDir, batFile);
            if (!System.IO.File.Exists(path))
            {
                Debug.LogError("bat文件不存在：" + path);
            }
            else
            {
                EdtUtil.RunBat(batFile, "", workingDir);
            }
        }

        private static void RunBat(string batfile, string args, string workingDir = "")
        {
            var p = CreateShellExProcess(batfile, args, workingDir);
            p.Close();
        }

        public static string FormatPath(string path)
        {
            path = path.Replace("/", "\\");
            if (Application.platform == RuntimePlatform.OSXEditor)
                path = path.Replace("\\", "/");
            return path;
        }
    }

}
