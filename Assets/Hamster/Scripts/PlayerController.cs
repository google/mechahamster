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
        const float kFellOffLevelHeight = -10.0f;
        const float kMaxVelocity = 20f;
        const float kMaxVelocitySquared = kMaxVelocity * kMaxVelocity;
        const int kInitialHitPoints = 3;

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

        // Height of the kill-plane.
        // If the player's y-coordinate ever falls below this, it is treated as
        // a loss/failure.

        void Update()
        {
            if (IsProcessingDeath)
                return;

            if (isLocalPlayer)
            {
                float elapsedTime = Time.deltaTime; //  time since last frame.
                Vector2 input = inputController.GetInputVector();
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
                }


                Rigidbody rigidBody = GetComponent<Rigidbody>();
                rigidBody.AddForce(new Vector3(input.x, 0, input.y));

                if (transform.position.y < kFellOffLevelHeight)
                    EndGame();
            }
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
        }

        //==========================================================================================================
        //  server stuff below
        //==========================================================================================================
        const float kTimeScale = 1.0f / 60.0f;
        Vector2 forceThisFrame;

        //  These are methods that the server should handle.
        void EndGame()
        {
            if (!isServer) return;

            if (OnFallSpawn != null)
            {
                // Spawn in the death particles. Note that the particles should clean themselves up.
                Instantiate(OnFallSpawn, transform.position, Quaternion.identity);

                // We don't want the ball to keep the ball where it died, so that the camera can
                // see the on death particles before respawning.
                IsProcessingDeath = true;
                Rigidbody rigidBody = GetComponent<Rigidbody>();
                rigidBody.isKinematic = true;
                // Disable the children, which have the visible components of the ball.
                foreach (Transform child in transform)
                {
                    child.gameObject.SetActive(false);
                }
                StartCoroutine(DelayedResetLevel());
            }
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

        //  Server commands start here.
        [Command]
        void Cmd_ServerAddForce(Vector3 force)
        {
            //  bail on garbage numbers
            if (float.IsNaN(force.x) || float.IsNaN(force.y) || float.IsInfinity(force.z)) return;

            Rigidbody rigidBody = myRigidBody;
            if (rigidBody == null)
                rigidBody = myRigidBody = GetComponent<Rigidbody>();
            rigidBody.AddForce(new Vector3(force.x, 0, force.y));
            if (transform.position.y < kFellOffLevelHeight)
            {
                if (OnFallSpawn != null)    //  this needs to be rewritten for network.
                {
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

        }

    }
}
