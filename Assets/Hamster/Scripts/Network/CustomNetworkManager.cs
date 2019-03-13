using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

//#define SHOW_NETWORK_ERRORS
namespace customNetwork
{
    public class CustomNetworkManager : NetworkManager
    {
        const int kLastUnityMsgType = 255;   //  Use a number after Unity's own message types for our own message types. Note: it's not actually 255. But we're leaving room for Unity to grow its own enum list.
        const int kMaxShort = 32767;
        public enum hamsterMsgType
        {
            hmsg_serverLevel= kLastUnityMsgType,   //  server tells player what level is currently playing
            hmsg_serverVersion, //  server tells client what version it's running.
            hmsg_serverState,   //  the state the server is in. NOTE: this may cause the Cl
            hmsg_clientOpenMatchAck,    //  client affirms that OpenMatch start message was received through the server state.
            hmsg_serverOpenMatchAckBack,//  server acknowledges that it received the client's ack. This allows the client to shut down its connection to the static lobby server if it is still connected.
            //  the following are still incomplete. They are there as placeholders.
            hmsg_serverPlayerDied,  //  server tells player that they've died
            hmsg_serverPlayerFinished,  //  server tells player that they've finished this level
            hmsg_serverGameFinished,    //  server tells player that the game has been completed.
            hmsg_serverPlayerIsWinner,  //  server tells player that they're the winner!
            hmsg_newLevel,  //  server tells player that a new level has been loaded
            hmsg_EndOfMessageList = kMaxShort   //  do not use this. this just means that everything needs to be smaller than this number because the network message uses a short for this key.
        }
        const int kMaxConnections = 32;
        const int kMaxPlayersPerConnection = 32;
        public static CustomNetworkManager s_instance;
        public GameObject[,] plrObject = new GameObject[kMaxConnections, kMaxPlayersPerConnection];
        public bool m_AutoCreatePlayerFromSpawnPrefabList;
        static short curColorIndex = 0;
        static short curLocalPlayerID = 0;  //  we may have multiple controllers/players on this single client. We don't, but we could.
        private bool bServerVersionDoesntMatch=false;  //  if the server version is different, we should know about it.
        private string serverVersion;
        public MultiplayerGame multiPlayerGame;
        CustomNetworkManagerHUD hud;
        //  openmatch
        public bool[] ackReceived = new bool[kMaxConnections];  //  for OpenMatch start ACk
        Coroutine readyRoutine;
        public enum debugOutputStyle
        {
            db_none,
            db_log,
            db_warning,
            db_error
        };

        public debugOutputStyle m_debug_style;  //  this allows us to use the console window settings to filter out info, warnings, or errors to be able to highlight these debug messages.
        void DebugOutput(string st)
        {
            DebugOutput(st, null);
            Debug.Log(st);
        }

        void DebugOutput(string st, params object[] args)
        {
            switch (m_debug_style)
            {
                default:
                    break;
                case debugOutputStyle.db_none:
                    break;
                case debugOutputStyle.db_log:
                    if (args != null)
                        Debug.LogFormat(st, args);
                    else
                        Debug.Log(st);
                    break;
                case debugOutputStyle.db_warning:
                    if (args != null)
                        Debug.LogWarningFormat(st, args);
                    else
                        Debug.LogWarning(st);
                    break;
                case debugOutputStyle.db_error:
                    if (args != null)
                        Debug.LogErrorFormat(st, args);
                    else
                        Debug.LogError(st);
                    break;

            }
        }
        public bool bIsHost;
        public bool bIsServer;      //  a host can be both a server and a client
        public bool bIsClient;      //
        public NetworkClient myClient;
        public NetworkConnection toServerConnection;  //  my connection to the server if I'm a client
        public List<NetworkConnection> client_connections;  //  as a server, these are the connections to me

        public MultiplayerGame getMultiPlayerPointer()
        {
            if (multiPlayerGame == null)
            {
                multiPlayerGame = GetComponent<MultiplayerGame>();
            }
            if (multiPlayerGame == null)
            {
                Debug.LogError("MultiPlayerGame component needs to be on the same object as CustomNetworkManager!\n");
            }
            return multiPlayerGame;
        }
        //  this centralizes the hack so that if the bug is fixed, we can just fix it here rather than in many places that are using either numPlayers or client_connections.Count
        public int getNumPlayers()
        {
            //return client_connections.Count;  //  this is technically correct, but doesn't work
            return numPlayers;  //  bug fix hack: so we use this 
        }
        //  need to put this somewhere. This is just fine for now.
        static public string LocalHostname()
        {
            System.Net.IPHostEntry host;
            string localHostname = System.Net.Dns.GetHostName();
            host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            //foreach (System.Net.IPAddress ip in host.AddressList)
            //{
            //    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            //    {
            //        localHostname = ip.Address.();
            //        break;
            //    }
            //}
            return localHostname;

        }
        static public string LocalIPAddress()
        {
            System.Net.IPHostEntry host;
            string localIP = "";
            host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            foreach (System.Net.IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }


        public void CreateNetworkPlayer(short localPlayerID)
        {
            if (!clientLoadedScene)
            {
                RequestServerSpawn(toServerConnection, (short)localPlayerID);
            }
        }
        //
        // Summary:
        //     Called on the client when connected to a server.
        //
        // Parameters:
        //   conn:
        //     Connection to the server.
        //  client makes connection to the server. 
        public override void OnClientConnect(NetworkConnection conn)
        {
            DebugOutput("CustomNetworkManager.OnClientConnect\n");
            toServerConnection = conn;
            CustomNetworkPlayer.conn = conn;
            GetMultiplayerPointer();
            if (multiPlayerGame != null)
            {
                multiPlayerGame.OnClientConnect(conn);
            }
            if (m_AutoCreatePlayerFromSpawnPrefabList)
                CreateNetworkPlayer(curLocalPlayerID);
        }

        /*
         *  The client requests a server spawn give a localControllerId.
         *  localControllerId - If the local client has multiple controllers (i.e. multiple players on a single client).
         */
        public void RequestServerSpawn(NetworkConnection conn, short localControllerId = 0)
        {
            DebugOutput("CustomNetworkManager.RequestServerSpawn:" + conn.ToString() + "localControllerID=" + localControllerId.ToString());
            if (!conn.isReady)  //  don't call this if we're already ready. 
            {
                ClientScene.Ready(conn);
            }
            CustomNetworkPlayer.CreatePlayerClient(localControllerId);
            //        ClientScene.AddPlayer(0);

        }
        //
        // Summary:
        //     Called on clients when disconnected from a server.
        //
        // Parameters:
        //   conn:
        //     Connection to the server.
        public override void OnClientDisconnect(NetworkConnection conn) {
            base.OnClientConnect(conn);
            DebugOutput("CustomNetworkManager.OnClientDisconnect:" + conn.ToString());
            NetworkClient.ShutdownAll();
        }
        //
        // Summary:
        //     Called on clients when a network error occurs.
        //
        // Parameters:
        //   conn:
        //     Connection to a server.
        //
        //   errorCode:
        //     Error code.
        public virtual void OnClientError(NetworkConnection conn, int errorCode) {
            DebugOutput("CustomNetworkManager.OnClientError:" + conn.ToString() + "errorCode=" + errorCode.ToString());
        }
        //
        // Summary:
        //     Called on clients when a servers tells the client it is no longer ready.
        //
        // Parameters:
        //   conn:
        //     Connection to a server.
        public virtual void OnClientNotReady(NetworkConnection conn) {
            DebugOutput("CustomNetworkManager.OnClientNotReady:" + conn.ToString());
        }
        //
        // Summary:
        //     Called on clients when a Scene has completed loaded, when the Scene load was
        //     initiated by the server.
        //
        // Parameters:
        //   conn:
        //     The network connection that the Scene change message arrived on.
        public virtual void OnClientSceneChanged(NetworkConnection conn) {
            DebugOutput("CustomNetworkManager.OnClientSceneChanged:" + conn.ToString());
        }
        //
        // Summary:
        //     Callback that happens when a NetworkMatch.DestroyMatch request has been processed
        //     on the server.
        //
        // Parameters:
        //   success:
        //     Indicates if the request succeeded.
        //
        //   extendedInfo:
        //     A text description for the error if success is false.
        public virtual void OnDestroyMatch(bool success, string extendedInfo) {
            DebugOutput("CustomNetworkManager.OnDestroyMatch:" + success.ToString() + "extendedInfo=" + extendedInfo);
        }
        //
        // Summary:
        //     Callback that happens when a NetworkMatch.DropConnection match request has been
        //     processed on the server.
        //
        // Parameters:
        //   success:
        //     Indicates if the request succeeded.
        //
        //   extendedInfo:
        //     A text description for the error if success is false.
        public virtual void OnDropConnection(bool success, string extendedInfo) {
            DebugOutput("CustomNetworkManager.OnDropConnection:" + success.ToString() + "extendedInfo=" + extendedInfo);
        }
        //
        // Summary:
        //     Callback that happens when a NetworkMatch.CreateMatch request has been processed
        //     on the server.
        //
        // Parameters:
        //   success:
        //     Indicates if the request succeeded.
        //
        //   extendedInfo:
        //     A text description for the error if success is false.
        //
        //   matchInfo:
        //     The information about the newly created match.
        public virtual void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo) {
            DebugOutput("CustomNetworkManager.OnMatchCreate:" + success.ToString() + "extInfo= " + extendedInfo + "MatchInfo=" + matchInfo.ToString());
        }
        //
        // Summary:
        //     Callback that happens when a NetworkMatch.JoinMatch request has been processed
        //     on the server.
        //
        // Parameters:
        //   success:
        //     Indicates if the request succeeded.
        //
        //   extendedInfo:
        //     A text description for the error if success is false.
        //
        //   matchInfo:B
        //     The info for the newly joined match.
        public virtual void OnMatchJoined(bool success, string extendedInfo, MatchInfo matchInfo) {
            DebugOutput("CustomNetworkManager.OnMatchJoined:" + success.ToString() + "matchInfo=" + matchInfo.ToString());
        }
        public virtual void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matchList) {
            DebugOutput("CustomNetworkManager.OnMatchList:" + success.ToString() + "matchList=" + matchList.Count.ToString());
        }

        void OnServerAddPlayerAutoPickPrefabInternal(NetworkConnection conn, short playerControllerId)
        {
            int prefabId = conn.connectionId % spawnPrefabs.Count;
            curColorIndex++;    //  hack: cycle through the colors. This is a hack because we don't have enough playerController to do this indefinitely!
            OnServerAddPlayerInternal(this.spawnPrefabs[prefabId], conn, playerControllerId);
        }
        /*
         *  This is sucessfully getting called even if there is no actual debug output.
         */
        void OnServerAddPlayerInternal(GameObject prefabToInstantiate, NetworkConnection conn, short playerControllerId)
        {
            Debug.Log("CustomNetworkManager.OnServerAddPlayerInternal: " + conn.ToString() + ", plrControllerId="+playerControllerId.ToString());
            /*
            if (m_PlayerPrefab == null)
            {
                if (LogFilter.logError) { Debug.LogError("The PlayerPrefab is empty on the NetworkManager. Please setup a PlayerPrefab object."); }
                return;
            }

            if (m_PlayerPrefab.GetComponent<NetworkIdentity>() == null)
            {
                if (LogFilter.logError) { Debug.LogError("The PlayerPrefab does not have a NetworkIdentity. Please add a NetworkIdentity to the player prefab."); }
                return;
            }
            */
            var id = playerControllerId; //conn.connectionId;
            if (id < conn.playerControllers.Count && conn.playerControllers[id].IsValid && conn.playerControllers[id].gameObject != null)
            {
                //  note: it seems that Unity already assigns a playerControllerId in its mysterious black box. So, the first connection with come here and already have a player. I'm not sure how.
                //  it seems that the playerController list is maintained on the client, but not on the server!!!
                //  to be clear: It seems to be normal that playerControllerId==0 passes through here. So, we skip those errors for playerControllerId==0
                if (LogFilter.logError) { Debug.LogError("There is already a player at that playerControllerId for this connectionId=" + conn.connectionId.ToString()); }
                if (LogFilter.logError) { Debug.LogError("id=" + id.ToString()); }
                if (LogFilter.logError) { Debug.LogError("id name=" + conn.playerControllers[id].gameObject.name); }
                return;
            }

            GameObject player;
            Transform startPos = GetStartPosition();
            if (startPos != null)
            {
                player = (GameObject)Instantiate(prefabToInstantiate, startPos.position, startPos.rotation);
                if (player != null)
                    plrObject[conn.connectionId, playerControllerId] = player;
            }
            else
            {
                player = (GameObject)Instantiate(prefabToInstantiate, Vector3.zero, Quaternion.identity);
                if (player != null)
                    plrObject[conn.connectionId, playerControllerId] = player;
            }

            NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
        }

        /*
         * Server responds to Client calling ClientScene.AddPlayer here.
         * This is successfully getting called even though the debug info doesn't print out.
         */
        public override void OnServerAddPlayer(NetworkConnection conn, short localPlayerControllerId, NetworkReader extraMessageReader)
        {
            DebugOutput("CustomNetworkManager.OnServerAddPlayer: " + conn.ToString());
            if (spawnPrefabs.Count <= 0)
            {
                Debug.LogError("Registered Spawnable Prefabs list must contain at least one player prefab at index 0 to spawn the player.");
            }
            OnServerAddPlayerAutoPickPrefabInternal(conn, localPlayerControllerId);
        }
        //
        // Summary:
        //     Called on the server when a client adds a new player with ClientScene.AddPlayer.
        //
        // Parameters:
        //   conn:
        //     Connection from client.
        //
        //   playerControllerId:
        //     Id of the new player.
        //
        //   extraMessageReader:
        //     An extra message object passed for the new player.
        public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
        {
            DebugOutput("CustomNetworkManager.OnServerAddPlayer: " + conn.ToString());
            getMultiPlayerPointer().notifyServerClientStart(conn);
            OnServerAddPlayerAutoPickPrefabInternal(conn, playerControllerId);
        }

        void CreateClientConnections(NetworkConnection conn)
        {
            if (client_connections == null)
            {
                client_connections = new List<NetworkConnection>();
            }
            if (!client_connections.Contains(conn))
            {
                Debug.LogFormat("CreateClientConnections: Adding client connection: {0}", conn);
                client_connections.Add(conn);
            }
        }
        //
        // Summary:
        //     Called on the server when a new client connects.
        //
        // Parameters:
        //   conn:
        //     Connection from client.
        public override void OnServerConnect(NetworkConnection conn)
        {
            DebugOutput("CustomNetworkManager.OnServerConnect: connId=" + conn.connectionId.ToString() + "\n");
            base.OnServerConnect(conn);
            CreateClientConnections(conn);
        }
        public void DestroyConnectionsPlayerControllers(NetworkConnection conn)
        {
            Debug.LogFormat("DestroyConnectionsPlayerControllers: Destroying {0} playerControllers for connection: {1}", conn.playerControllers.Count, conn);
            for(int ii=0; ii<conn.playerControllers.Count; ii++ )
            {
                Destroy(conn.playerControllers[ii].gameObject);
            }
            conn.playerControllers.Clear(); //  once we disconnect, all of our playerControllers are destroyed.
        }
        //
        // Summary:
        //     Called on the server when a client disconnects.
        //
        // Parameters:
        //   conn:
        //     Connection from client.
        public override void OnServerDisconnect(NetworkConnection conn)
        {
            base.OnServerDisconnect(conn);
            Debug.LogError("CustomNetworkManager.OnServerDisconnect: connId=" + conn.connectionId.ToString() + "\n");
            if (this.client_connections.Contains(conn)) {
                DestroyConnectionsPlayerControllers(conn);
                this.client_connections.Remove(conn);
                this.setAck(conn.connectionId, false);
                Debug.LogFormat("OnServerDisconnect: {0} connections remaining", this.client_connections.Count);
            }
        }

        //  obsolete in 2018.2
        //void OnDisconnectedFromServer(NetworkDisconnection info)
        //{
        //    Debug.Log("Disconnected from server: " + info);
        //}
        void OnApplicationQuit()
        {
            //  stop all the things, whatever we are.
            //this.StopClient();
            //this.StopHost();
            //this.StopServer();

            Debug.Log("Application ending after " + Time.time + " seconds");
            for (int ii = 0; ii < NetworkClient.allClients.Count; ii++)
            {
                NetworkClient.allClients[ii].connection.Disconnect();
                OnServerDisconnect(NetworkClient.allClients[ii].connection);    //  kill all of our connections
            }
        }
        //
        // Summary:
        //     Called on the server when a network error occurs for a client connection.
        //
        // Parameters:
        //   conn:
        //     Connection from client.
        //
        //   errorCode:
        //     Error code.
        public override void OnServerError(NetworkConnection conn, int errorCode) {
            base.OnServerError(conn, errorCode);
            this.client_connections.Remove(conn);   //  maybe do this, maybe do something else. If we can no longer talk to this client, we may want to let the other clients know.
        }
        //
        // Summary:
        //     Called on the server when a client is ready.
        //      NOTE: "ready" in this case means that ClientScene.Ready(conn) was called. But that function is obsolete.
        // Parameters:
        //   conn:
        //     Connection from client.
        public override void OnServerReady(NetworkConnection conn)
        {
            bool isHost;
            isHost = NetworkClient.active && NetworkServer.active;

            Debug.LogWarning("OnServerReady: " + conn.ToString()+"\n");
            //  we need to tell the client what level we've loaded.
            int levelIdx = -1;  //  no level is loaded that we know of (yet).
            if (Hamster.CommonData.gameWorld != null)   //  if the game knows about a levelIndex, then send it.
            {
                levelIdx = Hamster.CommonData.gameWorld.curLevelIdx;
                Debug.LogWarning("OnServerReady levelIdx=" + levelIdx.ToString() + ", isHost=" + isHost.ToString() + "\n");
            }
            else
            {
                Debug.LogWarning("Server didn't notify clients of server level idx");
            }
            if (conn.connectionId != conn.hostId)    //  hack: let the host choose a level through the menu! But other servers need to tell their clients what level to load
            {
                MessageBase msg = new UnityEngine.Networking.NetworkSystem.IntegerMessage(levelIdx);    //  test: yep, just send a number without any context for now. Later, wrap this in an appropriate MessageBase class.
                Debug.LogWarning("conn.Send server level: " + levelIdx.ToString() + " to " + conn.ToString());
                conn.Send((short)hamsterMsgType.hmsg_serverLevel, msg); //  tell our client what level we're using.
            }
            SendServerVersion(conn);
            CreateClientConnections(conn);
        }

        public void setAck(int connId, bool bit=true)
        {
            Debug.LogFormat("setAck: ACK bit for connId {0} changed from {1} to {2}", connId, ackReceived[connId], bit);
            ackReceived[connId] = bit; //  set or reset the OpenMatch ack.
        }

        void SendServerVersion(NetworkConnection conn)
        {
            serverVersion = Application.version;
            MessageBase serverVersionMsg = new UnityEngine.Networking.NetworkSystem.StringMessage(serverVersion);
            conn.Send((short)hamsterMsgType.hmsg_serverVersion, serverVersionMsg);
        }

        //  client wants the server version.
        //  [Command]   // this is not a NetworkBehaviour, so it won't work.
        void Cmd_SendServerVersion(int connectionId)
        {
            serverVersion = Application.version;
            MessageBase serverVersionMsg = new UnityEngine.Networking.NetworkSystem.StringMessage(serverVersion);
            NetworkServer.SendToClient(connectionId, (short)hamsterMsgType.hmsg_serverVersion, serverVersionMsg);
        }

        //  we will send the server state when the client needs to know it for some specific reason.
        //[Command]   // this is not a NetworkBehaviour, so it won't work.
        public void Cmd_SendServerState(int connectionId, string serverState)
        {
            MessageBase serverStateMsg = new UnityEngine.Networking.NetworkSystem.StringMessage(serverState);
            NetworkServer.SendToClient(connectionId, (short)hamsterMsgType.hmsg_serverState, serverStateMsg);
        }

        float lastServerVersionRequestTime = 0.0f;
        void RequestServerVersion(NetworkIdentity netid)
        {
            const float kWaitBetweenRequests = 30.0f;
            if (Time.fixedTime > lastServerVersionRequestTime + kWaitBetweenRequests)
            {
                Cmd_SendServerVersion(netid.connectionToServer.connectionId);
                lastServerVersionRequestTime = Time.fixedTime;
            }
        }

        //  I often put a trailing letter at the end of the version to distinguish between different versions I may make in a single day. This may cause problems if it's not there.
        //  so this method extracts the version number somewhat better, though it's also not perfect
        static public string getStrippedVersionNumber(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return "";

            string strippedVersion = raw.TrimEnd(raw[raw.Length - 1]);  //  this is how we used to do it. By default, always remember to put a single trailing character.
            double dResult;
            bool bSuccess = double.TryParse(raw, out dResult);  //  this should do the majority of the work. The above is the failsafe, but is probably even worse than TryParse.
            if (bSuccess)
            {
                strippedVersion = dResult.ToString();
            }
            return strippedVersion;
        }

        public double getServerVersionDouble(NetworkIdentity netid=null)
        {
            if (string.IsNullOrEmpty(serverVersion))
            {
                //  maybe we didn't get the server message. So request a resend of the server version.
                if (netid != null) ;
                RequestServerVersion(netid);
                return 0;
            }

            string serverVerStr = getStrippedVersionNumber(serverVersion);   //  strip off the single letter at the end.
            double serverVersionDbl = System.Convert.ToDouble(serverVerStr);
            return serverVersionDbl;
        }
        //
        // Summary:
        //     Called on the server when a client removes a player.
        //
        // Parameters:
        //   conn:
        //     The connection to remove the player from.
        //
        //   player:
        //     The player controller to remove.
        public virtual void OnServerRemovePlayer(NetworkConnection conn, PlayerController player) { }
        //
        // Summary:
        //     Called on the server when a Scene is completed loaded, when the Scene load was
        //     initiated by the server with ServerChangeScene().
        //
        // Parameters:
        //   sceneName:
        //     The name of the new Scene.
        public override void OnServerSceneChanged(string sceneName) {
            Debug.Log("CustomNetworkManager.OnServerSceneChanged. Server scene changed: " + sceneName  + "\n");
            if (sceneName == "NetworkMainGameScene")
            {

                MultiplayerGame.instance.ServerEnterMultiPlayerState<Hamster.States.ServerNetworkManagerOnlineSceneLoaded>();
            }
            else
            {
                Debug.LogWarning("MechaHamster code did not anticipate this scene change: " + sceneName);   //  you're probably extending this code. that's fine. The original code was hacked together specifically for MechaHamster, and thus is not as generic as I would like it to be.
            }
        }
        //
        // Summary:
        //     Callback that happens when a NetworkMatch.SetMatchAttributes has been processed
        //     on the server.
        //
        // Parameters:
        //   success:
        //     Indicates if the request succeeded.
        //
        //   extendedInfo:
        //     A text description for the error if success is false.
        public virtual void OnSetMatchAttributes(bool success, string extendedInfo) { }
        //
        // Summary:
        //     This is a hook that is invoked when the client is started.
        //
        // Parameters:
        //   client:
        //     The NetworkClient object that was started. This client was created on this machine. But may not have connected yet necessarily.
        public override void OnStartClient(NetworkClient client)
        {
            Hamster.CommonData.networkmanager = this;    //  there's probably a better place to put this.
            DebugOutput("CustomNetworkManager.OnStartClient\n");
            bIsClient = true;
            myClient = client;
            toServerConnection = client.connection;

            client.RegisterHandler((short)hamsterMsgType.hmsg_serverLevel, OnClientLevelMsg);
            client.RegisterHandler((short)hamsterMsgType.hmsg_serverVersion, OnClientVersion);
            client.RegisterHandler((short)hamsterMsgType.hmsg_serverState, OnClientServerState);
            
            //  Hamster.MainGame.NetworkSpawnPlayer(toServerConnection);  //  don't do this yet. Let the weird legacy Hamster code do it in its FixedUpdate, even though it's bad.
        }

        void LoadLevel(int levelIdx)
        {
            Hamster.States.LevelSelect lvlSel = new Hamster.States.LevelSelect();   //  create new state for FSM that will let us force the starting level.
            lvlSel.ForceLoadLevel(levelIdx); //  this is just the stub that initiates the state. It needs to run its update at least once before it has actually loaded any levels.
            Hamster.CommonData.mainGame.stateManager.ClearStack(lvlSel);    //  hack: Just slam that state in there disregarding all previous states! OMG!!!
        }
        //  we received a message from the server about what level the server is currently running.
        //  handles hamsterMsgType.hmsg_serverLevel
        void OnClientLevelMsg(NetworkMessage netMsg)
        {
            UnityEngine.Networking.NetworkSystem.IntegerMessage intMsg = netMsg.ReadMessage<UnityEngine.Networking.NetworkSystem.IntegerMessage>();
            int levelToLoad = intMsg.value;
            DebugOutput("OnClientLevelMsg: recved Server level request:" + levelToLoad.ToString());
            //  our server has declared a level that it has already loaded. Let's try to load that level.
            MultiplayerGame.instance.ClientSwapMultiPlayerState<Hamster.States.ClientLoadingLevel>(levelToLoad); //  make our client go into the OpenMatch server state!

            //  this is obsolete. Try to use the states to do this so that the Client level load is more sensible that relying on timing!
            //if (intMsg.value >= 0)
            //{
            //    LoadLevel(intMsg.value);
            //}
        }

        public bool isServerAndClientVersionMatch(out string serverV)
        {
            bool bIsMatched = !bServerVersionDoesntMatch;
            serverV = serverVersion;

            return bIsMatched;
        }
        void OnClientVersion(NetworkMessage netMsg)
        {
            UnityEngine.Networking.NetworkSystem.StringMessage strMsg = netMsg.ReadMessage<UnityEngine.Networking.NetworkSystem.StringMessage>();
            serverVersion = strMsg.value;
            string clientVersion = Application.version;
            Debug.Log("Client received Server version=" + serverVersion);
            if (serverVersion != clientVersion)
            {
                Debug.LogError("Server Version=" + serverVersion + " does not match client=" + clientVersion);
                bServerVersionDoesntMatch = true;
            }
            else
            {
                bServerVersionDoesntMatch = false;
            }
        }
        //  the server has sent the server state to us.
        void OnClientServerState(NetworkMessage netMsg)
        {
            UnityEngine.Networking.NetworkSystem.StringMessage strMsg = netMsg.ReadMessage<UnityEngine.Networking.NetworkSystem.StringMessage>();
            string serverState = strMsg.value;
            Debug.LogWarning("Client received Server state=" + serverState);
            if (serverState.Contains("ServerOpenMatchStart")) {
                //  the server has told us to start open match on this client.
                MultiplayerGame.instance.ClientSwapMultiPlayerState<Hamster.States.ClientOpenMatchStart>(); //  make our client go into the OpenMatch server state!
            }
        }
        //
        // Summary:
        //     This hook is invoked when a host is started.
        public override void OnStartHost()
        {
            Hamster.CommonData.networkmanager = this;    //  there's probably a better place to put this.
            DebugOutput("CustomNetworkManager.OnStartHost\n");
            bIsHost = true;
        }

        //  server handles hmsg_clientOpenMatchAck
        //  SERVER - one of my clients is telling me that it's ready.
        void svrOnClientReady(NetworkMessage netMsg)
        {
            UnityEngine.Networking.NetworkSystem.IntegerMessage intMsg = netMsg.ReadMessage<UnityEngine.Networking.NetworkSystem.IntegerMessage>();
            
            //int clientConnId = intMsg.value;    //  this client told us the connectionID they *think* they're on. But Unity plays a joke on us. They're different than the server ones for some reason. See this: https://docs.unity3d.com/ScriptReference/Networking.NetworkConnection-connectionId.html
            int clientConnId = netMsg.conn.connectionId;
            Debug.LogWarning("Server received OpenMatch ACK from client(" + clientConnId.ToString() + ")\n");
            this.setAck(clientConnId, true);
        }

        //
        // Summary:
        //     This hook is invoked when a server is started - including when a host is started. This server was created on this machine
        public override void OnStartServer()
        {
            Hamster.CommonData.networkmanager = this;    //  there's probably a better place to put this.
            DebugOutput("CustomNetworkManager.OnStartServer\n");
            bIsServer = true;
            //  register my server handlers
            NetworkServer.RegisterHandler((short)hamsterMsgType.hmsg_clientOpenMatchAck, svrOnClientReady);
        }
        //
        // Summary:
        //     This hook is called when a client is stopped.
        public override void OnStopClient()
        {
            DebugOutput("CustomNetworkManager.OnStopClient\n");
            base.OnStopClient();
        }
        //
        // Summary:
        //     This hook is called when a host is stopped.
        public override void OnStopHost()
        {
            DebugOutput("CustomNetworkManager.OnStopHost\n");
        }
        //
        // Summary:
        //     This hook is called when a server is stopped - including when a host is stopped.
        public override void OnStopServer()
        {
            DebugOutput("CustomNetworkManager.OnStopServer\n");
        }

        MultiplayerGame GetMultiplayerPointer()
        {
            if (multiPlayerGame == null)
                multiPlayerGame = UnityEngine.GameObject.FindObjectOfType<MultiplayerGame>();
            return multiPlayerGame;
        }

        public void OnGUIShowClientDebugInfo(CustomNetworkManagerHUD localHUD)
        {
            string strMsg;
            if (hud==null)
            {
                hud = this.GetComponent<CustomNetworkManagerHUD>();
            }
            localHUD = hud;
            if (localHUD != null && this.client_connections != null)
            {
                hud.scaledTextBox("Server's client info: numPlayers=" + this.numPlayers.ToString() +", nClients=" + this.client_connections.Count.ToString());

                strMsg = "client connections";
                strMsg += "(" + this.client_connections.Count.ToString()+")\n";
                if (this.client_connections!=null && this.client_connections.Count >0)
                    strMsg += this.client_connections[0].connectionId.ToString();
                for (int ii=1; ii<this.client_connections.Count; ii++)
                {
                    strMsg += ", " + this.client_connections[ii].connectionId.ToString();
                }
                hud.scaledTextBox(strMsg);
            }
        }

        private IEnumerator ReadyUpdate(float delay)
        {
            GetMultiplayerPointer();

            while (true)
            {
                if (multiPlayerGame != null && multiPlayerGame.agones != null)
                {
                    multiPlayerGame.agones.Ready();
                }
                else
                {
                    Debug.Log("CustomNetworkManager::ReadyUpdate() - Problem: multiPlayerGame={0}", multiPlayerGame);
                }

                yield return new WaitForSeconds(delay);
            }
        }

        public void StartReadyRoutine(float delay)
        {
            if (readyRoutine == null)
            {
                readyRoutine = StartCoroutine(ReadyUpdate(delay));
            }
            else
            {
                Debug.Log("CustomNetworkManager: StartReadyRoutine: readyRoutine is already allocated.");
            }
        }

        public void StopReadyRoutine()
        {
            if (readyRoutine != null)
            {
                Debug.Log("CustomNetworkManager: StopReadyRoutine: Stopping readyRoutine");
                StopCoroutine(readyRoutine);
                readyRoutine = null;
            }
            else
            {
                Debug.Log("CustomNetworkManager: StopReadyRoutine: readyRoutine is already de-allocated");
            }
        }
    }
}
