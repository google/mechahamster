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
  // State for license dialogs.  They contain a long text field,
  // with the license information, and a single button to return
  // to the previous state.
  class LicenseDialog : BaseState {

    string dialogText;
    Menus.LongTextDialogGUI dialogComponent;
    string LicenseFileName = "StandaloneLicense";
    int secretButtonPressCount = 0;
    const int SecretButtonThreshold = 4;

    public LicenseDialog() {
      TextAsset license = Resources.Load(LicenseFileName, typeof(TextAsset)) as TextAsset;
      dialogText = license.text;
    }

    public override void Initialize() {
      dialogComponent = SpawnUI<Menus.LongTextDialogGUI>(StringConstants.PrefabLicenseDialog);
      dialogComponent.LongText.SpawnText(dialogText);
      dialogComponent.ScrollRect.verticalNormalizedPosition = 1.0f;
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
        // If they pressed the secret button exactly the right number of times,
        // launch the profiling tool.
        if (secretButtonPressCount == SecretButtonThreshold) {
          manager.SwapState(new FullLevelTest());
        } else {
          manager.PopState();
        }
      } else if (source == dialogComponent.SecretButton.gameObject) {
        secretButtonPressCount++;
      }
    }
  }
}
