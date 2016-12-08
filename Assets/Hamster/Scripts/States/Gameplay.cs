using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Firebase.Unity.Editor;


namespace Hamster.States {
  class Gameplay : BaseState {

    // Initialization method.  Called after the state
    // is added to the stack.
    public override void Initialize() {
      Time.timeScale = 1.0f;
    }

    // Resume the state.  Called when the state becomes active
    // when the state above is removed.  That state may send an
    // optional object containing any results/data.  Results
    // can also just be null, if no data is sent.
    public override void Resume(StateExitValue results) {
      Time.timeScale = 1.0f;
    }

    // Called once per frame when the state is active.
    public override void Update() {
      if (Input.GetKeyDown(KeyCode.Escape)) {
        manager.PopState();
        return;
      }
    }

    // Called once per frame for GUI creation, if the state is active.
    public override void OnGUI() { }
  }
}
