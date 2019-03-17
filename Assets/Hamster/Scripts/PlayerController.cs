// Copyright 2017 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

namespace Hamster
{

    // Class to controll the player's avatar.  (The ball)
    public class PlayerController : NetworkBehaviour
    {
        Camera mycam;
        Transform mycamParentXform;
        Vector3 kYaxis = new Vector3(0, 1, 0);
        const float kExpectedFrameRate = 15.0f;     //  lowest expected frame rate in frames per second.
        const float kTimeScale = 1.0f / kExpectedFrameRate;  //  for frame rate independent 
        const float kForceFudgeFactor = 0.4f; //
        const float kPositionDelta = 0.15f;   //  fudge factor
        const float kCamRotateSpeed = 0.15f;
        const float kFellOffLevelHeight = -10.0f;
        const float kMaxVelocity = 20f;
        const float kMaxVelocitySquared = kMaxVelocity * kMaxVelocity;
        const float kServerForceThreshold = 0.02f;   //  below this threshold, we won't even tell the server about our force.
        const int kInitialHitPoints = 3;

        public bool isSpectator;   //  this give client authority to ball movement and uses a different control scheme to move the ball/camera.
        MultiplayerGame multiplayerGame;

        NetworkIdentity netIdentity;
        Rigidbody myRigidBody;

        public InputControllers.BasePlayerController inputController;

        // Game object that is spawned when the ball falls below the kill plane.
        public GameObject OnFallSpawn;

        // How long after death before restarting the level, in seconds.
        public float RespawnTime = 1.0f;

        // Has the player object touched a goal tile.
        public bool ReachedGoal { get; private set; }

        // Is the player object currently processing a death
        public bool IsProcessingDeath { get; private set; }

        // How many times the player can hit mines, spikes and similar before the game is over.
        public int HitPoints { get; private set; }

        private void Awake()
        {
            //  make some convenience pointers since GetComponent is slow. We don't want to do this often. Just once is enough.
            netIdentity = GetComponent<NetworkIdentity>();
            myRigidBody = GetComponent<Rigidbody>();
        }

        void Start()
        {
            multiplayerGame = FindObjectOfType<MultiplayerGame>();
            Time.maximumDeltaTime = kTimeScale;
            if (mycam==null)
            {
                mycam = FindObjectOfType<Camera>();
                mycamParentXform = mycam.transform.parent;
                if (mycamParentXform == null)
                {
                    mycamParentXform = mycam.transform;
                }
            }
            IsProcessingDeath = false;
            HitPoints = kInitialHitPoints;
            if (CommonData.currentReplayData == null)
            {
                inputController = new InputControllers.MultiInputController();
            }
            else
            {
                inputController = new InputControllers.ReplayController(
                    CommonData.currentReplayData,
                    CommonData.mainGame.stateManager.CurrentState() as States.Gameplay);
            }
        }

        public void MakeIntoSpectator(bool bmakeIntoSpectator)
        {
            //  put the camera on the ball.
            //  give client authority
            NetworkIdentity id = this.GetComponent<NetworkIdentity>();
            id.localPlayerAuthority = bmakeIntoSpectator;
            //  remove physics.
            this.myRigidBody.isKinematic = !bmakeIntoSpectator;
            this.myRigidBody.detectCollisions = !bmakeIntoSpectator;
            this.myRigidBody.useGravity = !bmakeIntoSpectator;
            this.isSpectator = bmakeIntoSpectator;

            CommonData.mainCamera.mode = bmakeIntoSpectator ? CameraController.CameraMode.Spectator : CameraController.CameraMode.Gameplay;
        }
        [Command]
        void Cmd_ResetPlayerPosition()
        {
            ResetPlayerPosition(this.gameObject);
        }
        [Command]
        void Cmd_ZeroPlayerMomentum()
        {
            ZeroPlayerMomentum();
        }

        [Command]
        public void Cmd_MakeIntoSpectator(bool bmakeIntoSpectator)
        {
            MakeIntoSpectator(bmakeIntoSpectator);
        }

        public void ClientMakeIntoSpectator(bool bmakeIntoSpectator)
        {
            if (isClient)
                Cmd_MakeIntoSpectator(bmakeIntoSpectator);

            MakeIntoSpectator(bmakeIntoSpectator);
        }
        //  The server should already be in Gameplay.GameplayMode.Gameplay state before the player has entered the game and been notified via OnStartLocalPlayer().
        static public void StartGamePlay()
        {
            Debug.Log("StartGamePlay()\n");

            Hamster.States.BaseState gameplayState = new Hamster.States.Gameplay(Hamster.States.Gameplay.GameplayMode.Gameplay);
            Hamster.CommonData.mainGame.stateManager.PushState(gameplayState);    //  mainGame isn't ready yet due to Unity having to start itself up.

            if (NetworkClient.active)
                MultiplayerGame.instance.ClientEnterMultiPlayerState<Hamster.States.ClientInGame>();
        }
        //  networking start.
        override public void OnStartLocalPlayer()   //  this is not enough. The server needs to know about the player's object so that it can reset its position upon death.
        {
            Debug.Log("OnStartLocalPlayer: " + this.name);
            if (CommonData.mainGame.player == null)
                CommonData.mainGame.player = this.gameObject;   //  this is obsolete legacy code that mirrors a codepath for single player. Should probably be removed.
            //  let the games begin!
            StartGamePlay();
        }


        void ResetPlayerPosition(GameObject plrGO)
        {
            Transform xformStart = customNetwork.CustomNetworkManager.singleton.GetStartPosition();
            plrGO.transform.position = xformStart.position;
            ZeroPlayerMomentum();   //  teleporters kill momentum otherwise strange things may happen.
        }
        void ZeroPlayerMomentum()
        {
            this.myRigidBody.velocity = Vector3.zero;
            this.myRigidBody.angularVelocity = Vector3.zero;
        }
        // Height of the kill-plane.
        // If the player's y-coordinate ever falls below this, it is treated as
        // a loss/failure.

        //    /*
        //     * Because the specatator has some freedom to move, it can mess up the camera. So we need to straighten it out every so often.
        //     * by that what we mean specifically, geometrically, is that we rotate around the camera's forward axis some number of degrees until its right axis is parallel with the x-z plane.
        //     * Nevermind. It's because the original code had the camera under another transform rather than controlling it directly.
        //     */
        //void StraightenCamera(Camera cam)
        //{
        //    float someNumberOfDegrees;
        //    Vector3 rotationalAxis = cam.transform.forward;
        //    Vector3 rightAxis = cam.transform.right;
        //    Vector3 xzPlaneAxis = rightAxis;//  this is the projection of the rightAxis onto the xzPlane
        //    xzPlaneAxis.y = 0;  //  this is the projection of the rightAxis onto the xzPlane
        //    //  with the rightAxis and its projection onto the xz-plane, we can the angle between these two vectors with a dot product
        //    float dotProduct = Vector3.Dot(rightAxis, xzPlaneAxis);
        //    float angleInRadians = Mathf.Acos(dotProduct);
        //    float angleInDegrees = Mathf.Rad2Deg*angleInRadians;   //  in degrees because n00bs.
        //    cam.transform.RotateAround(rotationalAxis, -angleInDegrees);//rotate the opposite way 
        //}

        void Update()
        {
            if (isLocalPlayer && Input.GetKeyDown(KeyCode.F12))  //  special key to request spectator mode.
            {
                ClientMakeIntoSpectator(!isSpectator);
            }
            if (IsProcessingDeath)
                return;
            //  common code to both localPlayer and server
            if (transform.position.y < kFellOffLevelHeight)
            {
                //  EndGame();  //  we don't end the game anymore because in a multiplayer game, other people are still playing. So let's not destroy the world.
                //  instead, let's just reset our position
                ResetPlayerPosition(this.gameObject);
            }

            if (isLocalPlayer && !isSpectator)
            {
                float elapsedTime = Time.deltaTime; //  time since last frame.
                Vector2 input = inputController.GetInputVector();   //  kKeyVelocity
                input *= kTimeScale;    //  just a fudge factor to make it feel right.
                if (elapsedTime <= 0.01f)   //  guard vs. divide by zero or negative nonsense.
                {
                    input = new Vector2(0, 0);
                }
                else
                {
                    input /= elapsedTime;   //  scaled via time elapsed so that we are frame-rate independent;
                }
                NetworkIdentity netid = GetComponent<NetworkIdentity>();
                const double kAxesFlipServerVersion = 1.20190212;
                double serverVersion = 0;
                if (CommonData.networkmanager!= null)
                    serverVersion = CommonData.networkmanager.getServerVersionDouble(netid);
                //  note: We're using 1 kg/s as a hack here implicitly. Thus, our units seem to be m/s, but really should be Newton = kg*m/(s^2) But since we're not writing a physics engine here, this shortcut should suffice.
                if (serverVersion >= kAxesFlipServerVersion || serverVersion == 0)  //  serverVersion=0 means that we weren't able to get an answer from the server yet. In that case, assume the server is the latest version. There are not going to be any "older" versions of the server in the wild!
                {
                    forceThisFrame = new Vector3(input.x, 0, input.y);  //  after 2019/02/12 - This is the new "correct" axes for the world that will allow us to do vector and matrix math somehwat more intuitively. Unity is a left-handed coordinate system with y-up. Original MechaHamster code defined its inputs in an xy-plane with a z-up world. So, we have to transform it to the way the geometric world is oriented. Some people just like to make things even more confusing, I guess.

                }
                else//  this is the obsolete way of having a mismatch between input axes orientation and world axes orientation from original Mecha-hamster.
                {
                    forceThisFrame = new Vector3(input.x, input.y, 0);  //  prior 2019/02/12 - that's how original MechaHamster code defined its inputs. So, we have to transform it to the way the world is oriented.
                }

                forceThisFrame *= kForceFudgeFactor;

                if (forceThisFrame.magnitude < kServerForceThreshold)
                    return;  // if we're too weak of a force, the server does not need know about you.
                if (forceThisFrame.sqrMagnitude > kMaxVelocitySquared)
                {
                    Debug.LogWarning(this.name + " used the force way too much.");
                    //  clamp the force to the max acceleration (for this frame) instead.
                    forceThisFrame.Normalize();
                    forceThisFrame *= kMaxVelocity/elapsedTime;
                    //return;  // if we're too strong, something bad happened, like a NaN perhaps. Don't send bad data to the server. We just bail here becaus we don't know what happened exactly. 
                }

                if (this.isClient)
                {
                    //  this tells the server to include your force into its next force calculation, whenever that might be.
                    if (isSpectator)
                    {
                        Cmd_ServerAddPosition(forceThisFrame);
                    }
                    else
                    {
                        if (forceThisFrame.magnitude >= kServerForceThreshold)// if we're too weak of a force, the server does not need know about you.
                            Cmd_ServerAddForce(forceThisFrame);
                    }
                    AddForce(forceThisFrame);   //  do this on the client. The server will send us back the correct positions. But this can get us on a headstart of where we think we should be.
                }
            }   //  if(isLocalPlayer)
                // ==================================================================================================================
                //  note: due to the return calls in the middle of isLocalPlayer, do not put common code after this line.
                // ==================================================================================================================
        }

        // Triggers a delayed reset of the level, using coroutines.
        IEnumerator DelayedResetLevel()
        {
            yield return new WaitForSeconds(RespawnTime);
            CommonData.gameWorld.ResetMap();
        }

        public void HandleGoalCollision()
        {
            ReachedGoal = true;
            bool isLocalPlayerAuthority = netIdentity.localPlayerAuthority;// are we client-authority?
            if (NetworkServer.active)
            {
                Debug.LogWarning("Server: Player has reached goal: " + this.name);
                NetworkConnection conn = this.connectionToClient;
                if (conn != null)
                {
                    Debug.LogWarning("Server call attempt multiplayerGame.cmd_OnServerClientFinishedGame : " + conn.ToString() + "\n");
                    if (multiplayerGame == null)
                    {
                        multiplayerGame = FindObjectOfType<MultiplayerGame>();
                    }
                    multiplayerGame.ClientFinishedGame(conn);
                    if (!isLocalPlayerAuthority)
                        {
                            //ResetPlayerPosition(this.gameObject);   //  only do this on the server.
                        }
                    }
                }
            if (NetworkClient.active)
            {
                Debug.LogWarning("Client: Player has reached goal: " + this.name);
                ResetPlayerPosition(this.gameObject);   //  Now that we have player auth, we need to do this!
            }

            //  on the client, need to tell the player that they won.
            //  on the server, need to update the game state so that we know a player has "won" the level.
        }

        //==========================================================================================================
        //  server stuff below
        //==========================================================================================================
        Vector3 forceThisFrame;


        //  These are methods that the server should handle.
        void EndGame()
        {
            if (!isServer) return;

            if (OnFallSpawn != null)
            {
                // Spawn in the death particles. Note that the particles should clean themselves up.
                Instantiate(OnFallSpawn, transform.position, Quaternion.identity);
            }

            // We don't want the ball to keep the ball where it died, so that the camera can
            // see the on death particles before respawning.
            IsProcessingDeath = true;
            Rigidbody rigidBody = GetComponent<Rigidbody>();
            rigidBody.isKinematic = true;
            //// Disable the children, which have the visible components of the ball.
            //foreach (Transform child in transform)
            //{
            //    child.gameObject.SetActive(false);
            //}
            StartCoroutine(DelayedResetLevel());
        }


        public void Hit(int damageAmount)
        {
            if (!isServer) return;

            if (damageAmount == 0)
                return;

            HitPoints -= damageAmount;

            if (HitPoints <= 0)
                EndGame();
        }
        void AddPosition(Vector3 deltaPos)
        {
            //  bail on garbage numbers
            if (float.IsNaN(deltaPos.x) || float.IsNaN(deltaPos.y) || float.IsInfinity(deltaPos.z)) return;
            this.transform.position += deltaPos;
        }
        void AddForce(Vector3 force)
        {
            //  bail on garbage numbers
            if (float.IsNaN(force.x) || float.IsNaN(force.y) || float.IsInfinity(force.z)) return;

            Rigidbody rigidBody = myRigidBody;
            if (rigidBody == null)
                rigidBody = myRigidBody = GetComponent<Rigidbody>();
            if (rigidBody != null)
                rigidBody.AddForce(force); //  yeah, flip the units around to be z-up. Not great, but That's the coordinate system that was inherited.
        }

        bool CheckHeightDeath()
        {
            bool isDead = false;
            if (transform.position.y < kFellOffLevelHeight)
            {
                isDead = true;
                if (OnFallSpawn != null)    //  this needs to be rewritten for network.
                {
                    Rigidbody rigidBody = myRigidBody;

                    // Spawn in the death particles. Note that the particles should clean themselves up.
                    Instantiate(OnFallSpawn, transform.position, Quaternion.identity);

                    // We don't want the ball to keep the ball where it died, so that the camera can
                    // see the on death particles before respawning.
                    IsProcessingDeath = true;
                    rigidBody.isKinematic = true;
                    // Disable the children, which have the visible components of the ball.
                    foreach (Transform child in transform)
                    {
                        child.gameObject.SetActive(false);
                    }
                    StartCoroutine(DelayedResetLevel());
                }
            }
            return isDead;
        }
        //  Server commands start here.
        [Command]
        void Cmd_ServerAddForce(Vector3 force)
        {
            AddForce(force);
        }

        //  lesser used. For spectator movement
        [Command]
        void Cmd_ServerAddPosition(Vector3 deltaPos)
        {
            AddPosition(deltaPos);
        }

        public static PlayerController FindLocalPlayerController()
        {
            PlayerController[] players = FindObjectsOfType<PlayerController>();
            foreach(PlayerController player in players)
            {
                if( player.isLocalPlayer )
                {
                    return player;
                }
            }
            return null;    // unexpected...
        }
    }
}
