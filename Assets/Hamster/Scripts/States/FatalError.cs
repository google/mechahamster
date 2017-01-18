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
      Time.timeScale = 0.0f;
      manager.PushState(new BasicDialog(errorText));
    }

    public override void Resume(StateExitValue results) {
      // After we have shown the dialog, quit.
      Application.Quit();
    }
  }
}
