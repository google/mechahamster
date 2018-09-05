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
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;
using Firebase.Leaderboard;

namespace Hamster.States {

  // State that shows the top finish times for a level.
  class TopTimes : BaseState {

    private Menus.TopTimesGUI menuComponent;
    private static LeaderboardController _leaderboardController;
    private LeaderboardController LeaderboardController {
      get {
        if (_leaderboardController == null) {
          _leaderboardController = GameObject.FindObjectOfType<LeaderboardController>() ??
              menuComponent != null ?
                  InstantiateLeaderboardController() :
                  null;
        }
        return _leaderboardController;
      }
    }

    private List<UserScore> displayTimes = null;
    // The sorted list of times to display.
    public List<UserScore> DisplayTimes {
      get {
        return displayTimes;
      }
      private set {
        if (value == null) {
          displayTimes = null;
        } else {
          displayTimes = new List<UserScore>(value);
          displayTimes.Sort((first, second) => {
            return (int)(first.Score - second.Score);
          });
          SetScoreText();
        }
      }
    }

    public TopTimes(List<UserScore> displayTimes) {
      DisplayTimes = displayTimes;
    }

    public override void Initialize() {
      InitializeUI();
      CommonData.mainCamera.mode = CameraController.CameraMode.Gameplay;
    }

    public override void Resume(StateExitValue results) {
      CommonData.mainCamera.mode = CameraController.CameraMode.Gameplay;
      if (results != null) {
        if (results.sourceState == typeof(WaitForTask)) {
          WaitForTask.Results resultData = results.data as WaitForTask.Results;

          var task =
            resultData.task as System.Threading.Tasks.Task<List<UserScore>>;
          if (task != null) {
            DisplayTimes = task.Result;
          }
        }
      }
      InitializeUI();
    }

    private void InitializeUI() {
      if (menuComponent == null) {
        menuComponent = SpawnUI<Menus.TopTimesGUI>(StringConstants.PrefabsTopTimes);
      }
      var leaderboardController = LeaderboardController;
      leaderboardController.enabled = true;
      ShowUI();
      menuComponent.RecordNames.text = "";
      menuComponent.RecordTimes.text = "";

      leaderboardController.AllScoreDataPath =
          TimeDataUtil.GetDBRankPath(CommonData.gameWorld.worldMap);
      if (leaderboardController.TopScores.Count > 0) {
        DisplayTimes = leaderboardController.TopScores;
      }
      leaderboardController.TopScoresUpdated += UpdateScores;
      leaderboardController.ScoreAdded += AddScore;
      menuComponent.LevelName.text = CommonData.gameWorld.worldMap.name;
    }

    private LeaderboardController InstantiateLeaderboardController() {
      var lc = GameObject.Instantiate(menuComponent.LeaderboardControllerPrefab);
      lc.transform.SetParent(CommonData.mainCamera.transform, false);
      return lc;
    }

    private void UpdateScores(object sender, TopScoreArgs args) {
      DisplayTimes = args.TopScores;
    }

    private void AddScore(object sender, UserScoreArgs args) {
      DisplayTimes = LeaderboardController.TopScores;
    }

    private void SetScoreText() {
      menuComponent.RecordNames.text = "";
      menuComponent.RecordTimes.text = "";
      foreach (var score in DisplayTimes) {
        menuComponent.RecordNames.text += score.Username + "\n";
        menuComponent.RecordTimes.text +=
            Utilities.StringHelper.FormatTime(score.Score) + " s\n";
      }
    }

    public override void Suspend() {
      CommonData.mainCamera.mode = CameraController.CameraMode.Menu;
      var leaderboardController = LeaderboardController;
      leaderboardController.TopScoresUpdated -= UpdateScores;
      leaderboardController.ScoreAdded -= AddScore;
      HideUI();
    }

    public override StateExitValue Cleanup() {
      DestroyUI();
      CommonData.mainCamera.mode = CameraController.CameraMode.Menu;
      var leaderboardController = LeaderboardController;
      leaderboardController.TopScoresUpdated -= UpdateScores;
      leaderboardController.ScoreAdded -= AddScore;
      return new StateExitValue(typeof(TopTimes));
    }

    public override void HandleUIEvent(GameObject source, object eventData) {
      if (source == menuComponent.BackButton.gameObject) {
        manager.PopState();
      } else if (source == menuComponent.TimeFrameButton30Days.gameObject) {
        LeaderboardController.Interval =
            60 /* seconds */ *
            60 /* minutes */ *
            24 /* hours */ *
            30 /* days */;
        menuComponent.TimeFrame30DaysText.fontStyle = FontStyle.Bold;
        menuComponent.TimeFrameAllTimeText.fontStyle = FontStyle.Normal;
      } else if (source == menuComponent.TimeFrameButtonAllTime.gameObject) {
        LeaderboardController.Interval = 0;
        menuComponent.TimeFrame30DaysText.fontStyle = FontStyle.Normal;
        menuComponent.TimeFrameAllTimeText.fontStyle = FontStyle.Bold;
      }
    }
  }
}
