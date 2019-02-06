using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
//  using UnityEngine.CoreModule.Network;   //  removed in 2018.3.1


/*
 * In NetworkIdentity component on the Player prefab, make sure that LocalAuthority is checked.
 */
namespace customNetwork
{
    public class CustomNetworkPlayer : NetworkBehaviour
    {
        const int kMaxPlayerControllers = 32;   //  this is really from Unity, not me. ClientScene.AddPlayer will throw an error at 33 players.

        public bool bServerAuthoritative;
        NetworkIdentity netIdentity;
        static float lastSpawnTime;

        float spawnTime;
        const float waitForSpawnBeforeAcceptingInputs = 0.65f;   //  if we don't do this, when we spawn new objects, they may cause most objects to spawn via keyboard command!
        public bool bCreateClientOnEnable;
        public bool bTestSpawnFromServer;
        public static NetworkConnection conn;
        public short playerControllerID;
        short curSpawnIdx = 0;
        const float kKeySpeed = 1.0f;
        const float kKeyRotationRate = 5.0f;
        bool bIsPlayerClientConnected;

        void ServerSpawn(GameObject go, GameObject plrGO)
        {
            Debug.LogWarning("NetworkPlayer.ServerSpawn: " + go.name);
            //  Network.Instantiate removed in 2018.3.1
            //Cmd_ServerPlayerSpawnAsPrefabID(0, 0);

            NetworkServer.Spawn(go);
            //NetworkServer.SpawnWithClientAuthority(go, plrGO);    //  try this later when the above is working.
        }


        [Command]
        void Cmd_NetworkSpawn(GameObject go, GameObject plrGO)
        {
            Debug.LogWarning("NetworkPlayer.Cmd_NetworkSpawn");
            ServerSpawn(go, plrGO);
        }


        /* 
         * Once we have a connection from the NetworkManager, we can must make a player client. This is the player's control to the server. Without this, you cannot call cmd_ on the server nor spawn an object.
         * This is what Unity calls the "Player GameObject" which is confusing. It's the player's client control of the server. It holds the key to being able to talk to the server with authority and authentication.
         */
        public static void CreatePlayerClient(short plrControlID)
        {
            if ((conn != null) && conn.isReady)
                Debug.LogWarning("NetworkPlayer.CreatePlayerClient: " + plrControlID.ToString() + "\n");
            if ((conn != null) && conn.isReady)
            {
                if (plrControlID < kMaxPlayerControllers)
                {
                    Debug.LogWarning("  conn.isReady: NetworkPlayer.CreatePlayerClient -> ClientScene.AddPlayer: " + plrControlID.ToString() + "\n");
                    ClientScene.AddPlayer(plrControlID);
                }
                else
                {
                    Debug.LogError("No player added. Too many players>32. NetworkPlayer.CreatePlayerClient -> ClientScene.AddPlayer: " + plrControlID.ToString() + "\n");
                }
            }
        }
        public static void DestroyPlayer(short plrControlID)
        {
            if ((conn != null) && conn.isReady)
            {
                if ((ClientScene.localPlayers.Count>0) && (plrControlID>= 0) && (plrControlID < ClientScene.localPlayers.Count))// check and bail if index out of range.
                {
                    Debug.LogWarning("NetworkPlayer.DestroyPlayer: " + plrControlID.ToString() +"/"+ ClientScene.localPlayers.Count.ToString()+ "\n");
                    if (ClientScene.localPlayers[plrControlID].unetView != null)    //  check to see if we've already been destroyed somehow
                    {
                        //  Note: yeah, Unity doesn't really keep track of this very well, so we need to manage localPlayers ouselves. I guess you could call it a bug. Anyway, we'll just manage the localPlayers[] array ourselves when we call RemovePlayer() which seems like it should do it automatically. Whatevs.
                        ClientScene.RemovePlayer(plrControlID);
                    }
                    //  if we've been destroyed, either by successful RemovePlayer above or before we even got here, let's clean up the localPlayers[] array because Unity doesn't do it.
                    if (ClientScene.localPlayers[plrControlID].unetView == null)
                    {
                        ClientScene.localPlayers.RemoveAt(plrControlID);
                    }
                }
            }
        }
        //void NetworkSpawn()
        //{
        //    Debug.LogWarning("NetworkPlayer.NetworkSpawn");
        //    if ((conn != null) && conn.isReady)
        //    {
        //        //curSpawnIdx = (short)((int)curSpawnIdx % 4);
        //        //GameObject prefabToSpawn = nm.spawnPrefabs[curSpawnIdx++];
        //        GameObject plrGO = this.gameObject;
        //        GameObject prefabToSpawn = plrGO;
        //    }
        //}
        //
        // Summary:
        //     Callback used by the visibility system to determine if an observer (player) can
        //     see this object.
        //
        // Parameters:
        //   conn:
        //     Network connection of a player.
        //
        // Returns:
        //     True if the player can see this object.
        public virtual bool OnCheckObserver(NetworkConnection conn)
        {
            if (conn != null)
                Debug.LogWarning("NetworkPlayer.OnCheckObserver: " + conn.ToString());
            return base.OnCheckObserver(conn);
        }
        //
        // Summary:
        //     Virtual function to override to receive custom serialization data. The corresponding
        //     function to send serialization data is OnSerialize().
        //
        // Parameters:
        //   reader:
        //     Reader to read from the stream.
        //
        //   initialState:
        //     True if being sent initial state.
        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            if (reader != null)
                Debug.LogWarning("NetworkPlayer.OnDeserialize: " + reader.ToString());
            base.OnDeserialize(reader, initialState);
        }
        //
        // Summary:
        //     This is invoked on clients when the server has caused this object to be destroyed.
        public virtual void OnNetworkDestroy()
        {
            Debug.LogWarning("NetworkPlayer.OnNetworkDestroy");
            base.OnNetworkDestroy();
        }

        public virtual bool OnRebuildObservers(HashSet<NetworkConnection> observers, bool initialize)
        {
            return base.OnRebuildObservers(observers, initialize);
        }

        //
        // Summary:
        //     Virtual function to override to send custom serialization data. The corresponding
        //     function to send serialization data is OnDeserialize().
        //
        // Parameters:
        //   writer:
        //     Writer to use to write to the stream.
        //
        //   initialState:
        //     If this is being called to send initial state.
        //
        // Returns:
        //     True if data was written.
        public virtual bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            return base.OnSerialize(writer, initialState);
        }

        //
        // Summary:
        //     Callback used by the visibility system for objects on a host.
        //
        // Parameters:
        //   vis:
        //     New visibility state.
        public virtual void OnSetLocalVisibility(bool vis)
        {
            Debug.LogWarning("NetworkPlayer.OnSetLocalVisibility");
        }

        //
        // Summary:
        //     This is invoked on behaviours that have authority, based on context and NetworkIdentity.localPlayerAuthority.
        public virtual void OnStartAuthority()
        {
            Debug.LogWarning("NetworkPlayer.OnStartAuthority");
        }


        //
        // Summary:
        //     Called on every NetworkBehaviour when it is activated on a client.
        public virtual void OnStartClient()
        {
            Debug.LogWarning("NetworkPlayer.OnStartClient");
        }
        //
        // Summary:
        //     Called when the local player object has been set up with a localPlayerControllerId.
        //  this will set isLocalPlayer on NetworkIdentity=true
        public virtual void OnStartLocalPlayer()
        {
            Debug.LogWarning("NetworkPlayer.OnStartLocalPlayer");
        }

        //
        // Summary:
        //     This is invoked for NetworkBehaviour objects when they become active on the server.
        public override void OnStartServer()
        {
            Debug.LogWarning("NetworkPlayer.OnStartServer");
            base.OnStartServer();
        }

        //
        // Summary:
        //     This is invoked on behaviours when authority is removed.
        public virtual void OnStopAuthority()
        {
            Debug.LogWarning("NetworkPlayer.OnStopAuthority");
        }


        private void OnEnable()
        {
            //  can only do this on the client or else we're gonna have a stack overflow situation and lock unity.
            if (bCreateClientOnEnable && isClient)
                CreatePlayerClient(playerControllerID);
        }
        private void Awake()
        {
            netIdentity = GetComponent<NetworkIdentity>();  //  allow us to conveniently grab this later.
        }

        // Start is called before the first frame update
        void Start()
        {
            spawnTime = Time.time;
        }

        // Update is called once per frame
        void Update()
        {
            float dTime = Time.deltaTime;
            if (isServer)
            {
                if (bTestSpawnFromServer)
                {
                    bTestSpawnFromServer = false;

                    ServerSpawn(this.gameObject, this.gameObject);
                }
            }
            if (isLocalPlayer && Time.time > spawnTime + waitForSpawnBeforeAcceptingInputs)
            {
                Vector3 moveVector = new Vector3(0, 0, 0);
                Vector3 rotEulerVector = new Vector3(0, 0, 0);
                //  rotation
                if (Input.GetKey(KeyCode.Q))
                {
                    rotEulerVector += new Vector3(0, -kKeyRotationRate * dTime, 0); //  turn left
                }
                else if (Input.GetKey(KeyCode.E))
                {
                    rotEulerVector += new Vector3(0, kKeyRotationRate * dTime, 0);   //  turn right
                }

                //  translation
                if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
                {
                    moveVector += new Vector3(0, 0, kKeySpeed * dTime);
                }
                else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
                {
                    moveVector += new Vector3(0, 0, -kKeySpeed * dTime);
                }
                if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
                {
                    moveVector += new Vector3(-kKeySpeed * dTime, 0, 0);
                }
                else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
                {
                    moveVector += new Vector3(kKeySpeed * dTime, 0, 0);
                }

                if (Input.GetKey(KeyCode.PageDown))
                {
                    moveVector += new Vector3(0, -kKeySpeed * dTime, 0);
                }
                else if (Input.GetKey(KeyCode.PageUp))
                {
                    moveVector += new Vector3(0, kKeySpeed * dTime, 0);
                }

                this.transform.Rotate(rotEulerVector);
                this.transform.localPosition += moveVector;

                //  hack to test spawning another player on a key press.
                if (Input.GetKeyDown(KeyCode.Insert))
                {
                    //NetworkSpawn();
                    if (Time.time > lastSpawnTime + waitForSpawnBeforeAcceptingInputs)
                    {
                        curSpawnIdx = (short)conn.playerControllers.Count;
                        CreatePlayerClient(curSpawnIdx++);
                        lastSpawnTime = Time.time;
                    }
                }
            }
        }
    }
}