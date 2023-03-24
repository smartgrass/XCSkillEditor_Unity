using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XiaoCao;

public class GameStartCall : MonoBehaviour
{
    public bool isPlayerManager = true;
    public bool isXCSceneManager = true;


    private void Awake()
    {
        var playerMan = PlayerMgr.Instance;
        var SceneMan = XCGameManager.Instance;
    }

}
