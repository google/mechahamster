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
        bool[] omClientACKRecvd;    //  mapped 1-for-1  by idx to custMgr.client_connections
        float lastTimeAckReqSent; //  after we send a request client to start OpenMatch and to send back an ACK.
        float timeBetweenAckReq = 2.0f;    //  how much time before we send another ack request to the client to start OpenMatch.
        int curNumPlayers;  //  this should really be the number of connections. Assumption of 1 client = 1 player is incorrect with the Add Player button. However, don't let the player do that and we'll be fine. Otherwise, bugs galore. GALORE!
        int nretries;

        //  server starts the OpenMatchGame and tells the clients about it to enter this state.
        //  -1 sends to ALL clients to start OpenMatch.
        //  > 0 time is to send to only clients which have not sent back an ACK that they have started OpenMatch!
        bool StartOpenMatchGame(float lastAbsTime= -1.0f)
        {
            bool bAllAcksRecvd = false;

            int connId = 0;
            string curStateName = this.GetType().ToString();
            if (custMgr != null)
            {
                Debug.LogWarning("StartOpenMatchGame: custMgr.client_connections.Count=" + custMgr.client_connections.Count.ToString() + "t=" + lastAbsTime.ToString());
                if (custMgr.client_connections.Count > 0)
                {
                    if (lastAbsTime < 0)
                    {
                        omClientACKRecvd = new bool[custMgr.client_connections.Count];
                        for (int ii = 0; ii < custMgr.client_connections.Count; ii++)
                        {
                            omClientACKRecvd[ii] = false;
                        }
                    }
                    bAllAcksRecvd = true;
                    for (int ii = 0; ii < custMgr.client_connections.Count; ii++)
                    {
                        connId = custMgr.client_connections[ii].connectionId;
                        omClientACKRecvd[ii] = this.custMgr.ackReceived[connId];
                        if (!omClientACKRecvd[ii])  //  if this client hasn't acknowledged that they have started OpenMatch, then tell them again.
                        {
                            bAllAcksRecvd = false;
                            Debug.LogWarning(ii.ToString() + ") SendServerState(" + curStateName + ") to: " + custMgr.client_connections[ii].playerControllers[0].gameObject.name + "\nretries="+nretries.ToString());
                            custMgr.setAck(connId, false); //  clear the ack bit.
                            custMgr.Cmd_SendServerState(connId, curStateName);
                            nretries++;
                        }
                    }
                    lastTimeAckReqSent = Time.realtimeSinceStartup;
                }
            }
            return bAllAcksRecvd;
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
                    curNumPlayers = custMgr.getNumPlayers();
                }
                hud = manager.GetComponent<CustomNetworkManagerHUD>();
                if (openMatch == null)
                {
                    if (hud !=null)
                        openMatch = hud.GetComponent<OpenMatchClient>();    //  this is probably not the best place for the OpenMatch component, but I don't care. It's where it is now. Change it later if it causes problems.
                }
            }
        }

        //  our message to tell the clients to find an OpenMatch game may have failed. so we must periodically tell them again so they can acknowledge with us.
        void WaitForClientACKs()
        {
            if (Time.realtimeSinceStartup >= lastTimeAckReqSent + timeBetweenAckReq)
            {
                bool allACKsRecvd = StartOpenMatchGame(lastTimeAckReqSent);
                if (allACKsRecvd)
                {
                    //  okay, all of the clients know to move to OpenMatch, so we can shut down this server!
                    MultiplayerGame.instance.ServerSwapMultiPlayerState<Hamster.States.ServerEndPreGameplay>();
                }
            }
        }
        override public void Initialize()
        {
            Debug.Log("ServerOpenMatchStart.Initialize");
            GetPointers();
            nretries = 0;
            if (custMgr != null & custMgr.bIsServer)
                StartOpenMatchGame(-1.0f);
        }
        override public void OnGUI()
        {
            //Debug.LogWarning("ServerOpenMatchStart.OnGUI");
            if (hud != null)
            {
                //hud.scaledTextBox("ServerOpenMatchStart.curNumPlayers=" + curNumPlayers.ToString());
                hud.scaledTextBox("ServerOpenMatchStart ip=" + manager.networkAddress + ", port=" + manager.networkPort.ToString());
                if (openMatch != null)
                    hud.scaledTextBox("openMatch ip=" + openMatch.Address + ", port=" + openMatch.Port.ToString());
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
                if (custMgr.client_connections != null)
                {
                    curNumPlayers = custMgr.getNumPlayers();
                }
            }

            if (openMatch != null && openMatch.Port != 0)
            {
                if (custMgr.bIsClient)
                {
                    MultiplayerGame.instance.ServerSwapMultiPlayerState<Hamster.States.ClientOpenMatchFound>();
                }

                //if (omAddress != openMatch.Address && omPort != openMatch.Port)
                //{
                //    Debug.LogWarning("ServerOpenMatchStart.Update openMatch.Address=" + openMatch.Address + ", port="+openMatch.Port.ToString());

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

            if (curNumPlayers > 0)
            {
                WaitForClientACKs();
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