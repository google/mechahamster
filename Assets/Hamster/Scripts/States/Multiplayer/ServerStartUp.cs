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
        override public void Initialize()
        {
            Debug.LogWarning("ServerStartup.Initialize");
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
            if (!bServerStarted)
            {
                if (manager != null)
                {
                    //  NOPE! This just creates an infinite loopMultiplayerGame.instance.EnterServerStartupState(levelIdx);  //  use this now instead of manager.StartServer()
                    bServerStarted = manager.StartServer();

                    //bServerStarted = ForceLoadLevel(levelIdx);    //  we can't do this until after NetworkMainGameScene automatically loads!
                }
            }
            Debug.Log("ServerStartup.Initialize=" + bServerStarted.ToString());

            if (bServerStarted)
            {

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

        }
    }
}   //  Hamster.States