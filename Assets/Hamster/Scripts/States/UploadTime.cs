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
using System.Collections.Generic;
using System;
using Firebase.Leaderboard;

namespace Hamster.States {

  // State used to upload the time taken to beat the current level to the database.
  class UploadTime : BaseState {
    // The time that will be saved.
    public long Time { get; private set; }

    // Whether the time was uploaded, used during cleanup.
    private bool TimeUploaded { get; set; }

    private static LeaderboardController LeaderboardController;

    public UploadTime(long time, LeaderboardController leaderboardController) {
      Time = time;
      LeaderboardController = leaderboardController;
      LeaderboardController.enabled = true;
      LeaderboardController.AllScoreDataPath =
          TimeDataUtil.GetDBRankPath(CommonData.gameWorld.worldMap);
    }

    public override void Initialize() {
      CommonData.mainCamera.mode = CameraController.CameraMode.Gameplay;
      TimeUploaded = false;

      Firebase.Analytics.FirebaseAnalytics.LogEvent(StringConstants.AnalyticsEventTimeUploadStarted,
        StringConstants.AnalyticsParamMapId, CommonData.gameWorld.worldMap.mapId);

      manager.PushState(new WaitForTask(
        TimeDataUtil.UploadReplay(Time,
          CommonData.gameWorld.worldMap, CommonData.gameWorld.PreviousReplayData)
              .ContinueWith(task => LeaderboardController.AddScore(task.Result)),
          StringConstants.UploadTimeTitle, true));
    }

    public override void Resume(StateExitValue results) {
      CommonData.mainCamera.mode = CameraController.CameraMode.Gameplay;
      if (results != null) {
        if (results.sourceState == typeof(WaitForTask)) {
          WaitForTask.Results resultData = results.data as WaitForTask.Results;

          if (resultData.task.IsFaulted) {
            Debug.LogException(resultData.task.Exception);
            manager.SwapState(new BasicDialog(StringConstants.UploadError));
            return;
          }

          TimeUploaded = true;
          Firebase.Analytics.FirebaseAnalytics.LogEvent(
            StringConstants.AnalyticsEventTimeUploadFinished,
            StringConstants.AnalyticsParamMapId, CommonData.gameWorld.worldMap.mapId);

          // Show the top times for the level, highlighting which one was just uploaded.
          var task =
            resultData.task as System.Threading.Tasks.Task<List<UserScore>>;
          List<UserScore> times = null;
          if (task != null) {
            times = task.Result;
          }
          manager.SwapState(new TopTimes(times));
          return;
        }
      }
      // This is a passthrough state, so if we are still the top, pop ourselves.
      if (manager.CurrentState() == this) {
        manager.PopState();
      }
    }

    public override void Suspend() {
      CommonData.mainCamera.mode = CameraController.CameraMode.Menu;
    }

    public override StateExitValue Cleanup() {
      CommonData.mainCamera.mode = CameraController.CameraMode.Menu;
      return new StateExitValue(typeof(UploadTime), TimeUploaded);
    }
  }
}
