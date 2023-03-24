using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XiaoCao;
using ButtonAttribute = XiaoCao.ButtonAttribute;

public class XiaoCaoEexample_1 : XiaoCaoWindow
{
    [MenuItem("Tools/XiaoCao/Eexample_1")]
    static void Open()
    {
        OpenWindow<XiaoCaoEexample_1>("Eexample_1");
    }
    //[Label] [Dropdown] [ShowIf] [Button]


    [XCLabel("这是str1")]
    public string str1;
    [XCLabel("这是str2")]
    public string str2;

    public Color color = Color.red;

    [OnValueChanged("fun5")]
    [Range(0,1)]
    public float value;

    [Dropdown("GetBoolValues")]
    public bool isShow = false;

    [Dropdown("dirList")]
    public string select ="1";

    string[] dirList =new[] {"1","2","3"};



    private DropdownList<bool> GetBoolValues()
    {
        return new DropdownList<bool>()
        {
            { "是",   true},
            { "不是",  false },
        };
    }

    [ShowIf("isShow")]
    public Object[] assets;

    //public UnityEvent action;

    //4 表示按钮的位置
    [ShowIf("isShow")]
    [Button("Button1", 6)]
    private void Fun1()
    {
        Debug.Log($"yns select={select} isWeapon={isShow}");
    }

    //-1表示放末尾, 最小是-10
    [Button("Button2", -1)]
    private void Fun4()
    {
        Debug.Log("yns  button2");
    }

    [Button("Button3", -1)]
    private void fun3()
    {
        Debug.Log($"yns Button3");
    }
    private void fun5()
    {
        Debug.Log($"yns value = {value}");
        color = Color.blue * value;
    }

    [Button("shader", -1)]
    private void fun6()
    {
        Debug.Log($"yns Button3");
        foreach (var item in Resources.FindObjectsOfTypeAll<Shader>())
        {
            Debug.Log($"shardar " + item.name  );
        } 

    }
}
