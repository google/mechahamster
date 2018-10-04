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
using Firebase.Auth;
using Firebase.Database;

namespace Hamster.States {
  // Utility state for deleting a user account.  Displays a
  // message confirming success/failure, and then either
  // swaps itself to manageAccount (if successful) or pops
  // itself off the stack.
  class DeleteAccount : BaseState {

    protected bool isComplete = false;
    protected bool wasSuccessful = false;
    protected string userId;

    public DeleteAccount() {
    }

    public override void Initialize() {
      userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
      manager.PushState(new WaitForTask(FirebaseAuth.DefaultInstance.CurrentUser.DeleteAsync(),
          StringConstants.DeleteAccountMessage, true));
    }

    public override void Resume(StateExitValue results) {
      ShowUI();
      if (results != null) {
        if (results.sourceState == typeof(WaitForTask)) {
          WaitForTask.Results taskResults = results.data as WaitForTask.Results;
          if (taskResults.task.IsFaulted) {
            manager.SwapState(new ManageAccount());
            manager.PushState(new BasicDialog(StringConstants.DeleteAccountFail +
                taskResults.task.Exception.ToString()));
          } else {
            // Delete the user's profile data:
            string path = CommonData.DBUserTablePath + userId;
            FirebaseDatabase database = FirebaseDatabase.GetInstance(CommonData.app);

            // Delete all maps associated with this user:
            database.GetReference(path).SetValueAsync(null);
            foreach (MapListEntry map in CommonData.currentUser.data.maps) {
              path = CommonData.DBMapTablePath + map.mapId;
              database.GetReference(path).SetValueAsync(null);
            }
            GooglePlayServicesSignIn.SignOut();
            SignInState.SetState(SignInState.State.SignedOut);

            manager.SwapState(new ChooseSignInMenu());
            manager.PushState(new BasicDialog(StringConstants.DeleteAccountSuccess));
          }
        }
      }
    }
  }
}
