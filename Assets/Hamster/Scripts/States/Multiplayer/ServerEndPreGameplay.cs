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
                MultiplayerGame.instance.ServerPopState();
            }
        }
    }
}   //  Hamster.States