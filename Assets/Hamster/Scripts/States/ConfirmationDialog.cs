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
  // State for basic dialog boxes on the screen.
  // Simply displays a message, and waits for the user to click the
  // button, and then returns to the previous state.
  class ConfirmationDialog : BaseState {

    string dialogTitle;
    string dialogText;
    // An optional ID to identify the state, in case the state requesting
    // this menu has more than one  "are you sure?" style prompt.
    string dialogId;
    Menus.ConfirmationDialogGUI dialogComponent;
    bool userIsSure = false;

    // An optional pair of states that we should jump to if
    // the user says "yes" or "no" to the dialog.  Makes it easier
    // to implement the common use-case of asking "are you sure you
    // want to do X?" and then jump to a new state based on the answer.
    BaseState NewStateIfConfirmed;
    BaseState NewStateIfNotConfirmed;

    public ConfirmationDialog(string dialogTitle, string dialogText,
        BaseState NewStateIfConfirmed = null, BaseState NewStateIfNotConfirmed = null) {
      ConstructorHelper(dialogTitle, dialogText, "",
          NewStateIfConfirmed, NewStateIfNotConfirmed);
    }

    public ConfirmationDialog(string dialogTitle, string dialogText, string dialogId = "",
        BaseState NewStateIfConfirmed = null, BaseState NewStateIfNotConfirmed = null) {
      ConstructorHelper(dialogTitle, dialogText, dialogId,
          NewStateIfConfirmed, NewStateIfNotConfirmed);
    }

    // Setup function used by the constructors.
    private void ConstructorHelper(string dialogTitle, string dialogText, string dialogId = "",
        BaseState NewStateIfConfirmed = null, BaseState NewStateIfNotConfirmed = null) {
      this.dialogTitle = dialogTitle;
      this.dialogText = dialogText;
      this.dialogId = dialogId;
      this.NewStateIfConfirmed = NewStateIfConfirmed;
      this.NewStateIfNotConfirmed = NewStateIfNotConfirmed;
    }

    public override void Initialize() {
      dialogComponent =
          SpawnUI<Menus.ConfirmationDialogGUI>(StringConstants.PrefabConfirmationDialog);
      dialogComponent.DialogTitle.text = dialogTitle;
      dialogComponent.DialogText.text = dialogText;
    }

    public override void Resume(StateExitValue results) {
      ShowUI();
    }

    public override void Suspend() {
      HideUI();
    }

    public override StateExitValue Cleanup() {
      DestroyUI();
      return new StateExitValue(typeof(ConfirmationDialog), new Results(userIsSure, dialogId));
    }

    public override void HandleUIEvent(GameObject source, object eventData) {
      if (source == dialogComponent.OkayButton.gameObject) {
        userIsSure = true;
        if (NewStateIfConfirmed != null) {
          manager.SwapState(NewStateIfConfirmed);
        } else {
          manager.PopState();
        }
      }
      if (source == dialogComponent.CancelButton.gameObject) {
        userIsSure = false;
        if (NewStateIfNotConfirmed != null) {
          manager.SwapState(NewStateIfNotConfirmed);
        } else {
          manager.PopState();
        }
      }
    }

    // Class for encapsulating the results of the confirmation
    // dialog, as well as an identifier that can be used, if a
    // state uses multile confirmation dialogs.
    public class Results {
      public bool UserIsSure;
      public string dialogId;

      public Results(bool UserIsSure, string dialogId) {
        this.UserIsSure = UserIsSure;
        this.dialogId = dialogId;
      }
    }

  }
}
