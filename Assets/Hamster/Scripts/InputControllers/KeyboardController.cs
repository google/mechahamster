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

  // Class for keyboard controller interfaces.
  // Responsible for returning a 2d vector representing the
  // player's movement, based on keypresses.
  public class KeyboardController : BasePlayerController {
    // Velocity, in world-units-per-second, from holding down
    // a key.
    const float kKeyVelocity = 8.0f;

    public override Vector2 GetInputVector() {
      Vector2 result = Vector2.zero;

      if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) {
        result += new Vector2(-kKeyVelocity, 0);
      }
      if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) {
        result += new Vector2(kKeyVelocity, 0);
      }
      if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) {
        result += new Vector2(0, kKeyVelocity);
      }
      if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) {
        result += new Vector2(0, -kKeyVelocity);
      }
      return result;
    }
  }
}
