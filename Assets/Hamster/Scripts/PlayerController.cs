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

namespace Hamster {

  // Class to controll the player's avatar.  (The ball)
  public class PlayerController : MonoBehaviour {
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

    void Start() {
      IsProcessingDeath = false;
      HitPoints = kInitialHitPoints;
      if (CommonData.currentReplayData == null) {
        inputController = new InputControllers.MultiInputController();
      } else {
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
    const int kInitialHitPoints = 3;

    // Since we're doing physics work, we use FixedUpdate instead of Update.
    void FixedUpdate() {
      if (IsProcessingDeath)
        return;

      Vector2 input = inputController.GetInputVector();
      Rigidbody rigidBody = GetComponent<Rigidbody>();
      rigidBody.AddForce(new Vector3(input.x, 0, input.y));

      if (transform.position.y < kFellOffLevelHeight)
        EndGame();
    }

    // Triggers a delayed reset of the level, using coroutines.
    IEnumerator DelayedResetLevel() {
      yield return new WaitForSeconds(RespawnTime);
      CommonData.gameWorld.ResetMap();
    }

    public void HandleGoalCollision() {
      ReachedGoal = true;
    }
    public void Hit(int damageAmount) {
      if (damageAmount == 0)
        return;

      HitPoints -= damageAmount;

      if (HitPoints <= 0)
        EndGame();
    }
    void EndGame() {
      if (OnFallSpawn != null) {
        // Spawn in the death particles. Note that the particles should clean themselves up.
        Instantiate(OnFallSpawn, transform.position, Quaternion.identity);

        // We don't want the ball to keep the ball where it died, so that the camera can
        // see the on death particles before respawning.
        IsProcessingDeath = true;
        Rigidbody rigidBody = GetComponent<Rigidbody>();
        rigidBody.isKinematic = true;
        // Disable the children, which have the visible components of the ball.
        foreach (Transform child in transform) {
          child.gameObject.SetActive(false);
        }
        StartCoroutine(DelayedResetLevel());
      }
    }
  }
}
