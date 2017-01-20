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
  // Basic account management GUI.  Allows you to add
  // an email to your anonymous account, or to log out.
  class ManageAccount : BaseState {

    string message = "";

    Firebase.Auth.FirebaseAuth auth;

    public override void Resume(StateExitValue results) {
      if (results != null) {
        if (results.sourceState == typeof(WaitForTask)) {
          WaitForTask.Results taskResults = results.data as WaitForTask.Results;
          if (taskResults.task.IsFaulted) {
            message = "Could not sign in: " + taskResults.task.Exception.ToString();
          } else {
            manager.PopState();
          }
        }
      }
    }

    // Initialization method.  Called after the state
    // is added to the stack.
    public override void Initialize() {
      auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
    }

    // Called once per frame for GUI creation, if the state is active.
    // TODO(ccornell): This needs some layout attention, if it's going
    // to see much use.  Needs to be centered at a minimum.
    public override void OnGUI() {

      GUI.skin = CommonData.prefabs.guiSkin;
      GUILayout.BeginVertical();

      // Anonymous account:
      if (auth.CurrentUser.IsAnonymous) {
        GUILayout.Label(StringConstants.LabelAnonymousAccount);
        if (GUILayout.Button(StringConstants.ButtonAddEmailPassword)) {
          manager.PushState(new AddEmailToAccount());
        }
      } else {
        GUILayout.Label(StringConstants.LabelPasswordAccount + auth.CurrentUser.Email);
      }

      if (GUILayout.Button(StringConstants.ButtonLogout)) {
        auth.SignOut();
        // TODO(ccornell): if anonymous, clear out their user data
        // and any maps they've saved, so our DB doesn't get orphans.
        manager.ClearStack(new Startup());
      }

      if (GUILayout.Button(StringConstants.ButtonOkay)) {
        manager.PopState();
      }
    }
  }
}
