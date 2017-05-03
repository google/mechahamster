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
using Firebase.RemoteConfig;

namespace Hamster.MapObjects {

  // When stepped on, these accelerate the ball in a direction.
  public class AccelerationTile : MapObject {

    // Static variable shared across all AccelerationTiles, to make
    // sure that only one acceleration applies at a time.  (Stops them
    // from getting double- or quadruple-speed, if they manage to touch
    // more than one tile on the same frame.)
    static bool triggeredThisFrame;

    // Acceleration force applied to the ball, set through Remote Config.
    public float Acceleration { get; private set; }

    private void Start() {
      Acceleration = (float)FirebaseRemoteConfig.GetValue(
          StringConstants.RemoteConfigAccelerationTileForce).DoubleValue;
    }

    public void FixedUpdate() {
      triggeredThisFrame = false;
    }

    private void OnTriggerStay(Collider collider) {
      if (!triggeredThisFrame) {
        triggeredThisFrame = true;
        if (collider.GetComponent<PlayerController>() != null) {
          Rigidbody rigidbody = collider.attachedRigidbody;
          Vector3 force = Vector3.forward * Acceleration;
          force = transform.TransformDirection(force);
          rigidbody.AddForce(force, ForceMode.Force);
        }
      }
    }
  }
}
