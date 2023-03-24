using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XiaoCao;

public class XiaoCaoObjectUsing : XiaoCaoWindow
{

    [MenuItem("Tools/XiaoCao/对象收藏夹")]
    static void Open()
    {
        OpenWindow<XiaoCaoObjectUsing>("对象收藏夹");
    }


    [NaughtyAttributes.Expandable]
    public ObjectUsing objectUsing;


    public override void OnEnable()
    {
        base.OnEnable();
        string path = "Assets/XiaoCaoTools/Example/Ignore/ObjectUsing.asset";
        objectUsing = AssetDatabase.LoadAssetAtPath<ObjectUsing>(path);

        if (objectUsing == null)
        {
            var newSO = ScriptableObject.CreateInstance<ObjectUsing>();
            AssetDatabase.CreateAsset(newSO, path);
            objectUsing = AssetDatabase.LoadAssetAtPath<ObjectUsing>(path);
        }
    }
}
