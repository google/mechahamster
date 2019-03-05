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

        int curNumPlayers;

        void StartOpenMatchGame()
        {
            int connId = 0;
            string curStateName = this.GetType().ToString();
            for (int ii = 0; ii < custMgr.client_connections.Count; ii++)
            {
                if (custMgr != null)
                {
                    connId = custMgr.client_connections[ii].connectionId;
                    custMgr.Cmd_SendServerState(connId, curStateName);
                }
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
                    curNumPlayers = custMgr.client_connections.Count;
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
            if (custMgr != null && custMgr.client_connections != null)
            {
                curNumPlayers = custMgr.client_connections.Count;
            }
        }
        override public void OnGUI()
        {
            Debug.LogWarning("ServerOpenMatchStart.OnGUI");
            if (hud != null)
            {
                hud.scaledTextBox("curNumPlayers=" + curNumPlayers.ToString());
            }
        }

        // Update is called once per frame
        override public void Update()
        {
            Debug.LogWarning("ServerOpenMatchStart.Update: " + manager.name);
            if (custMgr == null)
            {
                custMgr = manager as customNetwork.CustomNetworkManager;
                Debug.LogWarning("ServerOpenMatchStart.Update: custMgr" + custMgr.name + custMgr.client_connections.Count.ToString());
            }
            if (custMgr != null)
            {
                if (custMgr.client_connections != null)
                {
                    curNumPlayers = custMgr.client_connections.Count;
                }
            }

            if (openMatch != null && openMatch.Port != 0)
            {
                if (omAddress != openMatch.Address && omPort != openMatch.Port)
                {
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
                //  No players. End of OpenMatch
                //  do something here to close open match so we can go back to the state where we 
                MultiplayerGame.instance.ServerSwapMultiPlayerState<Hamster.States.ServerEndPreGameplay>();

                // Something might need to kick the clients gracefully here. This will just tell Agones to terminate
                // the server because the match is up.
                if (MultiplayerGame.instance.agones != null)
                {
                    MultiplayerGame.instance.ServerGameOver();
                    MultiplayerGame.instance.agones.Shutdown();
                }
            }
        }
    }
}   //  Hamster.States