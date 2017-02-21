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

using System.Collections.Generic;
using UnityEngine;

namespace Hamster.States {

  // State used to upload the time taken to beat the current level to the database.
  class UploadTime : BaseState {
    // Width/Height of the menu, expressed as a portion of the screen width:
    const float MenuWidth = 0.40f;
    const float MenuHeight = 0.75f;

    // The name the time will be saved under.
    public string Name { get; private set; }
    // The time that will be saved.
    public long Time { get; private set; }

    // Whether the time was uploaded, used during cleanup.
    private bool TimeUploaded { get; set; }

    // The time data that is being uploaded.
    public TimeData UploadedTimeData { get; private set; }

    public UploadTime(long time) {
      Name = PlayerPrefs.GetString(StringConstants.UploadScoreNameKey,
        StringConstants.UploadScoreDefaultName);
      Time = time;
    }

    public override void Initialize() {
      CommonData.mainCamera.mode = CameraController.CameraMode.Gameplay;
      TimeUploaded = false;

      Firebase.Analytics.FirebaseAnalytics.LogEvent(StringConstants.AnalyticsEventTimeUploadStarted,
        StringConstants.AnalyticsParamMapId, CommonData.gameWorld.worldMap.mapId);
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
            resultData.task as System.Threading.Tasks.Task<List<TimeData>>;
          List<TimeData> times = null;
          if (task != null) {
            times = task.Result;
          }
          manager.SwapState(new TopTimes(times, UploadedTimeData));
        }
      }
    }

    public override void Suspend() {
      CommonData.mainCamera.mode = CameraController.CameraMode.Menu;
    }

    public override StateExitValue Cleanup() {
      CommonData.mainCamera.mode = CameraController.CameraMode.Menu;
      // Save what the user typed into the name for next time.
      PlayerPrefs.SetString(StringConstants.UploadScoreNameKey, Name);
      return new StateExitValue(typeof(UploadTime), TimeUploaded);
    }

    public override void OnGUI() {
      float menuWidth = MenuWidth * Screen.width;
      float menuHeight = MenuHeight * Screen.height;
      GUI.skin = CommonData.prefabs.guiSkin;

      GUILayout.BeginArea(
          new Rect((Screen.width - menuWidth) / 2, (Screen.height - menuHeight) / 2,
          menuWidth, menuHeight));

      GUILayout.Label(StringConstants.UploadTimeTitle);

      Name = GUILayout.TextField(Name, 16);
      GUILayout.Label(string.Format(StringConstants.FinishedTimeText,
        Utilities.StringHelper.FormatTime(Time)));

      GUILayout.BeginVertical();

      GUILayout.Space(20);

      if (GUILayout.Button(StringConstants.ButtonUploadTime)) {
        UploadedTimeData = new TimeData(Name, Time);
        manager.PushState(new WaitForTask(
          UploadedTimeData.UploadTime(CommonData.gameWorld.worldMap),
            StringConstants.UploadTimeTitle, true));
      }
      if (GUILayout.Button(StringConstants.ButtonCancel)) {
        TimeUploaded = false;
        manager.PopState();
      }
      GUILayout.EndVertical();

      GUILayout.EndArea();
    }
  }
}
