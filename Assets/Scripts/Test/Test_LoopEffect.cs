using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
public class Test_LoopEffect : MonoBehaviour
{
    public Test_FixCamera fixCamera;
    public int curIndex = 0;
    public Transform CurEffect;


    private void Start()
    {
        CurEffect = fixCamera.target;
    }

    [Button]
    public void Loop()
    {
        int count = transform.childCount;
        for (int i = 0; i < count; i++)
        {
            var ps = transform.GetChild(i).GetComponentInChildren<ParticleSystem>().main;
            ps.loop = true;

        }
    }


    [Button()]
    public void Next()
    {
        int count = transform.childCount;
        fixCamera.target = transform.GetChild(curIndex);
        CurEffect = fixCamera.target;
        curIndex++;
        if(curIndex == count)
        {
            curIndex = 0;
        }

    }
    [Button()]
    public void Last()
    {
        int count = transform.childCount;
        fixCamera.target = transform.GetChild(curIndex);
        CurEffect = fixCamera.target;
        curIndex--;
        if (curIndex == -1)
        {
            curIndex = count-1;
        }
    }

}
