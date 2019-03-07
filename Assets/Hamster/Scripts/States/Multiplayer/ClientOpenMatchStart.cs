using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
namespace Hamster.States
{
    //  this is where the players from the "preOpenMatch" game are matched in OpenMatch and disconnected from their current game. This starts a new 4 player game with OpenMatch.
    public class ClientOpenMatchStart : BaseState
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

        void DisconnectPreviousConnection()
        {
            if (NetworkClient.active)
            {
                NetworkClient.ShutdownAll();
            }
        }
        void OpenMatchRequest()
        {
            Debug.LogWarning("Attempting to connect to Open Match!");

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
            Debug.Log("ClientOpenMatchStart.Initialize");
            GetPointers();
            OpenMatchRequest();
        }
        override public void OnGUI()
        {
            //Debug.LogWarning("ClientOpenMatchStart.OnGUI");
            if (hud != null)
            {
                hud.scaledTextBox("ClientOpenMatchStart.curNumPlayers=" + curNumPlayers.ToString());
                hud.scaledTextBox("ClientOpenMatchStart ip=" + manager.networkAddress + ", port=" + manager.networkPort.ToString());
                if (openMatch != null)
                    hud.scaledTextBox("openMatch ip=" + openMatch.Address + ", port=" + openMatch.Port.ToString());
            }
        }

        // Update is called once per frame
        override public void Update()
        {
            //Debug.LogWarning("ClientOpenMatchStart.Update: " + manager.name);
            if (custMgr == null)
            {
                custMgr = manager as customNetwork.CustomNetworkManager;
                //Debug.LogWarning("ClientOpenMatchStart.Update: custMgr" + custMgr.name + custMgr.client_connections.Count.ToString());
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
                if (custMgr.bIsClient)
                {
                    MultiplayerGame.instance.ClientSwapMultiPlayerState<Hamster.States.ClientOpenMatchFound>();
                }

                //if (omAddress != openMatch.Address && omPort != openMatch.Port)
                //{
                //    Debug.LogWarning("ClientOpenMatchStart.Update openMatch.Address=" + openMatch.Address + ", port="+openMatch.Port.ToString());

                //    manager.networkAddress = openMatch.Address;
                //    manager.networkPort = openMatch.Port;
                //    manager.StartClient();
                //    omAddress = openMatch.Address;
                //    omPort = openMatch.Port;
                //    bOpenMatchWaiting = false;
                //}
            }
            else
            {
            }

            if (curNumPlayers >= 4)
            {
                //  fire off the OpenMatchState!
                //  do something here to start OpenMatch
                //MultiplayerGame.instance.ServerSwapMultiPlayerState<Hamster.States.ClientOpenMatchStart>(0, true);
            }
            else if (curNumPlayers <= 0)
            {
                if (custMgr != null & custMgr.bIsServer)    //  only the server can shut down the game. The client cannot do this.
                {
                    //  No players. End of OpenMatch
                    MultiplayerGame.instance.ServerSwapMultiPlayerState<Hamster.States.ServerEndPreGameplay>();

                }
            }
        }
    }
}   //  Hamster.States