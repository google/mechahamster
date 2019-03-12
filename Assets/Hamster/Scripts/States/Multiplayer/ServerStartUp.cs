using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
namespace Hamster.States
{
    public class ServerStartup : BaseState
    {
        public NetworkManager manager;
        public bool bServerStarted = false;
        public int levelIdx = 0;
        bool isHost;
        override public void Initialize()
        {
            isHost = NetworkClient.active && NetworkServer.active;
            if (isHost)
            {
                levelIdx = -1;
            }
            Debug.Log("ServerStartup.Initialize\n");
            if (!bServerStarted)
            {
            }
            Debug.Log("ServerStartup.Initialize=" + bServerStarted.ToString() + "\n");
            AttemptStartServer();
            if (bServerStarted)
            {

            }
        }

        void AttemptStartServer()
        {
            Debug.Log("ServerStartup.AttemptStartServer\n");
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

            }

            // If we have an AgonesComponent we can communicate via Agones. Don't assume this
            // will always exist.
            if (MultiplayerGame.instance.agones != null) {
                MultiplayerGame.instance.agones.BeginHealthCheck();
            }

            if (manager != null)
            {
                //  NOPE! This just creates an infinite loopMultiplayerGame.instance.EnterServerStartupState(levelIdx);  //  use this now instead of manager.StartServer()
                //  somehow you can get this error: Cannot open socket on ip {*} and port {7777}; check please your network, most probably port has been already occupied
                bServerStarted = manager.StartServer(); //  this should now be the only place in the code which calls this. Do not litter this throughout the code anymore.
                if (!bServerStarted)
                {
                    Debug.LogError("StartServer failed so some reason!");
                }
                //bServerStarted = ForceLoadLevel(levelIdx);    //  we can't do this until after NetworkMainGameScene automatically loads!
            }
        }
        // Start is called before the first frame update
        void Start()
        {
            Debug.LogWarning("ServerStartup.Start");
            //ForceLoadLevel();
        }

        // Update is called once per frame
        void Update()
        {
            Debug.LogWarning("ServerStartup.Update");
            AttemptStartServer();
        }
    }
}   //  Hamster.States