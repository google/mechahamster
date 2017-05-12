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
  // State for requesting username/password from the user for
  // adding email to an existing account.
  class AddEmail : BaseState {

    Firebase.Auth.FirebaseAuth auth;
    Menus.AddEmailGUI dialogComponent;

    public override void Initialize() {
      auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
      dialogComponent = SpawnUI<Menus.AddEmailGUI>(StringConstants.PrefabsAddEmailMenu);
    }

    public override void Suspend() {
      HideUI();
    }

    public override void Resume(StateExitValue results) {
      ShowUI();
      if (results != null) {
        WaitForTask.Results taskResults = results.data as WaitForTask.Results;
        if (taskResults != null && taskResults.task.IsFaulted) {
          manager.PushState(new BasicDialog(
              Utilities.StringHelper.SigninInFailureString(taskResults.task)));
        } else {
          manager.PopState();
        }
      }
    }

    public override StateExitValue Cleanup() {
      DestroyUI();
      return new StateExitValue(typeof(AddEmail), null);
    }

    public override void HandleUIEvent(GameObject source, object eventData) {
      if (source == dialogComponent.CancelButton.gameObject) {
        manager.PopState();
      } else if (source == dialogComponent.ContinueButton.gameObject) {
        Firebase.Auth.Credential credential =
            Firebase.Auth.EmailAuthProvider.GetCredential(
            dialogComponent.Email.text, dialogComponent.Password.text);
        manager.PushState(new WaitForTask(auth.CurrentUser.LinkWithCredentialAsync(credential),
            StringConstants.LabelLinkingAccount, true));
      }
    }
  }
}
