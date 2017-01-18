using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Firebase.Unity.Editor;


namespace Hamster.States {
  // State for asking how the user wants to sign in -
  // Through an auth provider, creating an account, or
  // doing an anonymous signin for later.
  class SignInMenu : BaseState {

    Firebase.Auth.FirebaseAuth auth;

    bool signedIn = false;

    public override StateExitValue Cleanup() {
      return new StateExitValue(typeof(SignInMenu), null);
    }

    public override void Resume(StateExitValue results) {
#if (UNITY_EDITOR)
      manager.PopState();
#else
      if (auth.CurrentUser != null) {
        manager.PopState();
      } else {
        manager.PushState(new BasicDialog("Error signing in."));
      }
#endif
    }

    // Initialization method.  Called after the state
    // is added to the stack.
    public override void Initialize() {
      Time.timeScale = 0.0f;

      auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
    }

    // Called once per frame for GUI creation, if the state is active.
    // TODO(ccornell): This needs some layout attention, if it's going
    // to see much use.  Needs to be centered at a minimum.
    public override void OnGUI() {
      GUI.skin = CommonData.prefabs.guiSkin;
      GUILayout.BeginVertical();
      if (GUILayout.Button(StringConstants.ButtonSignInWithEmail)) {
        manager.PushState(new SignInEmail());
      }
      if (GUILayout.Button(StringConstants.ButtonCreateAccount)) {
        manager.PushState(new CreateAccount());
      }
      if (GUILayout.Button(StringConstants.ButtonSignInAnonymously)) {
        manager.PushState(
            new WaitForTask(auth.SignInAnonymouslyAsync(), StringConstants.LabelSigningIn));
      }
      GUILayout.EndVertical();
    }
  }
}
