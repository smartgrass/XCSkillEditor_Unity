//using System.Collections;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;
using System;
using System.Net;
using System.Net.Sockets;
using Mirror.Discovery;

namespace XiaoCao
{
    public class LoginUIPanel : UIBase
    {
        public Text ipText;

        public InputField ipField;
        public Dropdown ipDropdown;

        public Button ServerStartBtn;

        public Button ConnetStartBtn;

        public Button FindServerBtn;


        private NetworkManager networkManager;
        private NetworkDiscovery networkDiscovery;

        readonly Dictionary<long, ServerResponse> discoveredServers = new Dictionary<long, ServerResponse>();
        Vector2 scrollViewPos = Vector2.zero;


        private void Awake()
        {
            var playerMan = PlayerMgr.Instance;
            //DontDestroyOnLoad(this);
        }

        private void Start()
        {
            ServerStartBtn.onClick.AddListener(OnServerStartBtn);
            ConnetStartBtn.onClick.AddListener(OnConnetStartBtn);
            FindServerBtn.onClick.AddListener(OnFindServerBtn);
            ipDropdown.onValueChanged.AddListener(OnDropChange);
            //ipDropdown
            var game = XCGameManager.Instance;

            ipText.text ="本地ip : "+ GetLocalIP().ToString();

            networkManager = GameObject.FindObjectOfType<NetworkManager>();
            networkDiscovery = networkManager.GetComponent<NetworkDiscovery>();
            networkDiscovery.OnServerFound.AddListener(OnServerFound);
        }

        private void OnServerFound(ServerResponse info)
        {
            discoveredServers[info.serverId] = info;
            List<string> options = new List<string>();
            ipDropdown.options.Clear();

            foreach (ServerResponse _info in discoveredServers.Values)
            {
                Debug.Log($"yns discoveredServers Add");
                options.Add(_info.EndPoint.Address.ToString());
            }
            options.IELogStr();
            ipDropdown.AddOptions(options);
            if (options.Count > 0)
            {
                OnDropChange(0);
            }

        }

        private void OnDropChange(int arg0)
        {
            Debug.Log($"yns  OnDropChange");
            //throw new NotImplementedException();
            ipField.text = ipDropdown.options[arg0].text;
        }

        private void OnFindServerBtn()
        {
            Debug.Log($"yns OnFindServerBtn ");
            discoveredServers.Clear();
            networkDiscovery.StartDiscovery();
        }


        private void OnConnetStartBtn()
        {
            Debug.Log($"yns  OnConnetStartBtn");
            SetConnetIP(ipField.text);
            //networkManager.autoCreatePlayer = true;
            networkDiscovery.StopDiscovery();
            networkManager.StartClient();
        }

        private void OnServerStartBtn()
        {
            //SetConnetIP("127.0.0.1");
            Debug.Log($"yns OnServerStartBtn ");
            SetConnetIP("localhost");
            discoveredServers.Clear();
            networkManager.StartHost();
            networkDiscovery.AdvertiseServer();
            //networkManager.ServerChangeScene()
        }

        //IEnumerator CheckConnet()
        //{
        //    //NetworkServer.isLoadingScene
        //}

        public void SetConnetIP(string ipStr)
        {
            networkManager.networkAddress = ipStr;
        }

        private IPAddress GetLocalIP()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }
            //???
            return null;
        }





        void Connect(string ip)
        {
            Uri url = new Uri(ip);
            networkDiscovery.StopDiscovery();
            NetworkManager.singleton.StartClient();
        }



    }

    public static class MirrorNetTool
    {
        //public static void FindSererPesponse(NetworkDiscovery networkDiscovery)
        //{

        //    networkDiscovery.StartDiscovery();
        //    networkDiscovery.OnServerFound.AddListener(OnServerFound);
        //    networkDiscovery.StopDiscovery();
        //}

        //private static void OnServerFound(ServerResponse arg0)
        //{

        //}
    }
}