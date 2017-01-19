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

namespace Hamster.InputControllers {

  // Class for accelerometer controller interfaces.
  // Responsible for returning a 2d vector representing the
  // player's movement, based on device accelerometers.  (i. e. tilting)
  public class TiltController : BasePlayerController {
    // Scalar, for modifying the x/z components of the accelerometer
    // velocity, to generate the final player velocity from tilting
    // the device.
    const float kTiltVelocity = 16.0f;


    public override Vector2 GetInputVector() {
      Vector3 tiltInput = Input.acceleration.normalized * kTiltVelocity;
      return new Vector3(tiltInput.x, tiltInput.y);
    }
  }
}
