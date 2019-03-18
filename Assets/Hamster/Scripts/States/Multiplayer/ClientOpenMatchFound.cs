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
        bool bOpenMatchWaiting = true;
        bool bHaveOpenMatchTicket = false;  // I have a golden ticket! I'm ready to go to OpenMatch! I don't need to be connected to the Lobby anymore!
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
            if ((NetworkClient.allClients.Count > 0) && (null!=isConnectedToOpenMatchServer()))
            {
                ShutdownNonOpenMatchConnections();
            }
        }

        public void OnConnectedToOpenMatch()
        {
            string msg = "!Success! Connect OM : om.Addr=" + openMatch.Address + ", port=" + openMatch.Port.ToString();
            bOpenMatchWaiting = false;
            if (hud != null)
                hud.showClientDebugInfoMessage(msg);
            DisconnectPreviousConnection(); //   if we put it here, we are assuming we can have both OpenMatch and Lobby servers connected at the same time!
        }
        NetworkConnection isConnectedToOpenMatchServer()
        {
            NetworkConnection conn = null;
            foreach (NetworkClient client in NetworkClient.allClients)
            {

                if ((null != client.connection) && (client.connection.address == openMatch.Address && client.connection.isConnected))
                {
                    return client.connection;
                }
            }
            return conn;
        }
        void attemptConnectToOpenMatch()
        {
            if (bOpenMatchWaiting)
            {
                Debug.LogWarning("ClientOpenMatchStart.Update openMatch.Address=" + openMatch.Address + ", port=" + openMatch.Port.ToString());

                manager.networkAddress = openMatch.Address;
                manager.networkPort = openMatch.Port;
                manager.StartClient();
                omAddress = openMatch.Address;
                omPort = openMatch.Port;

                if (null != isConnectedToOpenMatchServer())
                    bOpenMatchWaiting = false;
            }
        }

        //  try to connect to OM server.
        void ConnectToOpenMatchServer()
        {
            //  yeah, weird Unity logic bug: You can have client.active==true, but have allClient.Count==0 at the same time. So, we check on allClients.Count now.
            string serverMsg = "";
            if (NetworkClient.allClients.Count>0)
            {
                serverMsg = NetworkClient.allClients[0].connection.address;
            }
            string msg = "Cnx client.Active=" + NetworkClient.active.ToString() + ", bOMWait=" + bOpenMatchWaiting.ToString() + "\nnClients=" + NetworkClient.allClients.Count.ToString()
                + ", ip=" +serverMsg;
            if ((NetworkClient.active==false) && bOpenMatchWaiting)  //  we can only try this when we're not connected to anything and when we're waiting for openmatch connection.
            {
                NetworkConnection isConn = isConnectedToOpenMatchServer();
                //Debug.LogWarning(msg);
                if (hud==null)
                {
                    GetPointers();
                }
                if (null!=isConnectedToOpenMatchServer())    //  keep trying to connect as long as we're not connected to OM yet.
                {
                    msg = "?TRY? Connect OM : om.Addr=" + openMatch.Address + ", port=" + openMatch.Port.ToString();
                    Debug.LogWarning(msg);
                    if (bHaveOpenMatchTicket)
                    {
                        attemptConnectToOpenMatch();
                    }
                }
                if (isConn != null)
                {
                    msg = "Found Conn=" + isConn.ToString();
                    Debug.LogWarning(msg);
                }
                if (hud != null)
                    hud.showClientDebugInfoMessage(msg);
            }

            //  if this is true, then our job is done and we really should have moved on to a different state.
            NetworkConnection conn = isConnectedToOpenMatchServer();
            if (conn != null)   //  we have connection to OM!
            {
                msg = "DETECTED Connect OM : om.Addr=" + openMatch.Address + ", port=" + openMatch.Port.ToString() + "\n" + conn.ToString();
                bOpenMatchWaiting = false;
                multiplayergame.OnClientConnect(conn);  //  maybe we need to wait for CustomNetworkMangaer to do this from its own OnClientConnect()
                this.bHaveOpenMatchTicket = false;  //  take my ticket away!
            }
            else//  if we're not connected to OpenMatch, then it's a little more complicated. However, if we're in this state, we expect that we're still connected to the LOBBY server. So, we should disconnect from that LOBBY server so that we can go about our business of actually connecting to OM.
            {
                if (NetworkClient.allClients.Count > 0)
                {
                    //  kill our connection with the lobby server so that we can try to connect to OpenMatch server. Unity doesn't seem to support multiple connections at once in Unet, unsurprisingly.
                    NetworkClient.allClients[0].connection.Disconnect();
                    NetworkClient.allClients[0].connection.Dispose();
                }
                attemptConnectToOpenMatch();
            }
            {

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
            GetPointers();
            //Debug.LogWarning("ServerOpenMatchStart.OnGUI");
            if (hud != null)
            {
                //hud.scaledTextBox("ClientOpenMatchFound.curNumPlayers=" + curNumPlayers.ToString());
                if (openMatch != null)
                {
                    NetworkConnection isConn = isConnectedToOpenMatchServer();
                    if (isConn == null)
                    {
                        NetworkClient myClient;
                        string myAddress = "none";
                        if (NetworkClient.allClients.Count > 0)
                        {
                            myClient = NetworkClient.allClients[0];
                            myAddress = myClient.serverIp;
                        }
                        hud.scaledTextBox("OMFound, but not connected ip=" + openMatch.Address + ", port=" + openMatch.Port.ToString() + "\nnClients=" + NetworkClient.allClients.Count.ToString()
                            + ") conn=" + myAddress + ", client.active=" + NetworkClient.active.ToString());
                        //  let's try this to see if it changes anything.
                        if (NetworkClient.allClients.Count == 0)
                            ConnectToOpenMatchServer();
                    }
                    else
                    {
                        hud.scaledTextBox("OMFound ip=" + openMatch.Address + ", port=" + openMatch.Port.ToString() + "\nconn=" + isConn.ToString());

                    }
                }
                if (null!=isConnectedToOpenMatchServer())
                {
                    hud.scaledTextBox("isConnected=true ip=" + openMatch.Address + ":" + openMatch.Port.ToString());
                }
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