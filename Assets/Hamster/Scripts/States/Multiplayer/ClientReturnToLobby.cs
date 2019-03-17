using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
namespace Hamster.States
{
    //  This client is has finished the game and needs to return to the Lobby from OpenMatch.
    public class ClientReturnToLobby : BaseState
    {
        public NetworkManager manager;
        public CustomNetworkManagerHUD hud;
        JsonStartupConfig config;
        string lobbyAddress;
        int lobbyPort;

        private string myDebugMsg;
        bool isClientSceneAddPlayerCalled = false; //  must have called ClientScene.AddPlayer in one way or another

        void GetPointers()
        {
            if (manager == null || hud == null)
            {

                manager = UnityEngine.GameObject.FindObjectOfType<NetworkManager>();
                if (manager != null)
                {
                    hud = manager.GetComponent<CustomNetworkManagerHUD>();
                }
                else
                {
                    Debug.LogError("ClientInGame.GetPointers could not find NetworkManager!\n");
                }
            }

        }
        override public void Initialize()
        {
            Debug.LogWarning("ClientReturnToLobby.Initialize\n");
            GetPointers();
            if (hud != null)
                hud.ReadConfig();
            Shutdown(); //  close down. But then connect to "lobby" server.
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        void Shutdown()
        {
            manager.StopClient();
            Debug.LogWarning("ClientReturnToLobby.Shutdown\n");
            NetworkClient.ShutdownAll();    //  
        }
        public override void OnGUI()
        {
            hud.scaledTextBox(myDebugMsg);
        }

        NetworkConnection isConnectedToLobbyServer()
        {
            NetworkConnection conn = null;
            foreach (NetworkClient client in NetworkClient.allClients)
            {

                if ((null != client.connection) && (client.connection.address == lobbyAddress && client.connection.isConnected))
                {
                    return client.connection;
                }
            }
            return conn;
        }

        //  attempt to go back to the lobby server that is the IP in our config file.
        void ConnectToLobbyServer()
        {
            if (config == null)
                config = hud.ReadConfig(manager);

            lobbyAddress = config.startupConfig.serverIP;
            lobbyPort = System.Convert.ToInt32(config.startupConfig.serverPort);
            string serverRedirectMsg = "Try lobby connect: ip=" + lobbyAddress + ":" + lobbyPort.ToString();
            Debug.LogWarning(serverRedirectMsg);
            if (null == isConnectedToLobbyServer())
            {
                manager.StartClient();  //  once this is fired off, we should go into a waiting state.
                if (hud != null)
                {
                    hud.showClientDebugInfoMessage(serverRedirectMsg);
                }
            }
        }
        // Update is called once per frame
        public override void Update()
        {
            GetPointers();
            if (hud != null)
            {
                myDebugMsg = "ClientReturnToLobby: nPlr=" + manager.numPlayers.ToString() + " nClients=" + NetworkClient.allClients.Count.ToString() + "\n\tNetClient.active=" + NetworkClient.active.ToString();
                Debug.Log(myDebugMsg);
                ConnectToLobbyServer();
            }
        }
    }
}   //  Hamster.States