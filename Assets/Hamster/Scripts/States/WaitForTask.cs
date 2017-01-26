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
  // Utility state, for fetching structures from the database.
  // Returns the result in the result struct.
  class WaitForTask : BaseState {

    static int LabelHeight = 100;
    static int LabelWidth = 800;

    protected bool isComplete = false;
    string waitText;
    System.Threading.Tasks.Task task;

    public WaitForTask(System.Threading.Tasks.Task task, string waitText = "") {
      this.waitText = waitText;
      this.task = task;
    }

    // Called once per frame when the state is active.
    public override void Update() {
      if (task.IsCompleted) {
        manager.PopState();
      }
    }

    public override StateExitValue Cleanup() {
      return new StateExitValue(typeof(WaitForTask), new Results(task));
    }

    // Called once per frame for GUI creation, if the state is active.
    public override void OnGUI() {
      GUI.skin = CommonData.prefabs.guiSkin;
      UnityEngine.GUIStyle centeredStyle = GUI.skin.GetStyle("Label");
      centeredStyle.alignment = TextAnchor.UpperCenter;
      GUI.Label(new Rect(Screen.width / 2 - LabelWidth/2,
        Screen.height / 2 - LabelHeight/2, LabelWidth, LabelHeight), waitText, centeredStyle);
    }

    // Class for encapsulating the results of the database load, as
    // well as information about whether the load was successful
    // or not.
    public class Results {
      public System.Threading.Tasks.Task task;

      public Results(System.Threading.Tasks.Task task) {
        this.task = task;
      }
    }
  }
}
