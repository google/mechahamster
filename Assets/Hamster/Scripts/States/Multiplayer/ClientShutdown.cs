using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
namespace Hamster.States
{
    //  This client is playing the game now.
    public class ClientShutdown : BaseState
    {
        public NetworkManager manager;
        public CustomNetworkManagerHUD hud;
        private string myDebugMsg;
        bool isClientSceneAddPlayerCalled = false; //  must have called ClientScene.AddPlayer in one way or another

        void GetPointers()
        {
            if (manager == null || hud == null)
            {

                manager = UnityEngine.GameObject.FindObjectOfType<NetworkManager>();
                if (manager != null)
                {
                    hud = manager.GetComponent<CustomNetworkManagerHUD>();
                }
                else
                {
                    Debug.LogError("ClientInGame.GetPointers could not find NetworkManager!\n");
                }
            }

        }
        override public void Initialize()
        {
            GetPointers();
            Shutdown();
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        void Shutdown()
        {
            manager.StopClient();
            Debug.LogWarning("ClientShutdown.Shutdown\n");
            NetworkClient.ShutdownAll();    //  
        }
        public override void OnGUI()
        {
            hud.scaledTextBox(myDebugMsg);
        }
        // Update is called once per frame
        public override void Update()
        {
            if (hud != null)
            {
                myDebugMsg = "ClientShutdown: nPlr=" + manager.numPlayers.ToString() + " nClients=" + NetworkClient.allClients.Count.ToString() + "\n\tNetClient.active=" + NetworkClient.active.ToString();
            }
            else
            {
                GetPointers();
            }
        }
    }
}   //  Hamster.States