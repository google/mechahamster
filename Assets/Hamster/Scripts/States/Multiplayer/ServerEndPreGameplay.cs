using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
namespace Hamster.States
{
    //  this is where the players can simply join at any time and run around the map doing whatever they want until 4 players are reached and OpenMatch is triggered
    //  now that this has ended, we should do some clean up.
    public class ServerEndPreGameplay : BaseState
    {
        public NetworkManager manager;
        public customNetwork.CustomNetworkManager custMgr;
        public CustomNetworkManagerHUD hud;
        int curNumPlayers;
        int curNumClients;
        bool agoneHasShutdown = false;

        override public void Initialize()
        {
            Debug.Log("ServerEndPreGameplay.Initialize");
            if (manager == null)
            {
                manager = MultiplayerGame.instance.manager;
                //  hmm this seems to fail hard.
                // manager = UnityEngine.GameObject.FindObjectOfType<NetworkManager>();//GetComponent<NetworkManager>();
                if (manager == null)
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
                    curNumPlayers = manager.numPlayers;
                    hud = manager.GetComponent<CustomNetworkManagerHUD>();
                }
                if (custMgr != null && custMgr.bIsServer)
                {
                    disconnectAllClients();
                    curNumPlayers = custMgr.getNumPlayers();
                    // the server because the match is up.
                    if (MultiplayerGame.instance.agones != null)
                    {
                        MultiplayerGame.instance.ServerGameOver();
                        ShutdownAgones();
                    }
                }
            }
        }

        //  all of the clients need to be kicked off this server. The preOpenMatch lobby has ended and the players have found new matches through OpenMatch. So we need to kick all the connections off this server.
        void disconnectAllClients()
        {
            if (!custMgr) return;

            for(int ii=0; ii<custMgr.client_connections.Count; ii++)
            {
                custMgr.client_connections[ii].Disconnect();
            }
        }

        void ShutdownAgones()
        {
            if (MultiplayerGame.instance.agones != null && !agoneHasShutdown)
            {
                agoneHasShutdown = true;    //  don't know if we can spam agones.Shutdown or not. Just allow it unless it causes problems.
                MultiplayerGame.instance.agones.Shutdown();
            }
        }
        override public void OnGUI()
        {
            //Debug.Log("ServerEndPreGameplay.OnGUI");
            if (hud != null)
            {
                //hud.scaledTextBox("ServerEndPreGameplay curNumPlayers=" + curNumPlayers.ToString() + ",nClients=" + curNumClients.ToString() + "\n\tNetClient.active=" + NetworkClient.active.ToString());
            }
        }

        // Update is called once per frame
        override public void Update()
        {
            //Debug.Log("ServerEndPreGameplay.Update");
            curNumPlayers = manager.numPlayers;
            if (curNumPlayers > 0)
            {
                if (MultiplayerGame.instance.agones == null)
                {
                    MultiplayerGame.instance.ServerSwapMultiPlayerState<Hamster.States.ServerPreOpenMatchGamePlay>();
                }
            }
            else
            {
                ShutdownAgones();
            }
        }
    }
}   //  Hamster.States