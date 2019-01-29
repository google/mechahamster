using System;
using System.ComponentModel;

#if ENABLE_UNET
namespace UnityEngine.Networking
{
    [AddComponentMenu("Network/CustomNetworkManagerHUD")]
    [RequireComponent(typeof(NetworkManager))]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class CustomNetworkManagerHUD : MonoBehaviour
    {
        public NetworkManager manager;
        [SerializeField] public bool showGUI = true;
        [SerializeField] public int offsetX;
        [SerializeField] public int offsetY;

        // Runtime variable
        bool m_ShowServer;

        void Awake()
        {
            manager = GetComponent<NetworkManager>();
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
        void OnGUI()
        {
            const int kSpaceBetweenBoxes = 5;
            const int kTextBoxHeight = 35;
            const int kTextBoxWidth = 130;

            if (!showGUI)
                return;

            string ipv4 = manager.networkAddress;   //  this usually just says "localhost" which doesn't really help us type in an ip address later.
            ipv4 = customNetwork.CustomNetworkManager.LocalIPAddress();    //  none of these work.

            int xpos = 10 + offsetX;
            int ypos = 40 + offsetY;

            int spacing = kTextBoxHeight + kSpaceBetweenBoxes;
            bool noConnection = (manager.client == null || manager.client.connection == null ||
                                 manager.client.connection.connectionId == -1);

            if (!manager.IsClientConnected() && !NetworkServer.active && manager.matchMaker == null)
            {
                if (noConnection)
                {
                    if (UnityEngine.Application.platform != RuntimePlatform.WebGLPlayer)
                    {
                        if (GUI.Button(new Rect(xpos, ypos, 200, kTextBoxHeight), "LAN Host(H)"))
                        {
                            manager.StartHost();
                        }
                        ypos += spacing;
                    }

                    if (GUI.Button(new Rect(xpos, ypos, 105, kTextBoxHeight), "LAN Client(C)"))
                    {
                        manager.StartClient();
                    }

                    manager.networkAddress = GUI.TextField(new Rect(xpos + 100, ypos, kTextBoxWidth, kTextBoxHeight), manager.networkAddress);
                    ypos += spacing;

                    if (UnityEngine.Application.platform == RuntimePlatform.WebGLPlayer)
                    {
                        // cant be a server in webgl build
                        GUI.Box(new Rect(xpos, ypos, kTextBoxWidth, kTextBoxHeight), "(  WebGL cannot be server  )");
                        ypos += spacing;
                    }
                    else
                    {
                        if (GUI.Button(new Rect(xpos, ypos, kTextBoxWidth, kTextBoxHeight), "LAN Server Only(S)"))
                        {
                            manager.StartServer();
                        }
                        ypos += spacing;
                    }
                }
                else
                {
                    GUI.Label(new Rect(xpos, ypos, kTextBoxWidth, kTextBoxHeight/2), "Connecting to " + manager.networkAddress + ":" + manager.networkPort + "..");
                    ypos += spacing;


                    if (GUI.Button(new Rect(xpos, ypos, kTextBoxWidth, kTextBoxHeight), "Cancel Conn.Req."))
                    {
                        manager.StopClient();
                    }
                }
            }
            else
            {
                if (NetworkServer.active)
                {
                    string serverMsg = "Server ("+ customNetwork.CustomNetworkManager.LocalHostname() + "): "+ ipv4 + "\nport = " + manager.networkPort;
                    if (manager.useWebSockets)
                    {
                        serverMsg += " (Using WebSockets)";
                    }
                    GUI.Label(new Rect(xpos, ypos, kTextBoxWidth, kTextBoxHeight), serverMsg);
                    ypos += spacing;
                }
                if (manager.IsClientConnected())
                {
                    GUI.Label(new Rect(xpos, ypos, kTextBoxWidth, kTextBoxHeight), "Client: address=" + manager.networkAddress + " port=" + manager.networkPort);
                    ypos += spacing;
                }
            }

            if (manager.IsClientConnected() && !ClientScene.ready)
            {
                if (GUI.Button(new Rect(xpos, ypos, kTextBoxWidth, kTextBoxHeight), "Client Ready"))
                {
                    ClientScene.Ready(manager.client.connection);
                }
                ypos += spacing;
            }


            if (manager.IsClientConnected() && ClientScene.ready)
            {
                if (GUI.Button(new Rect(xpos, ypos, kTextBoxWidth, kTextBoxHeight), "Add player: (Ins)" + ClientScene.localPlayers.Count.ToString()))
                {
                    CreateNetworkPlayer();
                }
                ypos += spacing;
                if (GUI.Button(new Rect(xpos, ypos, kTextBoxWidth, kTextBoxHeight), "Del player: (Del)" + ClientScene.localPlayers.Count.ToString()))
                {
                    DestroyNetworkPlayer();
                }
                ypos += spacing;
            }

            if (NetworkServer.active || manager.IsClientConnected())
            {
                string stopButtonText = "Stop (Esc)";
                if (NetworkServer.connections.Count == 0 && NetworkServer.active)
                {
                    stopButtonText = "Stop server(Esc)"; 
                }
                if (GUI.Button(new Rect(xpos, ypos, kTextBoxWidth, kTextBoxHeight), stopButtonText))
                {
                    manager.StopHost();
                }
                ypos += spacing;
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
                    if (GUI.Button(new Rect(xpos, ypos, kTextBoxWidth, kTextBoxHeight), "Enable Match Maker (M)"))
                    {
                        manager.StartMatchMaker();
                    }
                    ypos += spacing;
                }
                else
                {
                    if (manager.matchInfo == null)
                    {
                        if (manager.matches == null)
                        {
                            if (GUI.Button(new Rect(xpos, ypos, kTextBoxWidth, kTextBoxHeight), "Create Internet Match"))
                            {
                                //manager.matchMaker.CreateMatch(manager.matchName, manager.matchSize, true, "", manager.OnMatchCreate);
                            }
                            ypos += spacing;

                            GUI.Label(new Rect(xpos, ypos, kTextBoxWidth, 20), "Room Name:");
                            manager.matchName = GUI.TextField(new Rect(xpos + 100, ypos, 100, kTextBoxHeight), manager.matchName);
                            ypos += spacing;

                            ypos += 10;

                            if (GUI.Button(new Rect(xpos, ypos, 200, kTextBoxHeight), "Find Internet Match"))
                            {
                                //manager.matchMaker.ListMatches(0, 20, "", manager.OnMatchList);
                            }
                            ypos += spacing;
                        }
                        else
                        {
                            foreach (var match in manager.matches)
                            {
                                if (GUI.Button(new Rect(xpos, ypos, kTextBoxWidth, kTextBoxHeight), "Join Match:" + match.name))
                                {
                                    manager.matchName = match.name;
                                    manager.matchSize = (uint)match.currentSize;
                                    //manager.matchMaker.JoinMatch(match.networkId, "", manager.OnMatchJoined);
                                }
                                ypos += spacing;
                            }
                        }
                    }

                    if (GUI.Button(new Rect(xpos, ypos, kTextBoxWidth, kTextBoxHeight), "Change MM server"))
                    {
                        m_ShowServer = !m_ShowServer;
                    }
                    if (m_ShowServer)
                    {
                        ypos += spacing;
                        if (GUI.Button(new Rect(xpos, ypos, kTextBoxWidth, kTextBoxHeight), "Local"))
                        {
                            manager.SetMatchHost("localhost", 1337, false);
                            m_ShowServer = false;
                        }
                        ypos += spacing;
                        if (GUI.Button(new Rect(xpos, ypos, kTextBoxWidth, kTextBoxHeight), "Internet"))
                        {
                            manager.SetMatchHost("mm.unet.unity3d.com", 443, true);
                            m_ShowServer = false;
                        }
                        ypos += spacing;
                        if (GUI.Button(new Rect(xpos, ypos, kTextBoxWidth, kTextBoxHeight), "Staging"))
                        {
                            manager.SetMatchHost("staging-mm.unet.unity3d.com", 443, true);
                            m_ShowServer = false;
                        }
                    }

                    ypos += spacing;

                    GUI.Label(new Rect(xpos, ypos, kTextBoxWidth, kTextBoxHeight), "MM Uri: " + manager.matchMaker.baseUri);
                    ypos += spacing;

                    if (GUI.Button(new Rect(xpos, ypos, kTextBoxWidth, kTextBoxHeight), "Disable Match Maker"))
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