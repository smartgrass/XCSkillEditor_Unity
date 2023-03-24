using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UIElements.VisualElement;
using Flux;

public class FindTextWindow : EditorWindow
{
    string str1;

    GameObject textObj;
    static GameObject targetObj;
    static FindTextWindow instance;
    static SerializedProperty serPro;
    //菜单工具
    [MenuItem("Tools/FindText")]
    static void ShowWindow()
    {
        instance = GetWindow<FindTextWindow>();
        instance.Show();
        instance.titleContent = new GUIContent("这是标题");
        //位置和窗口大小
        instance.position = new Rect(50, 50, 330, 200);
    }

    //				
    [MenuItem("CONTEXT/FSequence/Show")]
    private static void NewMenuOption(MenuCommand menuCommand)
    {
        var com = menuCommand.context as FSequence;
        com.Content.transform.hideFlags &= ~HideFlags.HideInHierarchy;
    }


    static void FindAssetsUsingSearchFilter(string name)
    {
        if (name.EndsWith("(clone)"))
        {
            name = name.Substring(0, name.Length - 7 - 1);
        }
        // Find all assets labelled with 'concrete' :
        var guids = AssetDatabase.FindAssets(name ,new string[] { "Assets/Resource"});
        foreach (var guid in guids)
        {
            string path  = AssetDatabase.GUIDToAssetPath(guid);
            targetObj = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
        }
    }

    void Clear()
    {
        targetObj = null;
        textObj = null;
        str1 = null;
    }


     void SearchText()
    {
        string searchStr = str1;
        if (string.IsNullOrEmpty(searchStr))
            return;

        List<Text> allText =new List<Text>(  Object.FindObjectsOfType<Text>( ));

        foreach (var item in allText)
        {
            if (item.text.Contains(searchStr))
            {
                Select(item.gameObject);
                FindAssetsUsingSearchFilter(item.transform.root.gameObject.name);
                textObj = item.gameObject;
            }
        }
    }


    public static void Select(Object obj)
    {
        Selection.objects =new Object[] { obj };
    }

    public static void Select(Object[] objs)
    {
        Selection.objects = objs;
    }

    private void OnGUI()
    {
        //搜索场景所有text
        EditorGUILayout.Separator();
        int fontSize = GUI.skin.textField.fontSize;
        int inputFontSize = GUI.skin.textField.fontSize;


        GUI.skin.label.fontSize = 20; //字体
        GUI.skin.textField.fontSize = 18;



        GUILayout.Label("Text内容查找");

        EditorGUILayout.BeginHorizontal();
        { 
            str1 = GUILayout.TextField(str1, new[] { GUILayout.Height(22), GUILayout.Width(240) });

            if (GUILayout.Button(new GUIContent("查找"), new[] { GUILayout.Height(20) }))
            {
                SearchText();
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Separator();
        EditorGUILayout.Separator();

        GUI.skin.label.fontSize = 10;
        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Label("Text" ,new[] {GUILayout.Width(45),});
            textObj = EditorGUILayout.ObjectField("", textObj, typeof(GameObject), true) as GameObject;

        }
        EditorGUILayout.EndHorizontal();

        //SerializeFieldClass serObj = new SerializeFieldClass();
        //var serializedObject = new SerializedObject(serObj);

        //serPro = serializedObject.GetIterator();
        //EditorGUILayout.PropertyField(serPro);
        //serObj = EditorGUILayout.ObjectField("", serObj, typeof(SerializeFieldClass), true) as SerializeFieldClass;

        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Label("panel", new[] { GUILayout.Width(45), });
            targetObj = EditorGUILayout.ObjectField("", targetObj, typeof(GameObject), true) as GameObject;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Separator();
        if (GUILayout.Button(new GUIContent("清空"), new[] { GUILayout.Height(20), GUILayout.Width(80) }))
        {
            Clear();
        }

        //还原字体大小
        GUI.skin.label.fontSize = fontSize;
        GUI.skin.textField.fontSize = inputFontSize;
    }

    [MenuItem("Assets/LogType")]
    private static void LogType()
    {
        var obj = Selection.activeObject;
        Debug.Log("yns  " + obj.name + "  " + obj.GetType());
    }

}


[SerializeField]
public class SerializeFieldClass :Object
{
    public int id = 0;
    //public List<string> levelList;
}
