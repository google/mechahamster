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
  // State for displaying fatal errors.
  // Displays a basic dialog, and when that's completed, quits the application.
  class FatalError : BaseState {

    string errorText;

    public FatalError(string errorText) {
      this.errorText = errorText;
    }

    // Initialization method.  Called after the state
    // is added to the stack.
    public override void Initialize() {
      manager.PushState(new BasicDialog(errorText));
    }

    public override void Resume(StateExitValue results) {
      // After we have shown the dialog, quit.
      Application.Quit();
    }
  }
}
