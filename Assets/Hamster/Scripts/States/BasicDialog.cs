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

    // Initialization method.  Called after the state
    // is added to the stack.
    public override void Initialize() {
      Time.timeScale = 0.0f;
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
