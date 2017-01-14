using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Firebase.Unity.Editor;


namespace Hamster.States {
  // Startup state - handles one-time startup tasks, before kicking off the main
  // menu state.
  class Startup : BaseState {

    bool isComplete = false;
    string UserId;

    public Startup(string UserId) {
      this.UserId = UserId;
    }

    // Initialization method.  Called after the state
    // is added to the stack.
    public override void Initialize() {
      Time.timeScale = 0.0f;
      // When the game starts up, it needs to either download the user data
      // or create a new profile.
      manager.PushState(new States.FetchUserData(UserId));
    }

    public override void Resume(StateExitValue results) {
      Time.timeScale = 0.0f;
      manager.SwapState(new States.MainMenu());
    }

    // Called once per frame when the state is active.
    public override void Update() {
      if (isComplete) {
        manager.PopState();
      }
    }
  }
}
