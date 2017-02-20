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

    // The number of decimal places to round the time to.
    const int RoundTimeToDecimal = 3;

    // How long it takes to reach setting the timescale to 0, in seconds.
    private float SlowdownTotalTime = 1.0f;
    // The realtime that the state is started, in seconds.
    private float StartTime { get; set; }

    // The total time taken to finish the level, in milliseconds.
    private long ElapsedGameTime { get; set; }

    // Tracks if the score being shown has been uploaded.
    private bool ScoreUploaded { get; set; }

    // Unity component for the GUI Menu.
    Menus.LevelFinishedGUI dialogComponent;

    public override void Initialize() {
      Time.timeScale = 1.0f;
      StartTime = Time.realtimeSinceStartup;
      // Grab the elapsed game time now, as we leave timeScale > 0 for a bit.
      ElapsedGameTime = CommonData.gameWorld.ElapsedGameTimeMs;
      ScoreUploaded = false;

      dialogComponent = SpawnUI<Menus.LevelFinishedGUI>(StringConstants.PrefabsLevelFinishedMenu);
      dialogComponent.NewRecordText.gameObject.SetActive(false);
      dialogComponent.SubmitButton.gameObject.SetActive(!CommonData.gameWorld.HasPendingEdits);
      dialogComponent.ElapsedTimeText.text = string.Format(StringConstants.FinishedTimeText,
          Utilities.StringHelper.FormatTime(ElapsedGameTime));
    }

    public override void Resume(StateExitValue results) {
      dialogComponent.SubmitButton.gameObject.SetActive(
        !CommonData.gameWorld.HasPendingEdits && !ScoreUploaded);
      if (results != null) {
        if (results.sourceState == typeof(UploadTime)) {
          ScoreUploaded = (bool)results.data;
        } else if (results.sourceState == typeof(TopTimes)) {
          ScoreUploaded = true;
        }
      }
    }

    public override void Suspend() {
      dialogComponent.gameObject.SetActive(false);
      Time.timeScale = 0.0f;
    }

    public override StateExitValue Cleanup() {
      DestroyUI();
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

    public override void HandleUIEvent(GameObject source, object eventData) {
      if (source == dialogComponent.LevelsButton.gameObject) {
        manager.PopState();
      } else if (source == dialogComponent.RetryButton.gameObject) {
        manager.SwapState(new Gameplay());
      } else if (source == dialogComponent.SubmitButton.gameObject) {
        manager.PushState(new UploadTime(ElapsedGameTime));
      } else if (source == dialogComponent.MainButton.gameObject) {
        manager.ClearStack(new MainMenu());
      }
    }
  }
}
