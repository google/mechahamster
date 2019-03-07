using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
namespace Hamster.States
{
    //  This client is playing the game now.
    public class ClientInGame : BaseState
    {
        public NetworkManager manager;
        bool isClientSceneAddPlayerCalled = false; //  must have called ClientScene.AddPlayer in one way or another

        override public void Initialize()
        {
            if (manager == null)
            {

                manager = UnityEngine.GameObject.FindObjectOfType<NetworkManager>();
            }

        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        public override void Update()
        {
            //Debug.Log("ClientInGame.Update() nplayers=" + manager.numPlayers.ToString());
            //Debug.Log("ClientInGame.Update() NetworkClient.allClients.Count=" + NetworkClient.allClients.Count.ToString());
            //Debug.Log("ClientInGame.Update() NetworkClient.allClients[0].isConnected=" + NetworkClient.allClients[0].isConnected.ToString());
            //  note: on the client, we can't actually keep track of how many players are in the game!
            if (manager.numPlayers <= 0 && (NetworkClient.allClients.Count > 0) && !NetworkClient.allClients[0].isConnected)
            {
                if (NetworkClient.active)   //  I'm the only client and I'm not in the game anymore, so I need to tell my server who I'm still connected to.
                {
                    MultiplayerGame.instance.ClientEnterMultiPlayerState<Hamster.States.ServerEndPreGameplay>();
                    NetworkClient.ShutdownAll();
                }

            }
        }
    }
}   //  Hamster.States