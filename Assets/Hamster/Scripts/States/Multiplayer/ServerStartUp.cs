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

        override public void Initialize()
        {
            if (manager == null)
            {

                manager = UnityEngine.GameObject.FindObjectOfType<NetworkManager>();//GetComponent<NetworkManager>();
                if (manager != null)
                    bServerStarted = manager.StartServer();
            }
            Debug.Log("ServerStartup.Initialize=" + bServerStarted.ToString());

            if (bServerStarted)
            {

            }
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}   //  Hamster.States