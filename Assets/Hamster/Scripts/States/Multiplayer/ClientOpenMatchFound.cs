using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
namespace Hamster.States
{
    //  after the client requests an OpenMatch and its IP is received, we call this and begin.
    public class ClientOpenMatchFound : BaseState
    {
        public NetworkManager manager;
        public customNetwork.CustomNetworkManager custMgr;
        public CustomNetworkManagerHUD hud;
        public MultiplayerGame multiplayergame;
        //  OpenMatch stuff
        bool bOpenMatchWaiting = false;
        private OpenMatchClient openMatch;
        string omAddress;
        int omPort=-1;

        int curNumPlayers;  //  this should really be the number of connections. Assumption of 1 client = 1 player is incorrect with the Add Player button. However, don't let the player do that and we'll be fine. Otherwise, bugs galore. GALORE!

        //  server starts the OpenMatchGame and tells the clients about it to enter this state.

    void ShutdownNonOpenMatchConnections()
        {
            foreach (NetworkClient client in NetworkClient.allClients)
            {
                if ((client.connection.address != openMatch.Address) && client.connection.isConnected)
                {
                    client.Disconnect();
                }
            }
        }
        void DisconnectPreviousConnection()
        {
            if (NetworkClient.active && (null!=isConnectedToOpenMatchServer()))
            {
                ShutdownNonOpenMatchConnections();
            }
        }

        public void OnConnectedToOpenMatch()
        {
            string msg = "!Success! Connect OM : om.Addr=" + openMatch.Address + ", port=" + openMatch.Port.ToString();
            bOpenMatchWaiting = false;
            DisconnectPreviousConnection(); //   if we put it here, we are assuming we can have both OpenMatch and Lobby servers connected at the same time!
        }
        NetworkConnection isConnectedToOpenMatchServer()
        {
            NetworkConnection conn = null;
            foreach (NetworkClient client in NetworkClient.allClients)
            {
                if (client.connection.address == openMatch.Address && client.connection.isConnected)
                {
                    return client.connection;
                }
            }
            return conn;
        }

        //  try to connect to OM server.
        void ConnectToOpenMatchServer()
        {
            string msg = "Cnx client.Active=" + NetworkClient.active.ToString() + ",bOMWait=" + bOpenMatchWaiting.ToString();
            if (!NetworkClient.active && bOpenMatchWaiting)  //  we can only try this when we're not connected to anything and when we're waiting for openmatch connection.
            {
                //Debug.LogWarning(msg);
                if (hud==null)
                {
                    GetPointers();
                }
                if (null!=isConnectedToOpenMatchServer())    //  keep trying to connect as long as we're not connected to OM yet.
                {
                    manager.networkAddress = openMatch.Address;
                    manager.networkPort = openMatch.Port;
                    msg = "?TRY? Connect OM : om.Addr=" + openMatch.Address + ", port=" + openMatch.Port.ToString();
                    manager.StartClient();
                }
                omAddress = openMatch.Address;
                omPort = openMatch.Port;
            }

            //  if this is true, then our job is done and we really should have moved on to a different state.
            NetworkConnection conn = isConnectedToOpenMatchServer();
            if (conn != null)
            {
                msg = "DETECTED Connect OM : om.Addr=" + openMatch.Address + ", port=" + openMatch.Port.ToString() + "\n" + conn.ToString();
                bOpenMatchWaiting = false;
                multiplayergame.OnClientConnect(conn);
            }
            if (hud != null)
            {
                hud.showClientDebugInfoMessage(msg);
            }
        }

        //void OpenMatchRequest()
        //{

        //    Debug.Log("Attempting to connect to Open Match!");

        //    DisconnectPreviousConnection();

        //    // This string is what a match is filtered on. Don't change it unless
        //    // there is a server-side filter which can create a match with a new value.
        //    string modeCheckJSON = @"{""mode"": {""battleroyale"": 1}";

        //    if (openMatch.Connect("35.236.24.200", modeCheckJSON))
        //    {
        //        Debug.Log("Match request sent!");
        //        bOpenMatchWaiting = true;
        //    }
        //}

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
                    curNumPlayers = custMgr.getNumPlayers();
                }
                hud = manager.GetComponent<CustomNetworkManagerHUD>();
                if (openMatch == null)
                {
                    if (hud !=null)
                        openMatch = hud.GetComponent<OpenMatchClient>();    //  this is probably not the best place for the OpenMatch component, but I don't care. It's where it is now. Change it later if it causes problems.
                }
                multiplayergame = manager.GetComponent<MultiplayerGame>();
            }
        }
        override public void Initialize()
        {
            GetPointers();
            bOpenMatchWaiting = true;
            //  ConnectToOpenMatchServer(); //  stall. put in a wait state between disconnect from previous server and connection to openmatch

        }
        override public void OnGUI()
        {
            //Debug.LogWarning("ServerOpenMatchStart.OnGUI");
            if (hud != null)
            {
                //hud.scaledTextBox("ClientOpenMatchFound.curNumPlayers=" + curNumPlayers.ToString());
                if (openMatch != null)
                    hud.scaledTextBox("openMatch ip=" + openMatch.Address + ", port=" + openMatch.Port.ToString());
            }
        }

        // Update is called once per frame
        override public void Update()
        {
            GetPointers();
            //Debug.LogWarning("ServerOpenMatchStart.Update: " + manager.name);
            if (custMgr == null)
            {
                custMgr = manager as customNetwork.CustomNetworkManager;
                //Debug.LogWarning("ServerOpenMatchStart.Update: custMgr" + custMgr.name + custMgr.client_connections.Count.ToString());
            }
            if (custMgr != null)
            {
                if (custMgr.client_connections != null)
                {
                    curNumPlayers = custMgr.getNumPlayers();
                }
            }

            if (openMatch != null && openMatch.Port != 0)
            {

                if (bOpenMatchWaiting)  //  if we've been asked to connect to OM, keep trying until we succeed.
                {
                    ConnectToOpenMatchServer();
                }
            }
        }
    }
}   //  Hamster.States