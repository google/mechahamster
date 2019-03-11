using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
namespace Hamster.States
{
    public class ServerListenForClients : BaseState
    {
        public NetworkManager manager;
        int     curNumPlayers;
        public customNetwork.CustomNetworkManager custMgr;
        static float ReadyDelayTimeout = 30.0f; // seconds

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
            }
        }

        override public void Initialize()
        {
            GetPointers();

            if (custMgr)
            {
                Debug.Log("ServerListenForClients: Starting ready update routine");
                custMgr.StartReadyRoutine(ReadyDelayTimeout);
            }
        }

        // Update is called once per frame
        public override void Update()
        {
            curNumPlayers = manager.numPlayers;

            // The idea here is that a level load initially puts us into this state if we have Agones running.
            // In that case, any players who join transition us into ServerPreOpenMatchGamePlay which starts
            // all the real logic for managing the game. Along the way we occasionally signal that the server
            // is ready to Agones so that if we allocate a server it will return to the Ready state if nobody
            // ever joined it. This fixes the possibility of leaked servers due to menu problems as well as
            // allows dummy clients to allocate a server and have it eventually return to the pool.

            if (curNumPlayers > 0)
            {
                if (custMgr)
                {
                    Debug.Log("ServerListenForClients: Stopping ready update routine");
                    custMgr.StopReadyRoutine();
                }

                MultiplayerGame.instance.ServerSwapMultiPlayerState<Hamster.States.ServerPreOpenMatchGamePlay>();
            }
        }
    }
}   //  Hamster.States