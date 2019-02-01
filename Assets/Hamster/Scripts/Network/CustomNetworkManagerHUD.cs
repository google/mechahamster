using System;
using System.ComponentModel;

#if ENABLE_UNET

using UnityEngine.Rendering;

namespace UnityEngine.Networking
{
    [AddComponentMenu("Network/CustomNetworkManagerHUD")]
    [RequireComponent(typeof(NetworkManager))]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class CustomNetworkManagerHUD : MonoBehaviour
    {
        public int kTextBoxHeight = 40;
        public int kTextBoxWidth = 1024;

        public NetworkManager manager;
        [SerializeField] public bool showGUI = true;
        [SerializeField] public int offsetX;
        [SerializeField] public int offsetY;

        // Runtime variable
        bool m_ShowServer;

        void Awake()
        {
            manager = GetComponent<NetworkManager>();
            
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
            {
                Debug.LogFormat("Starting headless server @ {0}:{1}", manager.networkAddress.ToString(), manager.networkPort.ToString());
                manager.StartServer();
            }
        }

        void Update()
        {
            if (!showGUI)
                return;

            if (!manager.IsClientConnected() && !NetworkServer.active && manager.matchMaker == null)
            {
                if (UnityEngine.Application.platform != RuntimePlatform.WebGLPlayer)
                {
                    if (Input.GetKeyDown(KeyCode.S))
                    {
                        manager.StartServer();
                    }
                    if (Input.GetKeyDown(KeyCode.H))
                    {
                        manager.StartHost();
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
                    Application.Quit();
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
        const int kSpaceBetweenBoxes = 2;

        string scaledTextField(out float newYpos, float xpos, float ypos, float w, float h, string tField)
        {
            const float kButtonSpace = 1.5f;
            int spacing = kTextBoxHeight + kSpaceBetweenBoxes;
            int kFontSize = kTextBoxHeight / 2;
            GUIStyle textFieldStyle = new GUIStyle(GUI.skin.textField);
            textFieldStyle.fontSize = kFontSize;
            GUIContent content = new GUIContent(GUIContent.none);
            content.text = tField;

            Vector2 rectSize = textFieldStyle.CalcSize(content);
            Rect tempRect = new Rect(xpos, ypos, rectSize.x + kButtonSpace, rectSize.y + kButtonSpace);
            float space = tempRect.height;
            tField = GUI.TextField(tempRect, tField, textFieldStyle);
            newYpos = ypos + space + kSpaceBetweenBoxes;
            return tField;
        }
        bool scaledButton(out float newYpos, float xpos, float ypos, float w, float h, string buttonText)
        {
            const float kButtonSpace = 1.5f;
            int spacing = kTextBoxHeight + kSpaceBetweenBoxes;
            int kFontSize = kTextBoxHeight / 2;
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = kFontSize;
            GUIContent content = new GUIContent(GUIContent.none);
            content.text = buttonText;

            Vector2 rectSize = GUI.skin.button.CalcSize(content);
            Rect tempRect = new Rect(xpos, ypos, rectSize.x+ kButtonSpace, rectSize.y+ kButtonSpace);
            bool bButton = GUI.Button(tempRect, buttonText, buttonStyle);
            float space = tempRect.height;
            newYpos = ypos + space + kSpaceBetweenBoxes;
            return bButton;
        }
        float scaledTextBox(float xpos, float ypos, float w, float h, string txt)
        {
            int spacing = kTextBoxHeight + kSpaceBetweenBoxes;
            int kFontSize = kTextBoxHeight/2;
            GUIStyle textAreaStyle = new GUIStyle(GUI.skin.label);
            textAreaStyle.fontSize = kFontSize;
            GUIContent content = new GUIContent(GUIContent.none);
            content.text = txt;


            Vector2 rectSize = textAreaStyle.CalcSize(content);
            Rect tempRect = new Rect(xpos, ypos, rectSize.x, rectSize.y);
            GUI.Label(tempRect, txt, textAreaStyle);

            float space = tempRect.height;
            ypos += space + kSpaceBetweenBoxes;
            return ypos;
        }

        private void Start()
        {
            Screen.SetResolution(1280, 1024, false);
        }
        void OnGUI()
        {

            if (!showGUI)
                return;

            int spacing = kTextBoxHeight + kSpaceBetweenBoxes;
            int kFontSize = kTextBoxHeight/2;

            string ipv4 = manager.networkAddress;   //  this usually just says "localhost" which doesn't really help us type in an ip address later.
            ipv4 = customNetwork.CustomNetworkManager.LocalIPAddress();    //  none of these work.

            float xpos = 10 + offsetX;
            float ypos = 40 + offsetY;
            float newYpos = 0;

            bool noConnection = (manager.client == null || manager.client.connection == null ||
                                 manager.client.connection.connectionId == -1);

            //  you can find the version number in Build Settings->PlayerSettings->Version
            ypos = scaledTextBox(xpos, ypos, kTextBoxWidth, kTextBoxHeight, "Version=" + Application.version);

            //  for debugging. Don't waste space showing screen res.
            //  ypos = scaledTextBox(xpos, ypos, kTextBoxWidth, kTextBoxHeight, "Res=" + Screen.width.ToString() + "x" + Screen.height.ToString());

            GUI.skin.button.fontSize = kFontSize;

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

                    manager.networkAddress = scaledTextField(out newYpos, xpos+150, ypos, kTextBoxWidth, kTextBoxHeight, manager.networkAddress);
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
                            manager.StartServer();
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
                    string serverMsg = "Server("+ customNetwork.CustomNetworkManager.LocalHostname() + "): "+ ipv4 + "\n  port=" + manager.networkPort;
                    if (manager.useWebSockets)
                    {
                        serverMsg += " (Using WebSockets)";
                    }
                    ypos = scaledTextBox(xpos, ypos, kTextBoxWidth, kTextBoxHeight, serverMsg);
                }
                if (manager.IsClientConnected())
                {
                    ypos = scaledTextBox(xpos, ypos, kTextBoxWidth, kTextBoxHeight, "Client(" + customNetwork.CustomNetworkManager.LocalHostname() + ")=" + ipv4 + "\n  port=" + manager.networkPort);
                }
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
                                //manager.matchMaker.CreateMatch(manager.matchName, manager.matchSize, true, "", manager.OnMatchCreate);
                            }
                            ypos = newYpos;

                            GUI.Label(new Rect(xpos, ypos, kTextBoxWidth, 20), "Room Name:");
                            manager.matchName = GUI.TextField(new Rect(xpos + 100, ypos, 100, kTextBoxHeight), manager.matchName);
                            ypos += spacing;

                            ypos += spacing;

                            if (scaledButton(out newYpos, xpos, ypos, 200, kTextBoxHeight, "Find Internet Match"))
                            {
                                //manager.matchMaker.ListMatches(0, 20, "", manager.OnMatchList);
                            }
                            ypos = newYpos;
                        }
                        else
                        {
                            foreach (var match in manager.matches)
                            {
                                if (scaledButton(out newYpos, xpos, ypos, kTextBoxWidth, kTextBoxHeight, "Join Match:" + match.name))
                                {
                                    manager.matchName = match.name;
                                    manager.matchSize = (uint)match.currentSize;
                                    //manager.matchMaker.JoinMatch(match.networkId, "", manager.OnMatchJoined);
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