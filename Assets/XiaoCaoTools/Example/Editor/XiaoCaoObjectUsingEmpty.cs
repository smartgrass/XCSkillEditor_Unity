using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XiaoCao;

public class XiaoCaoObjectUsingEmpty : XiaoCaoWindow
{

    [MenuItem("Tools/XiaoCao/对象收藏夹Empty")]
    static void Open()
    {
        OpenWindow<XiaoCaoObjectUsingEmpty>("对象收藏夹Empty");
    }


    [NaughtyAttributes.Expandable]
    public ScriptableObject objectUsing;
}
