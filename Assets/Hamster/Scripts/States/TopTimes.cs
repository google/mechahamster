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

namespace Hamster.States {

  // State that shows the top finish times for a level.
  class TopTimes : BaseState {

    private Menus.TopTimesGUI menuComponent;

    private List<TimeData> displayTimes = null;
    // The sorted list of times to display.
    public List<TimeData> DisplayTimes {
      get {
        return displayTimes;
      }
      private set {
        if (value == null) {
          displayTimes = null;
        } else {
          displayTimes = new List<TimeData>(value);
          displayTimes.Sort((first, second) => {
            return (int)(first.time - second.time);
          });
        }
      }
    }

    public TopTimes(List<TimeData> displayTimes) {
      DisplayTimes = displayTimes;
    }

    public override void Initialize() {
      // If no display times were provided, retrieve them from the database.
      if (DisplayTimes == null) {
        manager.PushState(new WaitForTask(
          TimeData.GetTopTimes(CommonData.gameWorld.worldMap)));
      } else {
        InitializeUI();
      }
      CommonData.mainCamera.mode = CameraController.CameraMode.Gameplay;
    }

    public override void Resume(StateExitValue results) {
      CommonData.mainCamera.mode = CameraController.CameraMode.Gameplay;
      if (results != null) {
        if (results.sourceState == typeof(WaitForTask)) {
          WaitForTask.Results resultData = results.data as WaitForTask.Results;

          var task =
            resultData.task as System.Threading.Tasks.Task<List<TimeData>>;
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
      ShowUI();

      menuComponent.LevelName.text = CommonData.gameWorld.worldMap.name;
      menuComponent.RecordNames.text = "";
      menuComponent.RecordTimes.text = "";
      foreach (TimeData timeData in DisplayTimes) {
        menuComponent.RecordNames.text += timeData.name + "\n";
        menuComponent.RecordTimes.text +=
          Utilities.StringHelper.FormatTime(timeData.time) + " s\n";
      }
    }

    public override void Suspend() {
      CommonData.mainCamera.mode = CameraController.CameraMode.Menu;
      HideUI();
    }

    public override StateExitValue Cleanup() {
      DestroyUI();
      CommonData.mainCamera.mode = CameraController.CameraMode.Menu;
      return new StateExitValue(typeof(TopTimes));
    }

    public override void HandleUIEvent(GameObject source, object eventData) {
      if (source == menuComponent.BackButton.gameObject) {
        manager.PopState();
      }
    }
  }
}
