//  uncomment this if we want to experiment with Unity's matchmaking. But it's too broken for the later Unity versions.
//  #define OBSOLETE_2017_4
//#define TEST_ARGS
using System;
using System.ComponentModel;

using Hamster;


#if ENABLE_UNET

using UnityEngine.Rendering;

namespace UnityEngine.Networking
{
    [AddComponentMenu("Network/CustomNetworkManagerHUD")]
    [RequireComponent(typeof(NetworkManager))]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class CustomNetworkManagerHUD : MonoBehaviour
    {
        const int kDefaultLevelIdx = 0;//  defaults to load level 0

        public bool bShowDebugCurrentStateInfo = false; //  show the current finite state machine state
        public bool bShowDebugCmdlineArgs = false;  //  show the command line arguments.
        public bool skipLevelMenu = false;  //  skips the menu and starts server level right away
        public int kTextBoxHeight = 40;
        public int kTextBoxWidth = 1024;
        Vector2 lastTextPos;
        public int kSpaceBetweenBoxes = 5;
        public JsonStartupConfig config;
        public MultiplayerGame multiPlayerGame;

        int startLevel = kDefaultLevelIdx; 
        public NetworkManager manager;
        [SerializeField] public bool showGUI = true;
        [SerializeField] public int offsetX;
        [SerializeField] public int offsetY;

        //  to allow the client to connect to the server
        string serverAddress;
        string serverPort;

        // Runtime variable
        bool m_ShowServer;
        bool m_loadServerRequested = false;
        bool m_autoStartLevel = false;
        private OpenMatchClient openMatch;

        /*
         * We want to load the server as soon as we can, but still need to wait for varoius FSM states to initialize properly first.
         */
        void StartServerReq()   //  often times, we cannot load the server immediately. So, we make a request and let some things finish before we actually change states.
        {
            m_loadServerRequested = true;
            m_autoStartLevel = true;    //  this is some weird legacy logic.
        }

        //  this starts the level as soon as all of the other dependent pieces are ready to start the level. This means some pointers and such need to come online.
        //  This means this sits in Update() and gets called until it works. Ugly, but somewhat necessary as it's the simplest solution.
        //  this should really be named CheckStartLevel(), but changing the function name would make a mess of the version history.
        public bool AutoStartLevel(int levelIdx)
        {
            bool bSucceeded = false;
            if (m_autoStartLevel)
            {
                if (multiPlayerGame== null)
                {
                    GetMultiplayerPointer();
                }
                if (multiPlayerGame != null)
                {
                    m_autoStartLevel = false;   //  we gotta stop calling this over and over.
                    multiPlayerGame.EnterServerStartupState(startLevel);  //  use this now instead of manager.StartServer()
                    bSucceeded = true;

                    //  we no longer do agones.Ready() here because it should be done in the FSM started by ServerStartupState above.
                    //  see Hamster.States.ServerStartup.Initialize() or ServerLoadingLevel.GentleLoadLevel() for where this should happen now.
                    //// If we're running through Agones, signal ready after the level has loaded
                    //if (agones != null) {
                    //    agones.Ready();
                    //}
                }
            }
            return bSucceeded;

        }

        //  all the various StartServer type of functions should do this.
        void StartServerCommon()
        {
            if (manager == null)
            {
                manager = GetComponent<NetworkManager>();
            }
            if (m_autoStartLevel)
                m_loadServerRequested = !AutoStartLevel(startLevel);
        }

        void ReadConfig()
        {
            config = FindObjectOfType<JsonStartupConfig>();

            if (config != null)
            {
                if (!config.isConfigLoaded)
                {  //  strange, this should have already been loaded. But Unity timing for Start is weird, so we'll just load it anyway.
                    config.ReadJsonStartupConfig();
                }
                this.startLevel = config.startupConfig.startingLevel;
                this.serverAddress = config.startupConfig.serverIP;
                this.serverPort = config.startupConfig.serverPort;
            }
        }
        bool ReadCommandLineArg()
        {

            if (manager==null)
                manager = GetComponent<NetworkManager>();

            string[] args = System.Environment.GetCommandLineArgs();
            string input = "";
            int intArg = -1;
            Debug.Log("ReadCommandLineArg");

            for (int i = 0; i < args.Length; i++)
            {
                input = "";
                Debug.Log("ARG " + i + ": " + args[i]);
                if (i + 1 < args.Length)
                    input = args[i + 1];

                string lowCaseInput = args[i].ToLower();
                switch (lowCaseInput)
                {
                    case "-c":
                        Debug.Log("Start Client");
                        //  placeholder for when we want to start the client on a particular level from command line. Harder than it looks.
                        break;
                    case "-s":
                        Debug.Log("Start Server");
                        StartServerReq();
                        break;
                    case "-a":
                        Debug.Log("Communicating with Agones");

                        MultiplayerGame.instance.agones = GetComponent<AgonesClient>();
                        break;
                    case "-level":
                        if (Int32.TryParse(input, out intArg))
                        {
                            startLevel = intArg;
                        }
                        break;

                }
            }
#if TEST_ARGS
            bool bTestArgs = true;
            if (bTestArgs)
            {
                Debug.Log("Start Server");
                if (multiPlayerGame==null)
                {
                    GetMultiplayerPointer();
                }
                if (multiPlayerGame != null)
                {
                    multiPlayerGame.EnterServerStartupState(kDefaultLevelIdx);  //  use this now instead of manager.StartServer()
                                                                          //bServerStarted = manager.StartServer();  //  separated because you can start a host which will also need StartServerReq() afterwards.
                    bServerStarted = true;
                    //StartServerReq();
                    //m_autoStartLevel = true;
                }
            }
#endif
            return false;   //  we don't want to start the server here because StartServerReq() will start it later.
        }

        void GetMultiplayerPointer()
        {
            if (multiPlayerGame==null)
                multiPlayerGame = UnityEngine.GameObject.FindObjectOfType<MultiplayerGame>();
        }

        void Awake()
        {
        }
        private void Start()
        {
            if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.Null)   //  if we have graphics, we can set the resolution.
            {
                Screen.SetResolution(1280, 1024, false);
            }

            if (manager != null)
            {
                serverAddress = manager.networkAddress;
                serverPort = manager.networkPort.ToString();
                GetMultiplayerPointer();
                if (multiPlayerGame != null)
                {
                    multiPlayerGame.manager = manager;
                }
            }

            // Pull in the component if it's been added. If it hasn't the menu for Open Match won't appear
            // so this should be fine to do.
            if (openMatch == null)
            {
                openMatch = GetComponent<OpenMatchClient>();
            }

            ReadConfig();
            //  
            //  command line args take precedence over the .json config file because someone had to type it intentionally.
            bool bServerStarted = ReadCommandLineArg();

            //  Note to Graeme: This stuff was moved to Start() from Awake() because we need some cycles for the server stuff to get online with valid manager fields to call StartServer(). It's a Unity thing.
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
            {
                Debug.LogFormat("Starting headless server @ {0}:{1}", manager.networkAddress.ToString(), manager.networkPort.ToString());
                if (!bServerStarted)
                {
                    //if (manager.StartServer())    //  we no longer call this anymore, but instead let the FSM handle it. However, we still need to let Unity finish all of its Start() calls, so we can't start right away and need to wait until our Update() loop to actually call it so that we are certain that all Start() calls have been executed.
                    StartServerReq();   //  so we must make a delayed call to start the FSM-state that will start the server.
                }
            }
        }

        void Update()
        {
            if (m_loadServerRequested)
            {
                StartServerCommon();
            }
            if (Input.GetKeyDown(KeyCode.Tilde) || Input.GetKeyDown(KeyCode.BackQuote))
            {
                showGUI = !showGUI; //  toggle GUI.
            }

            if (!showGUI)
            {
                return;
            }

            if (!manager.IsClientConnected() && !NetworkServer.active && manager.matchMaker == null)
            {
                if (UnityEngine.Application.platform != RuntimePlatform.WebGLPlayer)
                {
                    if (Input.GetKeyDown(KeyCode.S))
                    {
//                        if (manager.StartServer())
                            StartServerReq();
                    }
                    if (Input.GetKeyDown(KeyCode.H))
                    {
                        manager.StartHost();
                        StartServerReq();
                    }
                }
                if (Input.GetKeyDown(KeyCode.C))
                {
                    manager.StartClient();
                }
            }
            if (NetworkServer.active || NetworkClient.active)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    manager.StopHost();
                }
                if (NetworkClient.active)
                {
                    if (Input.GetKeyDown(KeyCode.Insert))
                    {
                        CreateNetworkPlayer();
                    }
                    if (Input.GetKeyDown(KeyCode.Delete))
                    {
                        DestroyNetworkPlayer();
                    }
                }
            }
            else
            {   //  back button on android quits the app if we're not connected.
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    Hamster.MainGame.QuitGame();
                }
            }
        }
        void DestroyNetworkPlayer()
        {
            if (ClientScene.localPlayers.Count <= 0) return;    //  bail. nobody to destroy.

            //  note: Unity doesn't seem to keep track of their array of localPlayers very precisely. Thus, we must keep track of the players we've added on our own! We cannot rely on their structures to be correct.
            short plrControllerID = (short)(ClientScene.localPlayers.Count - 1);    //  destroy the last localPlayer.
            short unetPlrControllerId = (short)(ClientScene.localPlayers[plrControllerID].unetView.playerControllerId); //  not sure if this is correct. Does Unity keep track of the localPlayer's controllerIDs properly? Must try to find out. Note that it doesn't keep track of the localPlayers array properly. I do that for Unity. But I don't keep track of the unet pointer, so that's up to Unity's code which is unreliable.
            Debug.LogWarning("CustomNetworkManagerHUD.DestroyNetworkPlayer: " + plrControllerID.ToString() +  ", unetPlrctrlId=" + unetPlrControllerId.ToString() + "\n");

            customNetwork.CustomNetworkPlayer.DestroyPlayer((short)(unetPlrControllerId));  //  do the thing we came here for.
        }
        void CreateNetworkPlayer()
        {
            customNetwork.CustomNetworkPlayer.CreatePlayerClient((short)ClientScene.localPlayers.Count);
        }

        string scaledTextField(out float newYpos, float xpos, float ypos, float w, float h, string tField)
        {
            float unusedXPos;
            string result = scaledTextField(out newYpos, out unusedXPos, xpos, ypos, tField);   //  call the other one.
            return result;
        }

        string scaledTextField(out float newYpos, out float newXPos, float xpos, float ypos, string tField)
        {
            const float kMinWidth = 100.0f; //  if we have no text, our box becomes too small to click.
            const float kButtonSpace = 6.0f;
            int spacing = kTextBoxHeight + kSpaceBetweenBoxes;
            float screenHeightScaling = 1.0f;// Screen.currentResolution.height / 1024.0f;
            int kFontSize = (int)((kTextBoxHeight) * screenHeightScaling);
            GUIStyle textFieldStyle = new GUIStyle(GUI.skin.textField);
            textFieldStyle.fontSize = kFontSize;
            textFieldStyle.alignment = TextAnchor.MiddleCenter;// TextAlignment.Center;

            GUIContent content = new GUIContent(GUIContent.none);
            content.text = tField;

            Vector2 rectSize = textFieldStyle.CalcSize(content);
            float buttonWidth = Math.Max(kMinWidth, rectSize.x + kButtonSpace);
            Rect tempRect = new Rect(xpos, ypos- kButtonSpace / 2, buttonWidth, rectSize.y + kButtonSpace);
            float space = tempRect.height;

            // Set the internal name of the textfield
            GUI.SetNextControlName("MyTextField");

            tField = GUI.TextField(tempRect, tField, textFieldStyle);

            newYpos = ypos + space + kSpaceBetweenBoxes;
            newXPos = xpos + tempRect.width + kSpaceBetweenBoxes;
            lastTextPos.x = newXPos;
            lastTextPos.y = newYpos;
            return tField;
        }
        bool scaledButton(out float newYpos, float xpos, float ypos, float w, float h, string buttonText)
        {
            const float kButtonSpace = 1.5f;
            int spacing = kTextBoxHeight + kSpaceBetweenBoxes;
            float screenHeightScaling = 1.0f;// Screen.currentResolution.height / 1024.0f;
            int kFontSize = (int)((kTextBoxHeight) * screenHeightScaling);
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = kFontSize;
            GUIContent content = new GUIContent(GUIContent.none);
            content.text = buttonText;

            Vector2 rectSize = GUI.skin.button.CalcSize(content);
            Rect tempRect = new Rect(xpos, ypos, rectSize.x+ kButtonSpace, rectSize.y+ kButtonSpace);
            bool bButton = GUI.Button(tempRect, buttonText, buttonStyle);
            float space = tempRect.height;
            newYpos = ypos + space + kSpaceBetweenBoxes;
            lastTextPos.x = xpos;
            lastTextPos.y = newYpos;

            return bButton;
        }

        public float scaledTextBox(string txt)
        {
            float xpos = lastTextPos.x;
            float ypos = lastTextPos.y;
            ypos = scaledTextBox(xpos, ypos, txt);
            return ypos;
        }

        public float scaledTextBox(float xpos, float ypos, float w, float h, string txt)
        {
            return scaledTextBox(xpos, ypos, txt);
        }
        public float scaledTextBox(float xpos, float ypos, string txt)
        {
            float screenHeightScaling = 1.0f;// Screen.currentResolution.height / 1024.0f;
            int kFontSize = (int)((kTextBoxHeight) * screenHeightScaling);
            GUIStyle textAreaStyle = new GUIStyle(GUI.skin.label);
            textAreaStyle.fontSize = kFontSize;
            GUIContent content = new GUIContent(GUIContent.none);
            content.text = txt;


            Vector2 rectSize = textAreaStyle.CalcSize(content);
            Rect tempRect = new Rect(xpos, ypos, rectSize.x, rectSize.y);
            GUI.Label(tempRect, txt, textAreaStyle);

            float space = tempRect.height;
            ypos += space;
            lastTextPos.x = xpos;
            lastTextPos.y = ypos;

            return ypos;
        }

        float ShowCommandLineArgs(float xpos, float ypos)
        {
            string[] args = System.Environment.GetCommandLineArgs();
            string input = "";
            int intArg;
            Debug.Log("ReadCommandLineArg");
            string st;

            for (int i = 0; i < args.Length; i++)
            {
                st = " ARG " + i + ": " + args[i];
                ypos = scaledTextBox(xpos, ypos, st);
            }
            return ypos;
        }
            void OnGUI()
        {
            if (!showGUI)
            {
                OperatingSystemFamily fam = SystemInfo.operatingSystemFamily;
                if (fam == OperatingSystemFamily.Windows || fam == OperatingSystemFamily.Linux || fam == OperatingSystemFamily.MacOSX)
                {
                    scaledTextBox(0, 0, "Show GUI: Press tilde ~ or `");
                }
                return;
            }


            int spacing = kTextBoxHeight + kSpaceBetweenBoxes;
            float screenHeightScaling = 1.0f;// Screen.currentResolution.height / 1024.0f;
            int kFontSize = (int)((kTextBoxHeight) * screenHeightScaling);

            //  string ipv4 = manager.networkAddress;   //  this usually just says "localhost" which doesn't really help us type in an ip address later.
            string localipv4 = customNetwork.CustomNetworkManager.LocalIPAddress(); //  this is this machine's address.
            int port = manager.networkPort;

            float xpos = offsetX;
            float ypos = offsetY;
            float newYpos = 0;

            bool noConnection = (manager.client == null || manager.client.connection == null ||
                                 manager.client.connection.connectionId == -1);

            //  you can find the version number in Build Settings->PlayerSettings->Version
            string serverVersionMsg = "";
            customNetwork.CustomNetworkManager custmgr = manager as customNetwork.CustomNetworkManager;
            if (custmgr != null)
            {
                Hamster.CommonData.networkmanager = custmgr;    //  there's probably a better place to put this.
                string sv;
                if (!custmgr.isServerAndClientVersionMatch(out sv))
                {
                    serverVersionMsg = "MISMATCH ServerV=" + sv;
                    string serverVerFloat = customNetwork.CustomNetworkManager.getStrippedVersionNumber(sv);
                    string cv = Application.version;    //  client version is THIS machine!
                    string clientVerFloat = customNetwork.CustomNetworkManager.getStrippedVersionNumber(cv);
                    double serverVersion = Convert.ToDouble(serverVerFloat);
                    double clientVersion = Convert.ToDouble(clientVerFloat);
                    if (clientVersion > serverVersion)
                    {
                        ypos = scaledTextBox(xpos, ypos, "client=" + cv + ">server=" + sv);
                    }
                    else if (clientVersion > serverVersion)
                    {
                        ypos = scaledTextBox(xpos, ypos, "client=" + cv + "<server=" + sv);
                    }

                }
                else
                {
                    customNetwork.CustomNetworkManager custMgr = this.manager as customNetwork.CustomNetworkManager;
                    if (custMgr && custMgr.bIsServer)
                    {
                        string serverVerFloat = customNetwork.CustomNetworkManager.getStrippedVersionNumber(sv);
                        serverVersionMsg = "serverV=" + sv;
                    }
                }

            }
            ypos = scaledTextBox(xpos, ypos, "clientV=" + Application.version + ", " + serverVersionMsg);

            if (bShowDebugCmdlineArgs)
                ypos = ShowCommandLineArgs(xpos, ypos);
            string curState;
            if (bShowDebugCurrentStateInfo )
            {
                if (Hamster.CommonData.mainGame != null && Hamster.CommonData.mainGame.stateManager != null)
                {
                    if (this.multiPlayerGame != null)
                    {
                        string multiplayerState = this.multiPlayerGame.clientStateManager.CurrentState().GetType().ToString();
                        ypos = scaledTextBox(xpos, ypos, "client=" + multiplayerState);
                        multiplayerState = this.multiPlayerGame.serverStateManager.CurrentState().GetType().ToString();
                        ypos = scaledTextBox(xpos, ypos, "server=" + multiplayerState);
                    }
                    else
                    {
                        GetMultiplayerPointer();
                    }

                    curState = Hamster.CommonData.mainGame.stateManager.CurrentState().GetType().ToString();
                    ypos = scaledTextBox(xpos, ypos, "curState=" + curState);
                }
                if (Hamster.CommonData.gameWorld != null)
                    ypos = scaledTextBox(xpos, ypos, "curLevelIdx=" + Hamster.CommonData.gameWorld.curLevelIdx.ToString());
            }
            //  for debugging. Don't waste space showing screen res.
            //  ypos = scaledTextBox(xpos, ypos, kTextBoxWidth, kTextBoxHeight, "Res=" + Screen.width.ToString() + "x" + Screen.height.ToString());

            GUI.skin.button.fontSize = (int)kFontSize;

            if (!manager.IsClientConnected() && !NetworkServer.active && manager.matchMaker == null)
            {
                if (noConnection)
                {
                    if (UnityEngine.Application.platform != RuntimePlatform.WebGLPlayer)
                    {
                        //if (scaledButton(out newYpos, xpos, ypos, 200, kTextBoxHeight), "LAN Host(H)"))
                        if (scaledButton(out newYpos, xpos, ypos, 200, kTextBoxHeight, "LAN (H)ost"))
                        {
                            manager.StartHost();
                        }
                        ypos = newYpos;
                    }

                    if (scaledButton(out newYpos, xpos, ypos, 105, kTextBoxHeight, "LAN (C)lient"))
                    {
                        manager.StartClient();
                    }
                    //  ypos = newYpos;
                    float offsetXPos = xpos;

                    manager.networkAddress = serverAddress = scaledTextField(out newYpos, out offsetXPos, offsetXPos+250, ypos, serverAddress);
                    serverPort = scaledTextField(out newYpos, out offsetXPos, offsetXPos, ypos, serverPort);
                    manager.networkPort = Convert.ToInt32(serverPort);
                    ypos = newYpos;

                    if (UnityEngine.Application.platform == RuntimePlatform.WebGLPlayer)
                    {
                        // cant be a server in webgl build
                        GUI.Box(new Rect(xpos, ypos, kTextBoxWidth, kTextBoxHeight), "(  WebGL cannot be server  )");
                        ypos += spacing;
                    }
                    else
                    {
                        if (scaledButton(out newYpos, xpos, ypos, kTextBoxWidth, kTextBoxHeight, "LAN (S)erver Only"))
                        {
                            if (!skipLevelMenu)
                                startLevel = -1;    //  allow the player to choose the level
                            StartServerReq();
                        }
                        ypos = newYpos;
                    }
                }
                else
                {
                    ypos = scaledTextBox(xpos, ypos, kTextBoxWidth, kTextBoxHeight/2, "Connecting to\n  " + manager.networkAddress + ":" + manager.networkPort + "..");


                    if (scaledButton(out newYpos, xpos, ypos, kTextBoxWidth, kTextBoxHeight, "Cancel Conn.Req."))
                    {
                        manager.StopClient();
                    }
                    ypos = newYpos;
                }
            }
            else
            {

                if (NetworkServer.active)
                {
                    string serverMsg = "Server(" + customNetwork.CustomNetworkManager.LocalHostname() + "): " + localipv4 + "\n  port=" + port.ToString();
                    if (manager.useWebSockets)
                    {
                        serverMsg += " (Using WebSockets)";
                    }
                    ypos = scaledTextBox(xpos, ypos, kTextBoxWidth, kTextBoxHeight, serverMsg);
                }
                if (manager.IsClientConnected())
                {
                    ypos = scaledTextBox(xpos, ypos, kTextBoxWidth, kTextBoxHeight, "Client(" + customNetwork.CustomNetworkManager.LocalHostname() + ")=" + localipv4 + "\n  port=" + port.ToString());
                }
                ypos = scaledTextBox(xpos, ypos, kTextBoxWidth, kTextBoxHeight, "client.active=" + NetworkClient.active.ToString() + ", server.active=" + NetworkServer.active.ToString());
            }

            if (manager.IsClientConnected() && !ClientScene.ready)
            {
                if (scaledButton(out newYpos, xpos, ypos, kTextBoxWidth, kTextBoxHeight, "Client Ready"))
                {
                    ClientScene.Ready(manager.client.connection);
                }
                ypos = newYpos;
            }


            if (manager.IsClientConnected() && ClientScene.ready)
            {
                //  warning: adding extra players will mess up some logic that relies on one player per client, such as the OpenMatch criteria. however, it is left here to debug.
                if (scaledButton(out newYpos, xpos, ypos, kTextBoxWidth, kTextBoxHeight, "Add player: (Ins)" + ClientScene.localPlayers.Count.ToString()))
                {
                    CreateNetworkPlayer();
                }
                ypos = newYpos;
                if (scaledButton(out newYpos, xpos, ypos, kTextBoxWidth, kTextBoxHeight, "Del player: (Del)" + ClientScene.localPlayers.Count.ToString()))
                {
                    DestroyNetworkPlayer();
                }
                ypos = newYpos;
            }

            if (NetworkServer.active || manager.IsClientConnected())
            {
                string stopButtonText = "Stop (Esc)";
                if (NetworkServer.connections.Count == 0 && NetworkServer.active)
                {
                    stopButtonText = "Stop server(Esc)"; 
                }
                if (scaledButton(out newYpos, xpos, ypos, kTextBoxWidth, kTextBoxHeight, stopButtonText))
                {
                    manager.StopHost();

                    if (openMatch != null)
                    {
                        openMatch.Disconnect();
                    }
                }
                ypos = newYpos;
            }

            if (!NetworkServer.active && !manager.IsClientConnected() && noConnection)
            {
                ypos += 10;

                if (UnityEngine.Application.platform == RuntimePlatform.WebGLPlayer)
                {
                    GUI.Box(new Rect(xpos - 5, ypos, 220, kTextBoxHeight), "(WebGL cannot use Match Maker)");
                    return;
                }

                if (manager.matchMaker == null)
                {
                    if (scaledButton(out newYpos, xpos, ypos, kTextBoxWidth, kTextBoxHeight, "Enable Match Maker (M)"))
                    {
                        manager.StartMatchMaker();
                    }
                    ypos = newYpos;
                }
                else
                {
                    if (manager.matchInfo == null)
                    {
                        if (manager.matches == null)
                        {
                            if (scaledButton(out newYpos, xpos, ypos, kTextBoxWidth, kTextBoxHeight, "Create Internet Match"))
                            {
#if OBSOLETE_2017_4
                                manager.matchMaker.CreateMatch(manager.matchName, manager.matchSize, true, "", manager.OnMatchCreate);
#endif
                            }
                            ypos = newYpos;

                            //  don't accept the new ypos because we want the name to be on the same line.
                            //  ypos = 
                            scaledTextBox(xpos, ypos, kTextBoxWidth, 20, "Room Name:");

                            manager.matchName = scaledTextField(out newYpos, xpos + 300, ypos, 100, kTextBoxHeight, manager.matchName);
                            ypos = newYpos;

                            if (scaledButton(out newYpos, xpos, ypos, 200, kTextBoxHeight, "Find Internet Match"))
                            {
#if OBSOLETE_2017_4
                                manager.matchMaker.ListMatches(0, 20, "", manager.OnMatchList);
#endif
                            }
                            ypos = newYpos;
                        }
                        else
                        {
                            foreach (var match in manager.matches)
                            {
                                if (scaledButton(out newYpos, xpos, ypos, kTextBoxWidth, kTextBoxHeight, "Join Match:" + match.name))
                                {
#if OBSOLETE_2017_4
                                    manager.matchName = match.name;
                                    manager.matchSize = (uint)match.currentSize;
                                    manager.matchMaker.JoinMatch(match.networkId, "", manager.OnMatchJoined);
#endif
                                }
                                ypos = newYpos;
                            }
                        }
                    }

                    if (scaledButton(out newYpos, xpos, ypos, kTextBoxWidth, kTextBoxHeight, "Change MM server"))
                    {
                        m_ShowServer = !m_ShowServer;
                    }
                    ypos = newYpos;
                    if (m_ShowServer)
                    {
                        if (scaledButton(out newYpos, xpos, ypos, kTextBoxWidth, kTextBoxHeight, "Local"))
                        {
                            manager.SetMatchHost("localhost", 1337, false);
                            m_ShowServer = false;
                        }
                        ypos = newYpos;
                        if (scaledButton(out newYpos, xpos, ypos, kTextBoxWidth, kTextBoxHeight, "Internet"))
                        {
                            manager.SetMatchHost("mm.unet.unity3d.com", 443, true);
                            m_ShowServer = false;
                        }
                        ypos = newYpos;
                        if (scaledButton(out newYpos, xpos, ypos, kTextBoxWidth, kTextBoxHeight, "Staging"))
                        {
                            manager.SetMatchHost("staging-mm.unet.unity3d.com", 443, true);
                            m_ShowServer = false;
                        }
                        ypos = newYpos;
                    }

                    ypos += spacing;

                    GUI.Label(new Rect(xpos, ypos, kTextBoxWidth, kTextBoxHeight), "MM Uri: " + manager.matchMaker.baseUri);
                    ypos += spacing;

                    if (scaledButton(out newYpos, xpos, ypos, kTextBoxWidth, kTextBoxHeight, "Disable Match Maker"))
                    {
                        manager.StopMatchMaker();
                    }
                    ypos += spacing;
                }
            }
        }
    }
}
#endif //ENABLE_UNET
