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
using Firebase.Leaderboard;

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

    private LeaderboardController LeaderboardController;

    Gameplay.GameplayMode mode;
    public LevelFinished(Gameplay.GameplayMode mode) {
      this.mode = mode;
    }

    public override void Initialize() {
      Time.timeScale = 1.0f;
      StartTime = Time.realtimeSinceStartup;
      // Grab the elapsed game time now, as we leave timeScale > 0 for a bit.
      ElapsedGameTime = CommonData.gameWorld.ElapsedGameTimeMs;
      ScoreUploaded = false;

      // Only log completion if the level is not being edited, to ignore in progress levels.
      if (!CommonData.gameWorld.HasPendingEdits) {
        Firebase.Analytics.FirebaseAnalytics.LogEvent(StringConstants.AnalyticsEventMapFinished,
          StringConstants.AnalyticsParamMapId, CommonData.gameWorld.worldMap.mapId);
      }

      dialogComponent = SpawnUI<Menus.LevelFinishedGUI>(StringConstants.PrefabsLevelFinishedMenu);
      dialogComponent.NewRecordText.gameObject.SetActive(false);
      // We only allow them to submit times if we're online, and
      // not on a map that has pending edits.
      dialogComponent.SubmitButton.gameObject.SetActive(
        !CommonData.gameWorld.HasPendingEdits && CommonData.ShowInternetMenus());
      dialogComponent.ElapsedTimeText.text = string.Format(StringConstants.FinishedTimeText,
          Utilities.StringHelper.FormatTime(ElapsedGameTime));

      if (mode == Gameplay.GameplayMode.Editor) {
        // A few minor differences in edit mode:
        // * There is no "main menu" button
        // * The "levels" button is replaced by an "editor" button.
        // * "editor" is moved over to the left.
        dialogComponent.MainButton.gameObject.SetActive(false);
        UnityEngine.UI.Text textComponent =
          dialogComponent.LevelsButton.gameObject.GetComponentInChildren<UnityEngine.UI.Text>();
        if (textComponent != null) {
          textComponent.text = StringConstants.ButtonEditor;
        }
        dialogComponent.LevelsButton.transform.position =
            dialogComponent.MainButton.transform.position;
      }

    }

    public override void Resume(StateExitValue results) {
      if (results != null) {
        if (results.sourceState == typeof(UploadTime)) {
          ScoreUploaded = ScoreUploaded || (bool)results.data;
        } else if (results.sourceState == typeof(TopTimes)) {
          ScoreUploaded = true;
        }
      }

      ShowUI();
      dialogComponent.SubmitButton.gameObject.SetActive(
          !CommonData.gameWorld.HasPendingEdits &&
          !ScoreUploaded && CommonData.ShowInternetMenus());
    }

    public override void Suspend() {
      HideUI();
      Time.timeScale = 0.0f;
    }

    public override StateExitValue Cleanup() {
      CommonData.gameWorld.ResetMap();
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
        CommonData.mainGame.SelectAndPlayMusic(CommonData.prefabs.menuMusic, true);
        manager.PopState();
      } else if (source == dialogComponent.RetryButton.gameObject) {
        manager.SwapState(new Gameplay(mode));
      } else if (source == dialogComponent.SubmitButton.gameObject) {
        LeaderboardController = LeaderboardController ??
            GameObject.FindObjectOfType<LeaderboardController>() ??
            InstantiateLeaderboardController();
        manager.PushState(new UploadTime(ElapsedGameTime, LeaderboardController));
      } else if (source == dialogComponent.MainButton.gameObject) {
        manager.ClearStack(new MainMenu());
      }
    }

    private LeaderboardController InstantiateLeaderboardController() {
      Debug.Log("Creating LeaderboardController from LevelFinished.");
      return GameObject.Instantiate(dialogComponent.LeaderboardControllerPrefab);
    }
  }
}
