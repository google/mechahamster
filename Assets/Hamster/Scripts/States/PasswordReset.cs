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
  // State for requesting an email from the user, for the purposes
  // of letting them reset their password.
  class PasswordReset : BaseState {

    Firebase.Auth.FirebaseAuth auth;
    Menus.PasswordResetGUI dialogComponent;
    bool canceled = false;

    public override void Initialize() {
      auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
      dialogComponent = SpawnUI<Menus.PasswordResetGUI>(StringConstants.PrefabsPasswordResetMenu);
    }

    public override void Suspend() {
      HideUI();
    }

    public override void Resume(StateExitValue results) {
      ShowUI();
      if (results != null) {
        if (results.sourceState == typeof(WaitForTask)) {
          WaitForTask.Results taskResults = results.data as WaitForTask.Results;
          if (taskResults.task.IsFaulted) {
            manager.SwapState(new BasicDialog(StringConstants.SignInPasswordResetError));
          } else {
            manager.SwapState(new BasicDialog(string.Format(
                StringConstants.SignInPasswordReset, dialogComponent.Email.text)));
          }
        }
      }
    }

    public override StateExitValue Cleanup() {
      DestroyUI();
      return new StateExitValue(typeof(SignInWithEmail), new SignInResult(canceled));
    }

    public override void HandleUIEvent(GameObject source, object eventData) {
      if (source == dialogComponent.CancelButton.gameObject) {
        canceled = true;
        manager.PopState();
      } else if (source == dialogComponent.ContinueButton.gameObject) {
        manager.PushState(new WaitForTask(
            auth.SendPasswordResetEmailAsync(dialogComponent.Email.text)));
      }
    }
  }
}
