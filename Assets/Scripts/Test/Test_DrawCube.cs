using UnityEngine;
using System.Collections;
using XiaoCao;
using System.Collections.Generic;
using Flux;

public class Test_DrawCube : MonoBehaviour
{
    public bool IsShow = true;
    public Color drawColor;
    public Dictionary<FTrack, Dictionary<CubeRange, Transform>> drawDic = new Dictionary<FTrack, Dictionary<CubeRange, Transform>>(); 

    public void Regist(FTrack track)
    {
        if (!drawDic.ContainsKey(track))
            drawDic.Add(track, new Dictionary<CubeRange, Transform>());
    }

    public void Add(FTrack track, CubeRange rang, Transform transform)
    {
        try
        {
            if (!drawDic[track].ContainsKey(rang))
            {
                drawDic[track].Add(rang, transform);
            }
        }
        catch
        {
            Debug.Log("yns  bug");
        }

    }
    public void Remove(FTrack track, CubeRange rang)
    {
        drawDic[track]?.Remove(rang);
    }
    public void RemoveAll(FTrack track)
    {
        if(drawDic.ContainsKey(track))
            drawDic[track].Clear();
    }

    [NaughtyAttributes.Button]
    void ClearAll()
    {
        drawDic.Clear();
    }

    private void OnDrawGizmos()
    {
        if (!IsShow)
        {
            return;
        }

        foreach (var dic in drawDic)
        {
            foreach (var item in dic.Value)
            {
                DrawCollider(item.Key.pos, item.Key.size,item.Key.rotation, item.Value);
            }
        }
    }

    void DrawCollider(Vector3 center, Vector3 size, Vector3 rota, Transform targetTran)
    {
        var rotation = targetTran.rotation;
        var angle = rotation.eulerAngles + rota;
        
        Gizmos.matrix = Matrix4x4.TRS(targetTran.position, Quaternion.Euler(angle), targetTran.lossyScale);
        Gizmos.color = drawColor;
        Gizmos.DrawCube(center, size);
        Gizmos.matrix = Matrix4x4.identity;

        

    }
}
