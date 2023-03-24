using Mirror;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XiaoCao;

public class SkillEditorScene : MonoBehaviour
{
    [Header("Ìí¼ÓÒ»¸öNpc")]
    public bool AddOnStart = true;

    public AgentModelType agentName;

    public Vector3 startPos = Vector3.zero;

    private void Start()
    {
        if (AddOnStart)
        {
            StartCoroutine(WaitStart());
        }
    }

    [Button(enabledMode: EButtonEnableMode.Playmode)]
    public void AddNpc()
    {
        var setting = ResFinder.SoUsingFinder.DebugSo;
        PlayerMgr.Instance.AddFakePlayer(startPos, setting.AI,setting.addPlayerTag,agentName);
    }


    IEnumerator WaitStart()
    {
        while (!NetworkServer.active)
        {
            yield return null;
        }
        while(NetworkClient.localPlayer == null)
        {
            yield return null;
        }
        while (!NetworkClient.localPlayer.isActiveAndEnabled)
        {

            yield return null;
        }


        AddNpc();
    }
}
