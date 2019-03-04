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
        public CustomNetworkManagerHUD hud;
        int curNumPlayers;
        bool hasPlayers = false;

        override public void Initialize()
        {
            Debug.Log("ServerOpenMatchStart.Initialize");
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

            }
        }
        override public void OnGUI()
        {
            //Debug.Log("ServerEndPreGameplay.OnGUI");
            if (hud != null)
            {
                hud.scaledTextBox("ServerEndPreGameplay curNumPlayers=" + curNumPlayers.ToString());
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
            }
            else
            {
                // %%% Temp for testing -- Remove me once OM states are all working.
                if (hasPlayers)
                {
                    // Something might need to kick the clients gracefully here. This will just tell Agones to terminate
                    // the server because the match is up.
                    if (MultiplayerGame.instance.agones != null)
                    {
                        MultiplayerGame.instance.agones.Shutdown();
                    }
                }
            }
        }
    }
}   //  Hamster.States