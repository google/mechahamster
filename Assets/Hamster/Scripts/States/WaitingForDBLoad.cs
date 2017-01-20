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
  class WaitingForDBLoad<T> : BaseState {

    protected bool isComplete = false;
    protected bool wasSuccessful = false;
    protected T result = default(T);
    protected string path;

    Firebase.Database.FirebaseDatabase database;

    public WaitingForDBLoad(string path) {
      this.path = path;
    }

    // Initialization method.  Called after the state
    // is added to the stack.
    public override void Initialize() {
      database = Firebase.Database.FirebaseDatabase.GetInstance(CommonData.app);
      database.GetReference(path).GetValueAsync().ContinueWith(HandleResult);
    }

    protected virtual void HandleResult(
        System.Threading.Tasks.Task<Firebase.Database.DataSnapshot> task) {
      if (task.IsFaulted) {
        Debug.LogError("Database exception!  Path = [" + path + "]\n"
          + task.Exception);
      } else if (task.IsCompleted) {
        string json = task.Result.GetRawJsonValue();
        if (json != null) {
          result = JsonUtility.FromJson<T>(json);
          wasSuccessful = true;
        }
      }
      isComplete = true;
    }

    // Called once per frame when the state is active.
    public override void Update() {
      if (isComplete) {
        manager.PopState();
      }
    }

    public override StateExitValue Cleanup() {
      return new StateExitValue(typeof(WaitingForDBLoad<T>), new Results(result, wasSuccessful));
    }

    // Called once per frame for GUI creation, if the state is active.
    public override void OnGUI() {
      GUI.skin = CommonData.prefabs.guiSkin;
      UnityEngine.GUIStyle centeredStyle = GUI.skin.GetStyle("Label");
      centeredStyle.alignment = TextAnchor.UpperCenter;
      GUI.Label(new Rect(Screen.width / 2 - 400,
        Screen.height / 2 - 50, 800, 100), "Loading...", centeredStyle);
    }

    // Class for encapsulating the results of the database load, as
    // well as information about whether the load was successful
    // or not.
    public class Results {
      public T results;
      public bool wasSuccessful;

      public Results(T results, bool wasSuccessful) {
        this.results = results;
        this.wasSuccessful = wasSuccessful;
      }
    }

  }
}
