using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

using Hamster;
using Hamster.States;
/*
 * Note: Although it would be cleaner if this was a separate GameObject than NetworkManager, the way Unity works makes that more trouble than it's worth. Because the NetworkManager
 * is set to DontDestroyOnLoad, it gets put into a separate kind of scene than the other objects. They are separated such that one cannot refer to the other. Since MultiplayerGame needs to
 * refer to NetworkManager, that makes that structure a no-go. Thus, it's simply placed as a component onto the same Component as CustomNetworkManager. It's unclear what ramifications this may
 * have later down the road. But for now, it works.
 */
public class MultiplayerGame : /*NetworkBehaviour */MonoBehaviour
{
    //  game constants
    public const int kMaxPlayers = 4;
    public const int kMaxObservers = 1;
    public const int kMaxConnections = kMaxPlayers + kMaxObservers;
    //public const int kOpenMatchThreshold = kMaxConnections;    //  kMaxPlayers;    //  this is when OpenMatch fires. Hack: test 5 connections when we're testing end of game triggers.
    public const int kOpenMatchThreshold = kMaxPlayers;    //  GM: The above is WIP; fixing this for now back to the old way so that game clients can connect

    static public MultiplayerGame s_instance;
    static public MultiplayerGame instance {
        get
        {
            if (s_instance != null)
            {
                if (s_instance.manager == null)
                {
                    GameObject mgrGO = GameObject.FindGameObjectWithTag("NetworkManager");
                    if (mgrGO != null)
                    {
                        s_instance.manager = mgrGO.GetComponent<NetworkManager>();
                    }
                }
            }
            return s_instance;
        }
    }

    //  config file for multiplayer game
    public const string kConfigJsonBasename = "MHConfig";
    public const string kConfigJson = kConfigJsonBasename + ".json";

    public bool autoStartServer;    //  if we want the server to automatically start. This is mostly for debugging because Unity doesn't play nice with Start() and Network scenes!
    public NetworkManager manager;  //  developer should fill this field in Unity inspector.
    public JsonStartupConfig config;
    public ConnectionConfig connConfig; //  deprecated

    //  warning: These should be private, but are public for convenience for debugging and displaying state info. Use with appropriate caution. Try to only do read operations and not write operations on these states.
    public StateManager clientStateManager = new StateManager();    //  this is for the client. The server is separated because "Host" can have both on the same machine!
    public StateManager serverStateManager = new StateManager();    //  our statemachine that is separate from the MainGame state machine for single player.

    public int numPlayers = 1;  //  this is the number of players who can join. Autostart after this number is reached on the server
    public int defaultLevelIdx = 0;
    public int startingLevel;
    //public float[] playerFinishTimes = new float[kMaxPlayers];  //  where we record the finish times of the players.
    public NetworkConnection[] networkConnections = new NetworkConnection[kMaxPlayers];
    public Dictionary<int, float> startTimes;
    public Dictionary<int, float> finishTimes;
    public Dictionary<int, NetworkConnection> finishedClients;

    string serverAddress;
    string serverPort;

    //  Graeme's server stuff moved from CustomNetworkManagerHUD.cs
    public AgonesClient agones;
    public OpenMatchClient openMatch;

    //  for debugging
    public string curState;
    public void ReadConfig()
    {
        MultiplayerGame mpgame = this;
        mpgame.config = FindObjectOfType<JsonStartupConfig>();

        if (mpgame.config != null)
        {
            if (!mpgame.config.isConfigLoaded)
            {  //  strange, this should have already been loaded. But Unity timing for Start is weird, so we'll just load it anyway.
                mpgame.config.ReadJsonStartupConfig();
            }
            mpgame.serverAddress = mpgame.config.startupConfig.serverIP;
            mpgame.serverPort = mpgame.config.startupConfig.serverPort;
            mpgame.startingLevel = mpgame.config.startupConfig.startingLevel;
        }
    }

    void ClearFinishTimes()
    {
        if (startTimes != null)
            startTimes.Clear();
        if (finishTimes != null)
            finishTimes.Clear();
        if (finishedClients != null)
            finishedClients.Clear();
        //for (int ii = 0; ii < kMaxPlayers; ii++)
        //{
        //    playerFinishTimes[ii] = -1.0f;  //  initialize to negative time.
        //}
    }
    //  typical unity stuff below
    private void Awake()
    {
        if (s_instance == null)
        {
            s_instance = this;
            startTimes  = new Dictionary<int, float>();
            finishTimes = new Dictionary<int, float>();
            finishedClients = new Dictionary<int, NetworkConnection>();   //  clients which have finished the race.
        }
        DontDestroyOnLoad(this.gameObject); //  because NetworkManager has been set to DontDestroyOnLoad, it will be in a separate scene hierarchy/memory segment that cannot interact with this. Thus we must be in the same "zone" as the NetworkManager! Ugh, Unity!
        ClearFinishTimes();
    }

    //  use startLevelidx==-1 to choose a level through the menu.
    public void EnterServerStartupState(int startLevelidx)
    {
        Debug.Log("EnterServerStartupState(" + startLevelidx.ToString() + ")\n");
        ServerEnterMultiPlayerState<Hamster.States.ServerStartup>(startLevelidx);
    }

    //  swap the state
    public void ClientSwapMultiPlayerState<T>(int mode = 0, bool isSwapState = false) where T : Hamster.States.BaseState, new()
    {
        MultiplayerGame.EnterMultiPlayerState<T>(clientStateManager, mode, true);
    }
    //  swap the state
    public void ServerSwapMultiPlayerState<T>(int mode = 0, bool isSwapState = false) where T : Hamster.States.BaseState, new()
    {
        MultiplayerGame.EnterMultiPlayerState<T>(serverStateManager, mode, true);
    }

    //  push the state
    public void ClientEnterMultiPlayerState<T>(int mode = 0, bool isSwapState = false) where T : Hamster.States.BaseState, new()
    {
        MultiplayerGame.EnterMultiPlayerState<T>(clientStateManager, mode, isSwapState);
    }
    //  push the state
    public void ServerEnterMultiPlayerState<T>(int mode=0, bool isSwapState = false) where T : Hamster.States.BaseState, new()
    {
        MultiplayerGame.EnterMultiPlayerState<T>(serverStateManager, mode, isSwapState);
    }

    public void ClientPopState()
    {
        PopState(this.clientStateManager);
    }
    public void ServerPopState()
    {
        PopState(this.serverStateManager);
    }
    static void PopState(StateManager stateManager)
    {
        stateManager.PopState();
    }
    //  this is private. Use MultiplayerGame.instance.ClientEnterMultiPlayerState or MultiplayerGame.instance.ClientEnterMultiPlayerState to make explicit whether server or client FSM is affected.
    static private Hamster.States.BaseState EnterMultiPlayerState<T>(StateManager stateManager,  int mode=0, bool isSwapState = false) where T : Hamster.States.BaseState, new()
    {
        Hamster.States.BaseState state = new T();
        Debug.Log("State change: " + stateManager.CurrentState().ToString() + "->: " + state.ToString() /*+ "(swap/push=" + isSwapState.ToString()*/  + "(" + mode.ToString() + ")\n");
        //  some states require the mode. Pass that along here.
        //  this is an ugly way to pass variables, but I didn't want to change the core of stateManager
        ServerStartup serverStartupState = state as ServerStartup;
        if (serverStartupState!=null)
        {
            serverStartupState.levelIdx = mode;
        }
        ServerLoadingLevel serverLoadLevel = state as ServerLoadingLevel;
        if (serverLoadLevel != null)
        {
            Debug.Log("Server Loading Level=" + mode.ToString());
            serverLoadLevel.levelIdx = mode;
        }
        ClientLoadingLevel clientLoadLevel = state as ClientLoadingLevel;
        if (clientLoadLevel != null)
        {
            Debug.Log("Client Loading Level=" + mode.ToString());
            clientLoadLevel.levelIdx = mode;

        }
        if (isSwapState)
        {
            stateManager.SwapState(state);
        }
        else
        {
            stateManager.PushState(state);
        }
        return state;
    }

    //  This is called on the Client. The client should have only one connection to the server.
    public void OnClientConnect(NetworkConnection conn)
    {
        networkConnections[0] = conn;   //  we're on the client, so we have only ONE connection to the server.
        //Hamster.States.ClientConnected state = new Hamster.States.ClientConnected();   //  create new state for FSM that will let us force the starting level.
        //stateManager.PushState(state);
        //  this replaces the above with a templated version.
        BaseState prevState = MultiplayerGame.instance.clientStateManager.CurrentState();
        ClientConnected clientConnectedState =  MultiplayerGame.EnterMultiPlayerState<Hamster.States.ClientConnected>(MultiplayerGame.instance.clientStateManager) as ClientConnected;
        clientConnectedState.conn = conn;
        clientConnectedState.manager = manager;
        clientConnectedState.prevState = prevState;
    }

    //  the game is over. The server needs to tell the players that the game has ended!
    public void ServerGameOver()
    {
        customNetwork.CustomNetworkManager custMgr = manager as customNetwork.CustomNetworkManager;

        foreach (NetworkConnection conn in custMgr.client_connections)
        {
            MessageBase raceTimeMsg = new UnityEngine.Networking.NetworkSystem.StringMessage("race finished");
            Debug.LogError("ServerGameOver: hmsg_serverGameOver sent to: " + conn.connectionId.ToString());

            NetworkServer.SendToClient(conn.connectionId, (short)customNetwork.CustomNetworkManager.hamsterMsgType.hmsg_serverGameOver, raceTimeMsg);
        }
    }

    //[Command]
    public void Cmd_clientStartedGame(NetworkConnection conn)
    {
        this.startTimes[conn.connectionId] = Time.realtimeSinceStartup;
        Debug.LogWarning("Cmd_clientStartedGame: Start t=" + this.startTimes[conn.connectionId].ToString() + " conn=" + conn.ToString());
    }

    //  this doesn't actually seem to work for some reason. Maybe the message is too long?
    public void server_SendRaceTime(int connectionId, float raceTime)
    {
        string raceTimeStr = raceTime.ToString();   //  raw time. We might want to send this instead of the formatted time.

        //long elaspedTimeinMS = (long)(System.Convert.ToInt64(raceTime * 1000.0f));
        //raceTimeStr = string.Format(Hamster.StringConstants.FinishedTimeText, Hamster.Utilities.StringHelper.FormatTime(elaspedTimeinMS));

        Debug.LogWarning("Server send racetime=" + raceTimeStr + " to connId=" + connectionId.ToString());

        MessageBase raceTimeMsg = new UnityEngine.Networking.NetworkSystem.StringMessage(raceTimeStr);
        NetworkServer.SendToClient(connectionId, (short)customNetwork.CustomNetworkManager.hamsterMsgType.hmsg_serverPlayerFinished, raceTimeMsg);
    }
    //  called on the server when the client finished the game.
    //[Command]
    public float cmd_OnServerClientFinishedGame(NetworkConnection conn)
    {
        customNetwork.CustomNetworkManager custMgr = manager as customNetwork.CustomNetworkManager;
        string finishedMsg;
        this.finishTimes[conn.connectionId] = Time.realtimeSinceStartup;
        float raceTime = this.finishTimes[conn.connectionId] - this.startTimes[conn.connectionId];
        Debug.LogWarning("cmd_OnServerClientFinishedGame(" + conn.connectionId.ToString() + "): finish t=" + raceTime.ToString());
        if (custMgr != null)
        {
            //string whoFinished = conn. conn.connectionId;
            string formattedRaceTime = string.Format(Hamster.StringConstants.FinishedTimeText, Hamster.Utilities.StringHelper.FormatTime((long)(raceTime*1000.0f)));

            finishedMsg = "(" + this.finishedClients.Count.ToString() + "/" + custMgr.client_connections.Count.ToString() + ") " + formattedRaceTime + " #" + conn.connectionId.ToString() +" " + custMgr.getPlayerName(conn.connectionId);
            custMgr.ServerShout(finishedMsg);
        }
        //playerFinishTimes[conn.connectionId] = raceTime;

        if (!this.finishTimes.ContainsKey(conn.connectionId))
        {
            Debug.LogError("finishTimes has no key for connId=" + conn.connectionId.ToString());
        }
        else if (!this.startTimes.ContainsKey(conn.connectionId))
        {
            Debug.LogError("startTimes has no key for connId=" + conn.connectionId.ToString());
        }
        else
        {
            raceTime = this.finishTimes[conn.connectionId] - this.startTimes[conn.connectionId];
            Debug.Log("s=" + this.startTimes[conn.connectionId].ToString() + " f=" + this.finishTimes[conn.connectionId].ToString() + ", raceTime=" + raceTime.ToString());
            server_SendRaceTime(conn.connectionId, raceTime);
        }
        return raceTime;
    }
    //  called on the client when the client finished the game.
    public void ClientFinishedGame(NetworkConnection conn)
    {
        //  add this connectionId as one of the finished clients.
        if (!finishedClients.ContainsKey(conn.connectionId))
            finishedClients[conn.connectionId] = conn;

        float raceTime = cmd_OnServerClientFinishedGame(conn);

        //  tell that client they finished!
        //  wait until we get 4 finished clients before we do something.
        if (finishedClients.Count >= kMaxPlayers)
        {
            ServerGameFinished.EnterState(conn.connectionId, raceTime);
        }
    }

    public void notifyServerClientStart(NetworkConnection conn)
    {
        Debug.LogWarning("MultiplayerGame.notifyServerClientStart: " + conn.ToString() + "\n");
        if (conn != null)
        {
            Cmd_clientStartedGame(conn);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (autoStartServer)
        {
            EnterServerStartupState(startingLevel);
        }
    }


    // Update is called once per frame
    void Update()
    {
        curState = clientStateManager.CurrentState().ToString();
        if (clientStateManager.CurrentState().ToString() == "Hamster.States.BaseState")  //  we haven't started anything yet.
        {
            if (manager != null && manager.isNetworkActive)
            {
                if (NetworkServer.active)
                {
                    //ServerListenForClients listenState = new ServerListenForClients();
                    //clientStateManager.PushState(listenState);
                }
            }
        }
        clientStateManager.Update();
        serverStateManager.Update();
    }
    // Pass through to allow states to have their own GUI.
    void OnGUI()
    {
        clientStateManager.OnGUI();
        serverStateManager.OnGUI();
    }

}// class MultiPlayerGame