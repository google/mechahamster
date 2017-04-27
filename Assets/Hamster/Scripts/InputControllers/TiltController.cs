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
    const float TiltVelocity = 16.0f;

    // How fast the down-vector moves, when we're recalibrating.
    const float RecenterSpeed = 0.9f;

    // How far the down-vector can be off from the input (in radians)
    // before we start recalibrating.
    const float MaxAngleDifference = Mathf.PI / 6.0f;  // 30 degrees.
    static float MaxAngleSinSquared =
      Mathf.Sin(MaxAngleDifference) * Mathf.Sin(MaxAngleDifference);

    Vector3 currentDownVector;
    public TiltController() {
      currentDownVector = Input.acceleration.normalized;
      currentDownVector.x = 0;
    }

    public override Vector2 GetInputVector() {
      Vector3 inputVector = Input.acceleration.normalized;
      // For our down-vector, (and the self-adjustments) we only care
      // about the Y and Z axis.  We don't self-center for the X axis,
      // since we can assume that the phone will always be held perpendicular
      // to the face.
      Vector3 newDownVector = new Vector3(0.0f, inputVector.y, inputVector.z);

      Quaternion fromDownToWorld = Quaternion.FromToRotation(currentDownVector, Vector3.down);

      Vector3 translatedInput = fromDownToWorld * inputVector;

      // If the current angle is too far from the base state, recenter.
      if (Mathf.Abs(
        Vector3.Cross(newDownVector, currentDownVector).sqrMagnitude) > MaxAngleSinSquared ||
        Vector3.Dot(newDownVector, currentDownVector) < 0) {
        currentDownVector =
          Vector3.Lerp(currentDownVector, newDownVector,
            RecenterSpeed * Time.deltaTime).normalized;
      }

      Vector3 tiltInput = translatedInput * TiltVelocity;
      return new Vector3(tiltInput.x, -tiltInput.z);
    }
  }
}
