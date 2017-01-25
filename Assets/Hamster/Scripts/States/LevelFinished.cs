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
using System;
using System.Collections.Generic;

namespace Hamster.States {
  class LevelFinished : BaseState {
    // Width/Height of the menu, expressed as a portion of the screen width:
    const float MenuWidth = 0.40f;
    const float MenuHeight = 0.75f;

    // How long it takes to reach setting the timescale to 0, in seconds.
    private float SlowdownTotalTime = 1.0f;
    // The realtime that the state is started, in seconds.
    private float StartTime { get; set; }

    public override void Initialize() {
      Time.timeScale = 1.0f;
      StartTime = Time.realtimeSinceStartup;
    }

    public override void Suspend() {
      Time.timeScale = 0.0f;
    }

    public override StateExitValue Cleanup() {
      Time.timeScale = 0.0f;
      return null;
    }

    public override void Update() {
      float elapsedTime = Time.realtimeSinceStartup - StartTime;
      if (elapsedTime < SlowdownTotalTime) {
        Time.timeScale = Mathf.Lerp(1.0f, 0.0f, elapsedTime / SlowdownTotalTime);
      } else {
        Time.timeScale = 0.0f;
      }
    }

    public override void OnGUI() {
      float menuWidth = MenuWidth * Screen.width;
      float menuHeight = MenuHeight * Screen.height;
      GUI.skin = CommonData.prefabs.guiSkin;

      GUILayout.BeginArea(
          new Rect((Screen.width - menuWidth) / 2, (Screen.height - menuHeight) / 2,
          menuWidth, menuHeight));

      GUILayout.Label(StringConstants.FinishedTopText);

      GUILayout.BeginVertical();
      if (GUILayout.Button(StringConstants.ButtonRetry)) {
        CommonData.gameWorld.ResetMapObjects();
        manager.SwapState(new Gameplay());
      }
      if (GUILayout.Button(StringConstants.ButtonExit)) {
        manager.PopState();
      }
      GUILayout.EndVertical();

      GUILayout.EndArea();
    }
  }
}
