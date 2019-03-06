using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
namespace Hamster.States
{
    //  this is where the players from the "preOpenMatch" game are matched in OpenMatch and disconnected from their current game. This starts a new 4 player game with OpenMatch.
    public class ServerOpenMatchStart : BaseState
    {
        public NetworkManager manager;
        public customNetwork.CustomNetworkManager custMgr;
        public CustomNetworkManagerHUD hud;
        
        //  OpenMatch stuff
        bool bOpenMatchWaiting = false;
        private OpenMatchClient openMatch;
        string omAddress;
        int omPort=-1;

        int curNumPlayers;  //  this should really be the number of connections. Assumption of 1 client = 1 player is incorrect with the Add Player button. However, don't let the player do that and we'll be fine. Otherwise, bugs galore. GALORE!

        //  server starts the OpenMatchGame and tells the clients about it to enter this state.
        void StartOpenMatchGame()
        {
            int connId = 0;
            string curStateName = this.GetType().ToString();
            if (custMgr != null)
            {
                Debug.LogWarning("StartOpenMatchGame: custMgr.client_connections.Count=" + custMgr.client_connections.Count.ToString());
                for (int ii = 0; ii < custMgr.client_connections.Count; ii++)
                {
                    try
                    {
                        Debug.LogWarning(ii.ToString() + ") SendServerState(" + curStateName + ") to: " + custMgr.client_connections[ii].playerControllers[0].gameObject.name);
                        connId = custMgr.client_connections[ii].connectionId;
                        custMgr.Cmd_SendServerState(connId, curStateName);
                    }
                    catch
                    {
                        Debug.LogFormat("ServerOpenMatchStart::StartOpenMatchGame() - Exception accessing custMgr.client_connections index {0}", ii);
                    }
                }
            }
        }

        void DisconnectPreviousConnection()
        {
            if (NetworkClient.active)
            {
                NetworkClient.ShutdownAll();
            }
        }
        void OpenMatchRequest()
        {
            Debug.Log("Attempting to connect to Open Match!");

            DisconnectPreviousConnection();

            // This string is what a match is filtered on. Don't change it unless
            // there is a server-side filter which can create a match with a new value.
            string modeCheckJSON = @"{""mode"": {""battleroyale"": 1}";

            if (openMatch.Connect("35.236.24.200", modeCheckJSON))
            {
                Debug.Log("Match request sent!");
                bOpenMatchWaiting = true;
            }
        }

        void GetPointers()
        {
            if (manager == null)
                manager = MultiplayerGame.instance.manager;
            if (manager == null)    //  still null?
            {
                GameObject nm = GameObject.FindGameObjectWithTag("NetworkManager");
                if (nm == null)
                {
                }
                else
                {
                    manager = nm.GetComponent<NetworkManager>();
                }
            }
            if (manager != null)
            {
                custMgr = manager as customNetwork.CustomNetworkManager;
                if (custMgr != null && custMgr.client_connections != null)
                {
                    curNumPlayers = manager.numPlayers;
                }
                hud = manager.GetComponent<CustomNetworkManagerHUD>();
                if (openMatch == null)
                {
                    if (hud !=null)
                        openMatch = hud.GetComponent<OpenMatchClient>();    //  this is probably not the best place for the OpenMatch component, but I don't care. It's where it is now. Change it later if it causes problems.
                }
            }
        }
        override public void Initialize()
        {
            Debug.Log("ServerOpenMatchStart.Initialize");
            GetPointers();
            if (custMgr != null & custMgr.bIsClient)
                OpenMatchRequest();
            if (custMgr != null & custMgr.bIsServer)
                StartOpenMatchGame();
            //if (custMgr != null && custMgr.client_connections != null)
            //{
            //    curNumPlayers = custMgr.client_connections.Count;
            //}
        }
        override public void OnGUI()
        {
            //Debug.LogWarning("ServerOpenMatchStart.OnGUI");
            if (hud != null)
            {
                hud.scaledTextBox("ServerOpenMatchStart.curNumPlayers=" + curNumPlayers.ToString());
            }
        }

        // Update is called once per frame
        override public void Update()
        {
            //Debug.LogWarning("ServerOpenMatchStart.Update: " + manager.name);
            if (custMgr == null)
            {
                custMgr = manager as customNetwork.CustomNetworkManager;
                //Debug.LogWarning("ServerOpenMatchStart.Update: custMgr" + custMgr.name + custMgr.client_connections.Count.ToString());
            }
            if (custMgr != null)
            {
                curNumPlayers = manager.numPlayers;
            }

            if (openMatch != null && openMatch.Port != 0)
            {
                if (omAddress != openMatch.Address && omPort != openMatch.Port)
                {
                    Debug.LogWarning("ServerOpenMatchStart.Update openMatch.Address=" + openMatch.Address + ", port="+openMatch.Port.ToString());

                    manager.StopClient();
                    
                    manager.networkAddress = openMatch.Address;
                    manager.networkPort = openMatch.Port;
                    manager.StartClient();
                    omAddress = openMatch.Address;
                    omPort = openMatch.Port;
                    bOpenMatchWaiting = false;
                }
            }
            else
            {
            }

            if (curNumPlayers >= 4)
            {
                //  fire off the OpenMatchState!
                //  do something here to start OpenMatch
                //MultiplayerGame.instance.ServerSwapMultiPlayerState<Hamster.States.ServerOpenMatchStart>(0, true);
            }
            else if (curNumPlayers <= 0)
            {
                if (custMgr != null & custMgr.bIsServer)    //  only the server can shut down the game. The client cannot do this.
                {
                    // Clear out player states. This is totally a hack, but the OnDisconnect stuff doesn't seem to get called,
                    // and we don't want to leak clients. The server effectively resets at this point.
                    foreach (NetworkConnection conn in custMgr.client_connections)
                    {
                        custMgr.DestroyConnectionsPlayerControllers(conn);
                    }
                    custMgr.client_connections.Clear();

                    //  No players. End of OpenMatch
                    MultiplayerGame.instance.ServerSwapMultiPlayerState<Hamster.States.ServerEndPreGameplay>();
                }
            }
        }
    }
}   //  Hamster.States