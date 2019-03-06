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
        bool hasPlayers = false;
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
                    // Something might need to kick the clients gracefully here. This will just tell Agones to terminate
                    // the server because the match is up.
                    if (MultiplayerGame.instance.agones != null)
                    {
                        MultiplayerGame.instance.ServerGameOver();
                        ShutdownAgones();
                    }
                }
            }
        }

        void ShutdownAgones()
        {
            // Something might need to kick the clients gracefully here. This will just tell Agones to terminate
            // the server because the match is up.
            if (MultiplayerGame.instance.agones != null)
            {
                agoneHasShutdown = true;    //  don't know if we can spam agones.Shutdown or not. Just allow it unless it causes problems.
                MultiplayerGame.instance.agones.Shutdown();
            }
            else
            {  //  we've added a player, so we're no longer in this state.
            }
        }
        override public void OnGUI()
        {
            //Debug.Log("ServerEndPreGameplay.OnGUI");
            if (hud != null)
            {
                hud.scaledTextBox("ServerEndPreGameplay curNumPlayers=" + curNumPlayers.ToString() + ",nClients=" + curNumClients.ToString());
            }
        }

        // Update is called once per frame
        override public void Update()
        {
            //Debug.Log("ServerEndPreGameplay.Update");
            curNumPlayers = manager.numPlayers;
            if (curNumPlayers > 0)
            {
                // GM: Temporarily changing this so we can exit an OpenMatch match and shutdown a server.
                //MultiplayerGame.instance.ServerSwapMultiPlayerState<Hamster.States.ServerEndPreGameplay>(); //  start the game
                hasPlayers = true;
                MultiplayerGame.instance.ServerSwapMultiPlayerState<Hamster.States.ServerPreOpenMatchGamePlay>();
            }
            else
            {
                ShutdownAgones();
            }
        }
    }
}   //  Hamster.States