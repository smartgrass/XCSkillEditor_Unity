using Assets.Scripts.Enemy;
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using XiaoCao;

namespace XiaoCao
{
    public class PlayerMgr : Singleton<PlayerMgr>
    {
        #region NetWorkManager
        public NetworkIdentity LocalIdentity => NetworkClient.connection.identity;
        public uint LocalNetId => NetworkClient.connection.identity.netId;
        public bool isNetActive => NetworkServer.active;


        public PlayerState LocalPlayer;

        public bool IsLocalPlayerReady => LocalPlayer != null;

        public SkillCDTimer skillCD = new SkillCDTimer();

        public GameMode curMode;

        #endregion

        #region 委托
        public delegate void PlayerEvent(uint netId);


        private PlayerEvent OnLoginEvent; //登录
        private PlayerEvent OnStopEvent; //登出
        private PlayerEvent OnChangeEvent; //登录或者登出
        private PlayerEvent OnValueChangeEvent; //数值改变时 或登录或者登出


        public void AddListener(ClientEventType eventType, PlayerEvent player)
        {
            if (eventType == ClientEventType.Start)
                OnLoginEvent += player;
            else if ((eventType == ClientEventType.Stop))
                OnStopEvent += player;
            else if ((eventType == ClientEventType.Change))
                OnChangeEvent += player;
            else if ((eventType == ClientEventType.ValueChange))
                OnValueChangeEvent += player;
        }
        public void RemoveListener(ClientEventType eventType, PlayerEvent player)
        {
            if (eventType == ClientEventType.Start)
                OnLoginEvent -= player;
            else if ((eventType == ClientEventType.Stop))
                OnStopEvent -= player;
            else if ((eventType == ClientEventType.Change))
                OnChangeEvent -= player;
            else if ((eventType == ClientEventType.ValueChange))
                OnValueChangeEvent -= player;
        }

        private bool isPlayerStarted;
        private Action playerStartAciton;
        public void AddPlayerStartedAction(Action addAciton)
        {
            if (isPlayerStarted)
            {
                //如果已经开始就直接调用
                addAciton.Invoke();
            }
            else
            {
                playerStartAciton += addAciton;
            }
        }
        public void DoPlayerStart()
        {
            isPlayerStarted = true;
            playerStartAciton?.Invoke();
        }

        #endregion

        public Dictionary<uint, MonoAttacker> MonoAttackerDic = new Dictionary<uint, MonoAttacker>();
        public Action<uint> removeAckerAction;

        public Dictionary<uint, PlayerState> truePlayerDic = new Dictionary<uint, PlayerState>();

        public List<PlayerState> truePlayerList = new List<PlayerState>();

        public List<PlayerState> AgentList = new List<PlayerState>(); //All PlayerState


        public MonoAttacker GetAcker(uint netID)
        {
            if (!MonoAttackerDic.ContainsKey(netID)) return null;
            return MonoAttackerDic[netID];
        }

        public void ApplyAllAgentTag()
        {
            if (LocalPlayer == null)
            {
                return;
            }
            foreach (var item in AgentList)
            {
                item.SetColiderLayer();
            }
        }


        public void RegisterAttacker(MonoAttacker attacker)
        {
            uint netID = attacker.netId;
            if (!MonoAttackerDic.ContainsKey(netID))
            {
                MonoAttackerDic.Add(netID, attacker);
            }
        }
        public void DisRegisterAttacker(uint netID)
        {
            if (MonoAttackerDic.ContainsKey(netID))
            {
                MonoAttackerDic.Remove(netID);
            }
            removeAckerAction?.Invoke(netID);
        }


        public void Register(PlayerState player)
        {
            uint netID = player.netId;
            Debug.Log("yns PlayerManager Register " + netID);
            SceneScript.Instance.GetAgentTag(player);
            AgentList.Add(player);
            if (player.isTruePlayer)
            {
                truePlayerDic.Add(netID, player);
                truePlayerList.Add(player);
            }
            OnLoginEvent?.Invoke(netID);
            OnChangeEvent?.Invoke(netID);
            OnValueChangeEvent?.Invoke(netID);

        }
        public void DisRegister(uint netID)
        {
            OnChangeEvent?.Invoke(netID);
            OnStopEvent?.Invoke(netID);
            OnValueChangeEvent?.Invoke(netID); 
        }

        public void UpdatePlayerValue(uint netID = 0)
        {
            OnValueChangeEvent?.Invoke(netID);
        }

        public void SendBool(uint netId, bool isLocalOnly,string name, bool msg)
        {
            var acker = GetAcker(netId);
            if (acker == null)
                return;

            if (!isLocalOnly || (isLocalOnly && acker.IsLocal))
            {
                acker.SetBool(name, msg);
                Debug.Log($"yns Msg {name} {msg}");
            }
        }

        public void SendAll(uint netId, bool isLocalOnly, string name, float num, bool isOn =false, string str="")
        {
            var acker = GetAcker(netId);
            if (acker == null)
                return;

            if (!isLocalOnly || (isLocalOnly && acker.IsLocalTruePlayer))
            {
                acker.SendAll(name, num, isOn, str);
            }
        }


        public void SendNetMessage(PlayerNetEventName eventName, PlayerMessge messge)
        {
            LocalPlayer.SendNetMessage(eventName, messge);
        }

        public void AddFakePlayer(Vector3 startPos, bool isAi,
            AgentTag agentTag, AgentModelType agentName = AgentModelType.Player)
        {
            if (!Application.isPlaying)
                return;
            if (!NetworkClient.ready)
                return;

            if (!LocalPlayer.isServer)
            {
                Debug.Log($"yns LocalPlayer.isServer false ");
                return;
            }

            GameObject prfab = NetworkManager.singleton.playerPrefab;
            if (agentName != AgentModelType.Player)
            {
                prfab = NetworkManager.singleton.spawnPrefabs[(int)agentName - 1];
                Debug.Log($"yns enemy {prfab.name} ");
            }

            GameObject player = GameObject.Instantiate(prfab);

            Debug.Log($"yns  Add Player");
            NetworkServer.Spawn(player);

            //这边的修改都是本地的修改 对服务器不起作用 , 出生数据只认预制体
            PlayerState state = player.GetComponent<PlayerState>();
            state.transform.position = startPos;

            //对于SyncVar 的数据, 会hook过去
            state.isTruePlayer = false;
            state.AgentTag = agentTag; 

            state.isEnableAck = true;

            if (isAi)
            {
                state.ActiveAI();
            }

            NetworkIdentity id = state.GetComponent<NetworkIdentity>();
            if (!id.hasAuthority)
            {
                //
                //LocalConnectionToServer connectionToServer = (LocalConnectionToServer)NetworkClient.connection;
                //id.AssignClientAuthority(NetworkServer.connections[connectionToServer.connectionId]);
                id.AssignClientAuthority(NetworkClient.localPlayer.connectionToClient);
            }

        }

    }


}
