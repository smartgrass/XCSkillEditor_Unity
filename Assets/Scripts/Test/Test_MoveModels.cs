using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class Test_MoveModels : MonoBehaviour
{
    public int lineLen = 5;
    public Vector2 span;
    public bool isCenter;
    public float randomHight;
    // Update is called once per frame
    [Button]
    public void Move()
    {
        int count = transform.childCount;
        for (int i = 0; i < count; i++)
        {
            Vector2 pos = GetPosVec2(i, lineLen);
            var tf = transform.GetChild(i);
            var h = Random.Range(-randomHight / 2, randomHight);
            tf.position = new Vector3(span.x * pos.x, tf.position.y + h, span.y*pos.y);
        }

        if(isCenter)
        {
            int lineX = count >= 5 ? 5 : lineLen;
            int lineY = Mathf.RoundToInt((float)count / lineLen);
            lineX--;
            lineY--;

            Debug.Log($"yns {lineX} {lineY}");

            Vector3 offset = new Vector3((lineX * span.x) / 2, 0, (lineY * span.y) / 2);

            for (int i = 0; i < count; i++)
            {
                transform.GetChild(i).position -= offset;
            }
        }
    }

    //¶ÓÁÐ×ø±ê
    public Vector2Int GetPosVec2(int index,int lineLen)
    {
        int y = (index / lineLen); 
        int x = index - (lineLen * y ); 
        return new Vector2Int(x, y);
    }

}
