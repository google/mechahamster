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
            string msg = "ServerListenForClients: Starting ready update routine";
            GetPointers();

            if (custMgr)
            {
                msg = msg + "custMgr=" + custMgr.isActiveAndEnabled.ToString();
                Debug.Log(msg);
                custMgr.StartReadyRoutine(ReadyDelayTimeout);
            }
            else
            {
                msg = msg + "custMgr==null";
                Debug.Log(msg);
            }
            CustomNetworkManagerHUD hud = MultiplayerGame.instance.manager.GetComponent<CustomNetworkManagerHUD>();
            if (hud != null)
                hud.showClientDebugInfoMessage(msg);
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
            BaseState curState = Hamster.CommonData.mainGame.stateManager.CurrentState();
            string curStateName = curState.ToString();

            string msg = "ServerListenForClients.Update: curNumPlayers=" + curNumPlayers.ToString() + "\nstate=" + curStateName;
            GetPointers();
            CustomNetworkManagerHUD hud = MultiplayerGame.instance.manager.GetComponent<CustomNetworkManagerHUD>();
            if (hud != null)
                hud.showClientDebugInfoMessage(msg);


            if (curNumPlayers > 0)
            {
                if (custMgr)
                {
                    msg = msg + "\nServerListenForClients: Stopping ready update routine";
                    Debug.Log(msg);
                    custMgr.StopReadyRoutine();
                }

                MultiplayerGame.instance.ServerSwapMultiPlayerState<Hamster.States.ServerPreOpenMatchGamePlay>();
            }
            if (hud != null)
                hud.showClientDebugInfoMessage(msg);
        }
    }
}   //  Hamster.States