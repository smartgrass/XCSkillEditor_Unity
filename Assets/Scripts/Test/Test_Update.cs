using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using XiaoCao;

public class Test_Update : MonoBehaviour
{
    public CharacterController cc;
    public float g=9.8f;

    private void Start()
    {
        if (cc == null)
        {
            cc = transform.GetComponent<CharacterController>();
        }
    }

    public Vector3 move;
    public float time = 0.1f;
    [Button]
    public void OnMove()
    {
        cc.DOHit(move.y, move, time);
    }

    private void FixedUpdate()
    {
        cc.Move(g * Time.fixedDeltaTime*Vector3.down);
    }

}
