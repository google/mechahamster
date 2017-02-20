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
    // Width/Height of the menu, expressed as a portion of the screen width:
    const float MenuWidth = 0.40f;
    const float MenuHeight = 0.75f;

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
    // The optional time to highlight (usually the just entered time).
    public TimeData Highlight { get; private set; }

    public TopTimes(List<TimeData> displayTimes, TimeData highlight = null) {
      DisplayTimes = displayTimes;
      Highlight = highlight;
    }

    public override void Initialize() {
      // If no display times were provided, retrieve them from the database.
      if (DisplayTimes == null) {
        manager.PushState(new WaitForTask(
          TimeData.GetTopTimes(CommonData.gameWorld.worldMap)));
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
    }

    public override void Suspend() {
      CommonData.mainCamera.mode = CameraController.CameraMode.Menu;
    }

    public override StateExitValue Cleanup() {
      CommonData.mainCamera.mode = CameraController.CameraMode.Menu;
      return new StateExitValue(typeof(TopTimes));
    }

    public override void OnGUI() {
      GUI.skin = CommonData.prefabs.guiSkin;

      GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));

      // Center the GUI
      GUIStyle centeredStyle = GUI.skin.GetStyle("Label");
      centeredStyle.alignment = TextAnchor.UpperCenter;

      GUILayout.BeginVertical();
      GUILayout.Label(StringConstants.TopTimesTitle);
      if (DisplayTimes != null) {
        // Display the names and times in two columns.
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        int highlightIndex = -1;
        for (int i = 0; i < DisplayTimes.Count; ++i) {
          // Find the index of the time to highlight, if there is one.
          if (highlightIndex < 0 && Highlight != null &&
              DisplayTimes[i].name == Highlight.name &&
              DisplayTimes[i].time == Highlight.time) {
            highlightIndex = i;
          }
          string name = DisplayTimes[i].name;
          if (highlightIndex == i) {
            name = string.Format(StringConstants.TopTimesHighlight, name);
          }
          GUILayout.Label(name);
        }
        // If the highlight score wasn't in the list, add it at the end.
        if (Highlight != null && highlightIndex == -1) {
          GUILayout.Label(string.Format(StringConstants.TopTimesHighlight, Highlight.name));
        }
        GUILayout.EndVertical();

        GUILayout.BeginVertical();
        for (int i = 0; i < DisplayTimes.Count; ++i) {
          string time = Utilities.StringHelper.FormatTime(DisplayTimes[i].time) + " s";
          if (highlightIndex == i) {
            time = string.Format(StringConstants.TopTimesHighlight, time);
          }
          GUILayout.Label(time);
        }
        if (Highlight != null && highlightIndex == -1) {
          GUILayout.Label(string.Format(StringConstants.TopTimesHighlight,
            Utilities.StringHelper.FormatTime(Highlight.time) + " s"));
        }
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
      }

      GUILayout.BeginHorizontal();
      GUILayout.FlexibleSpace();
      if (GUILayout.Button(StringConstants.ButtonOkay)) {
        manager.PopState();
      }
      GUILayout.FlexibleSpace();
      GUILayout.EndHorizontal();
      GUILayout.EndVertical();

      GUILayout.EndArea();
    }
  }
}
