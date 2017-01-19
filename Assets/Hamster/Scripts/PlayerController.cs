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
    InputControllers.BasePlayerController inputController;

    void Start() {
      inputController = new InputControllers.MultiInputController();
    }

    // Height of the kill-plane.
    // If the player's y-coordinate ever falls below this, it is treated as
    // a loss/failure.
    const float kFellOffLevelHeight = -10.0f;
    const float kMaxVelocity = 20f;
    const float kMaxVelocitySquared = kMaxVelocity * kMaxVelocity;

    // Since we're doing physics work, we use FixedUpdate instead of Update.
    void FixedUpdate() {
      Rigidbody rigidBody = GetComponent<Rigidbody>();

      Vector2 input = inputController.GetInputVector();
      rigidBody.AddForce(new Vector3(input.x, 0, input.y));

      if (transform.position.y < kFellOffLevelHeight) {
        CommonData.mainGame.DestroyPlayer();
      }
    }
  }
}
