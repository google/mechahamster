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
  class BasicDialog : BaseState {

    string dialogText;
    Menus.BasicDialogGUI dialogComponent;

    public BasicDialog(string dialogText) {
      this.dialogText = dialogText;
    }

    public override void Initialize() {
      dialogComponent = SpawnUI<Menus.BasicDialogGUI>(StringConstants.PrefabBasicDialog);
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
      return null;
    }

    public override void HandleUIEvent(GameObject source, object eventData) {
      if (source == dialogComponent.OkayButton.gameObject) {
        manager.PopState();
      }
    }
  }
}
