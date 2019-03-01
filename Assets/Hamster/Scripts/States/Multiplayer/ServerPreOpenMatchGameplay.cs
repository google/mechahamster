using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
namespace Hamster.States
{
    //  this is where the players can simply join at any time and run around the map doing whatever they want until 4 players are reached and OpenMatch is triggered
    public class ServerPreOpenMatchGamePlay : BaseState
    {
        public NetworkManager manager;
        int curNumPlayers;

        override public void Initialize()
        {
            Debug.LogWarning("ServerPreOpenMatchGamePlay.Initialize");
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
                        Debug.LogError("Cannot start server without networkmanager");
                    }
                    else
                    {
                        manager = nm.GetComponent<NetworkManager>();
                    }
                }
                if (manager != null)
                {
                    curNumPlayers = manager.numPlayers;
                }

            }
        }

        // Update is called once per frame
        void Update()
        {
            Debug.LogWarning("ServerPreOpenMatchGamePlay.Update");

        }
    }
}   //  Hamster.States