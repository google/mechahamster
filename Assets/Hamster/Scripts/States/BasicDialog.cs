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

    Vector2 scrollViewPosition;
    string dialogText;
    string buttonText;

    public BasicDialog(string dialogText, string buttonText = "Okay") {
      this.dialogText = dialogText;
      this.buttonText = buttonText;
    }

    // Called once per frame for GUI creation, if the state is active.
    // TODO(ccornell): This needs some layout attention, if it's going
    // to see much use.  Needs to be centered at a minimum.
    public override void OnGUI() {
      GUI.skin = CommonData.prefabs.guiSkin;
      GUILayout.BeginVertical();
      scrollViewPosition = GUILayout.BeginScrollView(scrollViewPosition);
      GUILayout.Label(dialogText);
      GUILayout.EndScrollView();
      if (GUILayout.Button(buttonText)) {
        manager.PopState();
      }
      GUILayout.EndVertical();
    }
  }
}
