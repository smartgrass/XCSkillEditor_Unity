using NaughtyAttributes;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XiaoCao;
using ButtonAttribute = XiaoCao.ButtonAttribute;
using Object = UnityEngine.Object;

public class XiaoCaoSetPositionWindow : XiaoCaoWindow
{
    [MenuItem("Tools/XiaoCao/XiaoCaoSetPositionWindow")]
    static void Open()
    {
        OpenWindow<XiaoCaoSetPositionWindow>();
    }

    [OnValueChanged("CheckTransfrom")]
    public Transform targetParent;

    
    [NaughtyAttributes.ReadOnly]
    public Transform[] Groups = new Transform[3];


    public Vector3 offset;
    [Range(0,1)]
    public float pro;


    public bool AutoFollow = false;


    private void Update()
    {
        if (AutoFollow)
        {
            if (targetParent!=null)
            {
                Vector3 start = Groups[0].position + offset;
                Vector3 end = Groups[1].position + offset;
                Groups[2].position = Vector3.Lerp(start, end, pro);
            }
        }
    }

    private void CheckTransfrom()
    {
        if(targetParent!=null)
        {
            for (int i = 0; i < 3; i++)
            {
                Groups[i] = targetParent.GetChild(i);
            }
        }
    }


}
