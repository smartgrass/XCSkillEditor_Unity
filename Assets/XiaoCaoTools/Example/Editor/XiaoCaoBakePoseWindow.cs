using NaughtyAttributes;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XiaoCao;
using ButtonAttribute = XiaoCao.ButtonAttribute;
using Object = UnityEngine.Object;

public class XiaoCaoBakePoseWindow : XiaoCaoWindow
{
    [MenuItem("Tools/XiaoCao/BakePoseWindow")]
    static void Open()
    {
        OpenWindow<XiaoCaoBakePoseWindow>();
    }

    public SkinnedMeshRenderer SkinMesh;
    public Object SavaFolder;
    private Mesh BakedMeshResult;


    [Button]
    public void GetMesh()
    {
        if (BakedMeshResult == null) //메쉬 데이터가 없을 경우 새로운 메쉬 데이터 생성
        {
            BakedMeshResult = new Mesh();
        }
        SkinMesh.BakeMesh(BakedMeshResult); //타겟 스킨메쉬 -> BakeMesh에 구움

        int random = UnityEngine.Random.Range(0, 10);
        SaveAseet(BakedMeshResult, "Bake"+DateTime.Now.Second.ToString()+"_"+ random);
    }

    public void SaveAseet(Object asset,string Name)
    {
        string path =AssetDatabase.GetAssetPath(SavaFolder);
        AssetDatabase.CreateAsset(asset, path +"/"+Name+ ".asset"); //创建资源,保存Asset

    }

    public void CompressMesh()
    {

    }


}
