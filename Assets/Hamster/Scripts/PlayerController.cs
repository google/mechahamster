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
        const float kTimeScale = 1.0f / 60.0f;  //  for frame rate independent 
        const float kFellOffLevelHeight = -10.0f;
        const float kMaxVelocity = 20f;
        const float kMaxVelocitySquared = kMaxVelocity * kMaxVelocity;
        const int kInitialHitPoints = 3;

        bool isSpectator;   //  this give client authority to ball movement and uses a different control scheme to move the ball/camera.

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
            this.
            MakeIntoSpectator(bmakeIntoSpectator);
            //  different control scheme.
        }
        //  The server should already be in Gameplay.GameplayMode.Gameplay state before the player has entered the game and been notified via OnStartLocalPlayer().
        static public void StartGamePlay()
        {
            Hamster.States.BaseState gameplayState = new Hamster.States.Gameplay(Hamster.States.Gameplay.GameplayMode.Gameplay);
            Hamster.CommonData.mainGame.stateManager.PushState(gameplayState);    //  mainGame isn't ready yet due to Unity having to start itself up.
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
        }
        void ZeroPlayerMomentum()
        {
            this.myRigidBody.velocity = Vector3.zero;
            this.myRigidBody.angularVelocity = Vector3.zero;
        }
        // Height of the kill-plane.
        // If the player's y-coordinate ever falls below this, it is treated as
        // a loss/failure.

        float SpectatorControls()
        {
            if (!isSpectator) return 0.0f;   //  non-spectators do not have access to this special stuff.

            float zforce = 0.0f;
            float elapsedTime = Time.deltaTime;
            Vector2 input = inputController.GetInputVector();   //  kKeyVelocity
            float inputMag = Hamster.InputControllers.KeyboardController.kKeyVelocity * kTimeScale;    //  just a fudge factor to make it feel right.

            if (Input.GetKeyDown(KeyCode.Space))    //  space to reset position to StartPosition square
            {
                Cmd_ResetPlayerPosition();
                this.ResetPlayerPosition(this.gameObject);
                Cmd_ZeroPlayerMomentum();
                ZeroPlayerMomentum();
            }
            else if (Input.GetKeyDown(KeyCode.Return))    //  Return/Enter to zero momentum
            {
                Cmd_ZeroPlayerMomentum();
                ZeroPlayerMomentum();
            }
            else if (Input.GetKeyDown(KeyCode.PageUp))
            {
                if (elapsedTime > 0.01f)
                    zforce = inputMag/ elapsedTime;
            }
            else if (Input.GetKeyDown(KeyCode.PageDown))
            {
                if (elapsedTime > 0.01f)
                    zforce = -inputMag / elapsedTime;
            }
            return zforce;
        }
        void Update()
        {
            float spectatorZforce = 0.0f;
            if (Input.GetKeyDown(KeyCode.F12))  //  special key to request spectator mode.
            {
                ClientMakeIntoSpectator(!isSpectator);
            }
            if (this.isSpectator)
            {
                spectatorZforce = SpectatorControls();
            }
            if (IsProcessingDeath)
                return;
            //  common code to both localPlayer and server
            if (transform.position.y < kFellOffLevelHeight)
            {
                //  EndGame();  //  we don't end the game anymore because in a multiplayer game, other people are still playing. So let's not destroy the world.
                //  instead, let's just reset our position
                if (isServer)
                {
                    ResetPlayerPosition(this.gameObject);
                }
            }

            if (isLocalPlayer)
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
                //  note: We're using 1 kg/s as a hack here implicitly. Thus, our units seem to be m/s, but really should be Newton = kg*m/(s^2) But since we're not writing a physics engine here, this shortcut should suffice.
                forceThisFrame = input;

                if (this.isSpectator)
                {
                    forceThisFrame.z = spectatorZforce;
                }
                if (forceThisFrame.magnitude <= 0.05f)
                    return;  // if we're too weak of a force, the server does not need know about you.
                if (forceThisFrame.sqrMagnitude > kMaxVelocitySquared)
                {
                    Debug.LogWarning(this.name + " used the force way too much.");
                    return;  // if we're too strong, something bad happened, like a NaN perhaps. Don't send bad data to the server. We just bail here becaus we don't know what happened exactly. 
                }

                if (this.isClient)
                {
                    //  this tells the server to include your force into its next force calculation, whenever that might be.
                    Cmd_ServerAddForce(forceThisFrame);
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
            ResetPlayerPosition(this.gameObject);
            Debug.LogWarning("Player has reached goal: " + this.name);

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

        void AddForce(Vector3 force)
        {
            //  bail on garbage numbers
            if (float.IsNaN(force.x) || float.IsNaN(force.y) || float.IsInfinity(force.z)) return;

            Rigidbody rigidBody = myRigidBody;
            if (rigidBody == null)
                rigidBody = myRigidBody = GetComponent<Rigidbody>();
            if (rigidBody != null)
                rigidBody.AddForce(new Vector3(force.x, force.z, force.y)); //  yeah, flip the units around to be z-up. Not great, but That's the coordinate system that was inherited.
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

    }
}
