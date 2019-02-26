using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
namespace Hamster.States
{
    public class ClientConnected : BaseState
    {
        public NetworkManager manager;

        override public void Initialize()
        {
            if (manager == null)
            {

                manager = UnityEngine.GameObject.FindObjectOfType<NetworkManager>();
				//if (manager!=null)
				//	bServerStarted = manager.StartServer();
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