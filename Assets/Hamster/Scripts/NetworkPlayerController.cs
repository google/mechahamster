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
using UnityEngine.Networking;
using System.Collections;

namespace Hamster
{
    // Class to controll the player's avatar.  (The ball)
    public class NetworkPlayerController : NetworkBehaviour
    {
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

        private void Awake()
        {
            netIdentity = GetComponent<NetworkIdentity>();
            myRigidBody = GetComponent<Rigidbody>();
        }
        void Start()
        {
            IsProcessingDeath = false;
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
        const float kFellOffLevelHeight = -10.0f;
        const float kMaxVelocity = 20f;
        const float kMaxVelocitySquared = kMaxVelocity * kMaxVelocity;
        const float kTimeScale = 1.0f / 60.0f;
        Vector2 forceThisFrame;

        [Command]
        void Cmd_ServerAddForce(Vector3 force)
        {
            //  bail on garbage numbers
            if (float.IsNaN(force.x) || float.IsNaN(force.y) || float.IsInfinity(force.z)) return;

            Rigidbody rigidBody = myRigidBody;
            if (rigidBody == null)
                rigidBody = myRigidBody = GetComponent<Rigidbody>();
            rigidBody.AddForce(new Vector3(force.x, 0, force.y));
        }

        private bool CheckDeath()
        {
            bool isDead = false;

            if (transform.position.y < kFellOffLevelHeight)
            {
                Rigidbody rigidBody = myRigidBody;
                if (OnFallSpawn != null)    //  this needs to be rewritten for network.
                {
                    // Spawn in the death particles. Note that the particles should clean themselves up.
                    Instantiate(OnFallSpawn, transform.position, Quaternion.identity);

                }
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
            return isDead;
        }
        //  input routines handled here.
        private void Update()
        {
            if (IsProcessingDeath)
                return;
            if (CheckDeath())
                return;

            Rigidbody rigidBody = GetComponent<Rigidbody>();

            float elapsedTime = Time.deltaTime; //  time since last frame.
            Vector2 input = inputController.GetInputVector();
            input *= kTimeScale;
            input /= elapsedTime;   //  scaled via time;
            forceThisFrame = input;

            if (forceThisFrame.magnitude <= 0.05f)
                return;  // if we're too weak of a force, do nothing
            if (forceThisFrame.magnitude > 1000.0f)
                return;  // if we're too strong, something bad happened, like a NaN perhaps.

            if (this.isClient)
            {
                Cmd_ServerAddForce(forceThisFrame);
            }
            //else //   we no longer do this now that the server takes the commands to add force!
            //{
            //    rigidBody.AddForce(new Vector3(forceThisFrame.x, 0, forceThisFrame.y));
            //    if (transform.position.y < kFellOffLevelHeight)
            //    {
            //        if (OnFallSpawn != null)
            //        {
            //            // Spawn in the death particles. Note that the particles should clean themselves up.
            //            Instantiate(OnFallSpawn, transform.position, Quaternion.identity);

            //            // We don't want the ball to keep the ball where it died, so that the camera can
            //            // see the on death particles before respawning.
            //            IsProcessingDeath = true;
            //            rigidBody.isKinematic = true;
            //            // Disable the children, which have the visible components of the ball.
            //            foreach (Transform child in transform)
            //            {
            //                child.gameObject.SetActive(false);
            //            }
            //            StartCoroutine(DelayedResetLevel());
            //        }
            //    }
            //}
        }
        void FixedUpdate()
        {
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
    }
}
