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

  // Base Class for player controller interfaces.
  // Responsible for returning a 2-d vector representing
  // the player's movement.  (Abstracted to make it easy to write
  // new ones for different control schemes.)
  public class BasePlayerController {

    public virtual Vector2 GetInputVector() {
      return Vector2.zero;
    }
  }
}
