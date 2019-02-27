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
public class MutiplayerGame : MonoBehaviour
{
    //  game constants
    const int kMaxPlayers = 4;

    //  config file for multiplayer game
    public const string kConfigJsonBasename = "MHConfig";
    public const string kConfigJson = kConfigJsonBasename + ".json";

    public bool autoStartServer;    //  if we want the server to automatically start. This is mostly for debugging because Unity doesn't play nice with Start() and Network scenes!
    public NetworkManager manager;  //  developer should fill this field in Unity inspector.
    public JsonStartupConfig config;
    public ConnectionConfig connConfig; //  deprecated

    public StateManager stateManager = new StateManager();    //  our statemachine that is separate from the MainGame state machine for single player.

    public int numPlayers = 1;  //  this is the number of players who can join. Autostart after this number is reached on the server
    public int defaultLevelIdx = 0;
    public int startingLevel;
    public float[] playerFinishTimes = new float[kMaxPlayers];  //  where we record the finish times of the players.
    public NetworkConnection[] networkConnections = new NetworkConnection[kMaxPlayers];

    string serverAddress;
    string serverPort;

    //  for debugging
    public string curState;
    public void ReadConfig()
    {
        MutiplayerGame mpgame = this;
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

    //  typical unity stuff below
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject); //  because NetworkManager has been set to DontDestroyOnLoad, it will be in a separate scene hierarchy/memory segment that cannot interact with this. Thus we must be in the same "zone" as the NetworkManager! Ugh, Unity!
        for (int ii = 0; ii < kMaxPlayers; ii++)
        {
            playerFinishTimes[ii] = -1.0f;  //  initialize to negative time.
        }
    }

    void EnterServerStartupState()
    {
        Hamster.States.ServerStartup state = new Hamster.States.ServerStartup();   //  create new state for FSM that will let us force the starting level.
        stateManager.ClearStack(state);    //  hack: Just slam that state in there disregarding all previous states! OMG!!!
    }

    public void OnClientConnect(NetworkConnection conn)
    {
        networkConnections[0] = conn;   //  we're on the client, so we have only ONE connection to the server.
        Hamster.States.ClientConnected state = new Hamster.States.ClientConnected();   //  create new state for FSM that will let us force the starting level.
        stateManager.PushState(state);
    }
    // Start is called before the first frame update
    void Start()
    {
        if (autoStartServer)
        {
            EnterServerStartupState();
        }
    }


    // Update is called once per frame
    void Update()
    {
        curState = stateManager.CurrentState().ToString();
        if (stateManager.CurrentState().ToString() == "Hamster.States.BaseState")  //  we haven't started anything yet.
        {
            if (manager != null & manager.isNetworkActive)
            {
                if (NetworkServer.active)
                {
                    ServerListenForClients listenState = new ServerListenForClients();
                    stateManager.PushState(listenState);
                }
            }
        }
        stateManager.Update();
    }
    // Pass through to allow states to have their own GUI.
    void OnGUI()
    {
        stateManager.OnGUI();
    }

}// class MultiPlayerGame