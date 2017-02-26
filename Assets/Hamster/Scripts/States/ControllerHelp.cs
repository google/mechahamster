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
using System.Collections.Generic;
using Firebase.Unity.Editor;


namespace Hamster.States {
  // State for the "how to hold your controller" screen
  // that pops up when you start a game.  Ends after a fixed
  // amount of time has passed, or faster if you hold the
  // controller horizontally as pictured.  (i. e. minimum
  // acceleration in the X/Z axes.)
  class ControllerHelp : BaseState {

    bool currentlyFlat;
    float startedFlatTime;
    float stateStartedTime;

    // How long (in seconds) they have to hold the controller flat to get out of this.
    const float HoldItFlatGoal = 1.0f;
    // How long (in seconds) before the state times itself out and moves on.
    const float AutoTimeout = 6.0f;
    // The maximum magnitude of the x/y components of the accelerometer.
    const float FlatTolerance = 4.0f;

    Menus.ControllerHelpGUI guiComponent;

    Gameplay.GameplayMode mode;
    public ControllerHelp(Gameplay.GameplayMode mode) {
      this.mode = mode;
    }

    public override void Initialize() {
      stateStartedTime = Time.realtimeSinceStartup;
      guiComponent = SpawnUI<Menus.ControllerHelpGUI>(StringConstants.PrefabsControllerHelp);
    }

    public override void Resume(StateExitValue results) {
      ShowUI();
    }

    public override void Suspend() {
      HideUI();
    }

    public override StateExitValue Cleanup() {
      DestroyUI();
      return null;
    }

    public override void Update() {
      bool stateEnding = false;
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
      Vector2 tilt = new Vector2(GvrController.Accel.z, -GvrController.Accel.x);
#else
      Vector2 tilt = Vector2.zero;
#endif
      float currentTime = Time.realtimeSinceStartup;

      // Check if they've held the controller flat for long enough.
      if (tilt.SqrMagnitude() <= FlatTolerance * FlatTolerance) {
        // They're holding it flat.
        if (currentlyFlat) {
          // Have they been holding it flat for a while?
          if (currentTime - startedFlatTime >= HoldItFlatGoal) {
            stateEnding = true;
          }
        } else {
          // They just started.  Start the timer.
          currentlyFlat = true;
          startedFlatTime = currentTime;
        }
      } else {
        currentlyFlat = false;
      }
      // If they've been stuck on this screen for too long, move on.
      if (currentTime - stateStartedTime >= AutoTimeout) {
        stateEnding = true;
      }

      if (stateEnding) {
        manager.SwapState(new Gameplay(mode));
      }
    }

  }
}
