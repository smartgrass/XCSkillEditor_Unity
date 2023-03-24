using DG.Tweening;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using XiaoCao;

public class DebugForTest : MonoBehaviour
{
    //public PlayerState state;
    //public PlayerState state2;

    public static bool IsForDebug = false;

    public string message;
    //public int lv;


    [NaughtyAttributes.Button()]
    public void SwitchScene()
    {
        SceneManager.LoadScene(message);
    }

    //[NaughtyAttributes.Button()]
    //public void TestSendMessage3()
    //{
    //    PlayerMessge messge = new PlayerMessge();
    //    messge.StrMes = message;
    //    PlayerManager.Instance.SendNetMessage(PlayerEventName.String, messge);
    //}


    public Animator animator;
    public Ease ease = Ease.OutQuad;
    public Vector3 jumpVec3 = Vector3.up; //这里可以设计为地面位置
    public float jumpPower = 5;
    public int numJumps = 1;

    public float duration = 1.5f;
    [NaughtyAttributes.Button]
    public virtual void Jump()
    {
        animator = GetComponent<Animator>();

        Rigidbody rigidbody = GetComponent<Rigidbody>();
        //transform.DOShakePosition
        rigidbody.DOJump(jumpVec3, jumpPower, numJumps, duration).SetEase(ease);
        if (animator)
        {
            animator.Play(message,0,normalizedTime:0);
        }

    }

    [NaughtyAttributes.Button]
    public virtual void TestValue()
    {
        //Rigidbody rigidbody = GetComponent<Rigidbody>();
        CharacterController cc = GetComponent<CharacterController>();
        //cc.DOHit( jumpPower, duration);
    }


    [NaughtyAttributes.Button]
    public virtual void AddVecY()
    {
        CharacterController cc = GetComponent<CharacterController>();
        var animcctrl = PlayerMgr.Instance.LocalPlayer.playerMover;
        animcctrl.velocity.y += jumpPower;
        //cc.Move(jumpVec3);
    }
}
