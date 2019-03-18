using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
namespace Hamster.States
{
    public class ClientConnected : BaseState
    {
        public NetworkManager manager;
        public NetworkConnection conn;
        public BaseState prevState;
        const float timeBetweenAsks = 5.0f;
        float lastAskTime;

        override public void Initialize()
        {
            if (manager == null)
            {

                manager = UnityEngine.GameObject.FindObjectOfType<NetworkManager>();
            }
            //  the previous state needs to get some callbacks to know that we've connected.
            if (prevState != null)
            {
                ClientOpenMatchFound omFoundState = prevState as ClientOpenMatchFound;
                if (omFoundState != null)
                {
                    omFoundState.OnConnectedToOpenMatch();
                }
            }

        }

        // Start is called before the first frame update
        void Start()
        {

        }

        void AskForLevel()
        {
            if (Time.realtimeSinceStartup > lastAskTime + timeBetweenAsks)
            {
                customNetwork.CustomNetworkManager custMgr = manager as customNetwork.CustomNetworkManager;
                custMgr.ClientAskServerForLevel();
                lastAskTime = Time.realtimeSinceStartup;
            }
        }
        // Update is called once per frame
        void Update()
        {
            // sometimes, we can be here expecting to go to ClientLoadingLevel, but we missed the hmsg_serverLevel so OnClientLevelMsg was never called!
            AskForLevel();
        }
    }
}   //  Hamster.States