using cfg;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using XiaoCao;
using SimpleJSON;
using Mirror;

public class SkillDebugWindow : XiaoCaoWindow
{

    //public string path  = "Assets";

    [MenuItem("Tools/XiaoCao/技能配置窗口")]
    static void Open()
    {
        OpenWindow<SkillDebugWindow>();
    }

    //[Button]
    //private void AddPlayer()
    //{
    //    if (!Application.isPlaying)
    //        return;
    //    GameObject prfab = NetworkManager.singleton.playerPrefab;

    //    GameObject player =  GameObject.Instantiate(prfab);
    //    NetworkServer.Spawn(player);
    //    PlayerState state = player.GetComponent<PlayerState>();
    //    //NetworkIdentity identity = state.GetComponent<NetworkIdentity>();
    //    //identity.isLocalPlayer = false;
    //    state.isEnableNet = true;
    //    state.transform.position = new Vector3(0, 0, 0);
    //}
    [Button("打开技能配置",0)]
    private void OpenSkillSetting()
    {
        EditorExtend.OpenSkillSetting();
    }
    [Button("打开配置位置",0)]
    private void SavaSkillDir()
    {
        EditorExtend.OpenSkillDir();
    }
    [Button("刷新技能配置",-1)]
    private void SavaSkillSetting()
    {
        EditorExtend.SavaSkillSetting();
    }       

    [Button("刷新技能配置(Debug)",-1)]
    private void SavaSkillSettingDebug()
    {
        EditorExtend.SavaSkillSettingDebug();
    }
    


}
