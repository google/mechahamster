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

namespace Hamster.MapObjects {

  // When stepped on, these kick the player up into the air.
  public class JumpTile : MapObject {

    // Static variable shared across all jumptiles, to make sure
    // that they only get one jump impulse at once.  (Stops them
    // from getting double- or quadruple-powered jumps, if they
    // manage to touch more than one jump tile on the same frame.)
    static bool triggeredThisFrame;

    // Velocity of the kick, in world-units/second.
    public float kJumpVelocity = 8;

    public void Update() {
      triggeredThisFrame = false;
    }

    void OnTriggerEnter(Collider collider) {
      if (!triggeredThisFrame) {
        triggeredThisFrame = true;
        if (collider.GetComponent<PlayerController>() != null) {
          Rigidbody rigidbody = collider.attachedRigidbody;
          collider.attachedRigidbody.velocity =
              new Vector3(rigidbody.velocity.x, 0, rigidbody.velocity.z);
          rigidbody.AddForce(new Vector3(0.0f, kJumpVelocity, 0.0f), ForceMode.Impulse);
        }
      }
    }
  }
}
