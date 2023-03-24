using Mirror;
using System;
using System.Collections;
using UnityEngine;

namespace XiaoCao
{

    public class SceneScript : NetworkBehaviour
    {
        public static SceneScript _instance = null;
        public static SceneScript Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<SceneScript>();
                }
                return _instance;
            }
        }

        private PlayerMgr playerMgr => PlayerMgr.Instance;

        [SyncVar(hook = nameof(OnGameModeChanged))]
        public GameMode gameMode;

        public override void OnStartServer()
        {
            Debug.Log($"yns  OnStartServer");
            base.OnStartServer();
            if (isServer)
            {
                gameMode = ResFinder.SoUsingFinder.DebugSo.gameMode;
            }
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            Debug.Log($"yns  OnStartLocalPlayer");
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            _instance = this;
            Debug.Log($"yns  OnStartClient");
        }

        [ContextMenu("AddPlayer")]
        private void AddPlayer()
        {
            Debug.Log($"yns  AddPlayer ");
            if (NetworkClient.localPlayer == null)
            {
                NetworkClient.AddPlayer();
            }
        }

        private void OnGameModeChanged(GameMode old, GameMode newInt)
        {
            Debug.Log($"yns model change {old}->{newInt}");
            PlayerMgr.Instance.curMode = newInt;
            PlayerMgr.Instance.ApplyAllAgentTag();
        }

        public void GetAgentTag(PlayerState player)
        {
            if (player.isTruePlayer && player.IsLocal)
            {
                SetTag();
            }
        }

        public void SetTag()
        {
            int truePlayerCount = playerMgr.truePlayerList.Count;
            playerMgr.LocalPlayer.CmdSetTag ( truePlayerCount % 2 == 0 ? AgentTag.PlayerA : AgentTag.PlayerB);
        }


    }
}