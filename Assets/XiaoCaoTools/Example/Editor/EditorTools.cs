using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class EditorReferenceTools
{
    [MenuItem("Assets/Check/获取预制体图片引用")]
    static void CheckDependeces()
    {
        List<string> prefabList = new List<string>();
        foreach (var item in Selection.objects)
        {
            if (item is GameObject)
            {
                prefabList.Add(AssetDatabase.GetAssetPath(item));
            }
        }
        GetPngDependeces(prefabList);
    }
    public static List<string> GetSelectsPath(Object[] objects)
    {
        List<string> prefabList = new List<string>();
        foreach (var item in objects)
        {
            prefabList.Add(AssetDatabase.GetAssetPath(item));
        }
        return prefabList;
    }

    public static List<string> GetPngDependeces(List<string> prefabList)
    {
        string[] allDependencies = AssetDatabase.GetDependencies(prefabList.ToArray(), true);
        List<string> allPngDependecies = new List<string>();
        foreach (var item in allDependencies)
        {
            if (item.EndsWith(".png"))
            {
                allPngDependecies.Add(item);
            }
        }
        return allPngDependecies;
    }

    public static List<string> GetAllDependeces(List<string> prefabList)
    {
        string[] allDependencies = AssetDatabase.GetDependencies(prefabList.ToArray(), true);
        List<string> allPngDependecies = new List<string>();
        foreach (var item in allDependencies)
        {
            allPngDependecies.Add(item);
        }
        return allPngDependecies;
    }

    public static string[] KnowAllPicture(string dirFullPath)
    {
        List<string> liststring = new List<string>();

        var images = Directory.GetFiles(dirFullPath, ".", SearchOption.AllDirectories).Where(s => s.EndsWith(".png") || s.EndsWith(".jpg"));

        foreach (var i in images)
        {
            var str = i.Replace(Application.dataPath, "");
            var strpath = str.Replace("\\", "/");
            strpath = "Assets" + strpath;
            liststring.Add(strpath);
        }
        return liststring.ToArray();
    }

    //endsWith  ".png"
    public static string[] KnowAllTypeFile(string dirFullPath, string[] endsWith)
    {
        List<string> liststring = new List<string>();

        var images = Directory.GetFiles(dirFullPath, ".", SearchOption.AllDirectories).
        Where(
            (s) =>
            {
                foreach (var item in endsWith)
                {
                    if (s.EndsWith(item))
                        return true;
                }
                return false;
            }
        );

        foreach (var i in images)
        {
            var str = i.Replace(Application.dataPath, "");
            var strpath = str.Replace("\\", "/");
            strpath = "Assets" + strpath;
            liststring.Add(strpath);
        }
        return liststring.ToArray();
    }

    //路径转全局路径
    public static string AssetPathToFullPath(string assestPath)
    {
        string shortPath = assestPath.Remove(0, "Assets/".Length);
        string dirPath = Path.Combine(Application.dataPath, shortPath);
        return dirPath;
    }

    public static void MoveTextureToUnPackage(List<string> moveList, string newPath)
    {
        for (int i = 0; i < moveList.Count; i++)
        {
            MoveTextureToUnPackage(moveList[i], newPath);
        }
    }

    //将资源移动到别处 并标记为unsing
    public static void MoveTextureToUnPackage(string oldPath, string newPath)
    {
        string pngName = oldPath.Split('/').Last();

        string targetNewPath = $"{newPath}/{pngName}";

        if (oldPath == targetNewPath)
        {
            Debug.Log($"yns path same {oldPath}");
            return;
        }

        if (File.Exists(targetNewPath))
        {
            int random = Random.Range(0, 1000);
            targetNewPath = $"{newPath}/Un{random}_{pngName}";
            Debug.Log($"yns  Exits");
        }

        AssetDatabase.MoveAsset(oldPath, targetNewPath);
    }


    [MenuItem("Assets/Check/查找图片当前使用")]
    public static void FindPngInAll()
    {
        PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
        if (prefabStage == null)
        {
            FindPngInGame();
            return;
        }

        GameObject prefabRoot = prefabStage.prefabContentsRoot;
        if (prefabRoot == null)
            return;

        Debug.Log(prefabRoot);
        List<Object> finds = new List<Object>();

        //var prefab = PrefabStageUtility.GetCurrentPrefabStage();
        var sprites = prefabRoot.GetComponentsInChildren<Image>(true);
        //Debug.Log("yns  prefab " + prefab.name);
        var path = AssetDatabase.GetAssetPath(Selection.activeObject);

        var cur = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        var tex = AssetDatabase.LoadAssetAtPath<Texture>(path);
        bool isFind = false;
        foreach (var item in sprites)
        {
            //Debug.Log("yns  ite" + item.name);
            if (item.sprite == cur)
            {
                finds.Add(item.gameObject);
                isFind = true;
            }
            else if (item.material != null)
            {
                if (item.material.mainTexture == tex)
                {
                    finds.Add(item.gameObject);
                    isFind = true;
                }
            }
        }

        var rawImages = prefabRoot.GetComponentsInChildren<RawImage>(true);

        foreach (var item in rawImages)
        {
            if (item.material.mainTexture == tex)
            {
                finds.Add(item.gameObject);
                isFind = true;
            }
        }

        if (!isFind)
        {
            Debug.Log($"yns no Find Using");
        }
        Selection.objects = finds.ToArray();

    }

    private static void FindPngInGame()
    {
        var Images = GameObject.FindObjectsOfType<Image>(true);
        List<Sprite> spriteList = new List<Sprite>();

        foreach (var item in Selection.objects)
        {
            if (item is Texture2D)
            {
                spriteList.Add(AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GetAssetPath(item)));
            }
        }

        List<Object> finds = new List<Object>();

        bool isFind = false;
        foreach (var item in Images)
        {
            if (spriteList.Contains(item.sprite))
            {
                isFind = true;
                Debug.Log("yns  " + item.name);
                finds.Add(item.gameObject);
            }
        }
        if (isFind)
        {
            Debug.Log("yns findCount  " + finds.Count);
            Selection.objects = finds.ToArray();
        }
        else
        {
            Debug.Log("yns no find ");
        }
    }

}


public class EditorAssetExtend
{

    [MenuItem("Assets/Check/移动到Resources")]
    static void CheckMoveToResource()
    {
        foreach (var item in Selection.objects)
        {
            string oldPath = AssetDatabase.GetAssetPath(item);
            MoveToResources(oldPath);
        }
    }
    [MenuItem("Assets/Check/移动到UsingAnim")]
    static void CheckMoveToAnim()
    {
        foreach (var item in Selection.objects)
        {
            string oldPath = AssetDatabase.GetAssetPath(item);
            MoveToDir(oldPath, "Assets/_Res/Anim/Using");
        }
    }

    public static string MoveToResources(string oldPath)
    {
        //"Assets/Resources/"
        string fileName = Path.GetFileName(oldPath);
        string newPath = "Assets/Resources/SkillEffet/" + fileName;
        Debug.Log($"yns {newPath}");
        AssetDatabase.MoveAsset(oldPath, newPath);
        return newPath;
    }

    public static string MoveToDir(string oldPath,string dir)
    {
        //"Assets/Resources/"
        string fileName = Path.GetFileName(oldPath);
        string newPath = dir +"/"+ fileName;
        Debug.Log($"yns {newPath}");
        AssetDatabase.MoveAsset(oldPath, newPath);
        return newPath;
    }

    [MenuItem("Assets/SavaThisAssets")]
    private static void SavaSelectAsset()
    {
        SavaAsset(Selection.activeObject);
    }
    //, false, 2
    [MenuItem("Assets/XiaoCaoTools/移除预制体组件")]
    public static void RemoveCom()
    {
        foreach (var item in Selection.objects)
        {
            var path = AssetDatabase.GetAssetPath(item);

            GameObject root = PrefabUtility.LoadPrefabContents(path);
            if (root)
            {

                var rglist = root.GetComponentsInChildren<Rigidbody>(true);
                foreach (var rb in rglist)
                {
                    GameObject.DestroyImmediate(rb);
                }
                //var colList = root.GetComponentsInChildren<Collider>(true);
                //foreach (var col in colList)
                //{
                //    DestroyImmediate(col.gameObject);
                //}
                PrefabUtility.SaveAsPrefabAsset(root, path, out bool success);
            }
        }

    }

    [MenuItem("Assets/LogThisType")]
    private static void LogType()
    {
        Selection.activeObject.GetType().Name.LogStr(Selection.activeObject.name); 
    }
    [MenuItem("CONTEXT/Object/LogThis")]
    private static void LogCur(MenuCommand menuCommand)
    {
        var obj = menuCommand.context;
        LogToStringTool.LogObjectAll(obj, obj.GetType());
    }    
    
    public static void SavaAsset(UnityEngine.Object obj)
    {
        if (obj == null)
            return;

        EditorUtility.SetDirty(obj);
        if (obj is ScriptableObject)
        {
            var serializedObject = new SerializedObject(new UnityEngine.Object[] { obj}, obj);
            serializedObject.ApplyModifiedProperties();
            Debug.Log($"yns  ApplyModifiedProperties");
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
