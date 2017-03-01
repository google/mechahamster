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

namespace Hamster.Utilities {

  // Slowly rotates the skybox.
  class RotateBackground : MonoBehaviour {
    // Rotation in Euler angles, set in editor.
    public Vector3 rotation;

    Quaternion quat;
    void Start() {
      quat = Quaternion.Euler(rotation);
      if (CommonData.inVrMode) {
        // Remove Rotation in VR mode.
        Destroy(this);
      }
    }

    void Update() {
      // Need to use time.realtimeSinceStartup instead of Time.DeltaTime, because
      // the way we pause the physics simulation (and hence the game, while we're
      // in editor mode) is by setting the Timescale to 0.
      if (CommonData.mainGame != null) {
        transform.rotation = Quaternion.Lerp(transform.rotation,
            transform.rotation * quat, CommonData.mainGame.TimeSinceLastUpdate);
      }
    }
  }

}
