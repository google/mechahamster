using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Firebase.Unity.Editor;


namespace Hamster.States {
  // Basic account management GUI.  Allows you to add
  // an email to your anonymous account, or to log out.
  class ManageAccount : BaseState {

    string message = "";

    Firebase.Auth.FirebaseAuth auth;

    public override void Resume(StateExitValue results) {
      if (results != null) {
        if (results.sourceState == typeof(WaitForTask)) {
          WaitForTask.Results taskResults = results.data as WaitForTask.Results;
          if (taskResults.task.IsFaulted) {
            message = "Could not sign in: " + taskResults.task.Exception.ToString();
          } else {
            manager.PopState();
          }
        }
      }
    }

    // Initialization method.  Called after the state
    // is added to the stack.
    public override void Initialize() {
      auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
      Time.timeScale = 0.0f;
    }

    // Called once per frame for GUI creation, if the state is active.
    // TODO(ccornell): This needs some layout attention, if it's going
    // to see much use.  Needs to be centered at a minimum.
    public override void OnGUI() {

      GUI.skin = CommonData.prefabs.guiSkin;
      GUILayout.BeginVertical();

      // Anonymous account:
      if (auth.CurrentUser.IsAnonymous) {
        GUILayout.Label(StringConstants.LabelAnonymousAccount);
        if (GUILayout.Button(StringConstants.ButtonAddEmailPassword)) {
          manager.PushState(new AddEmailToAccount());
        }
      } else {
        GUILayout.Label(StringConstants.LabelPasswordAccount + auth.CurrentUser.Email);
      }

      if (GUILayout.Button(StringConstants.ButtonLogout)) {
        auth.SignOut();
        // TODO(ccornell): if anonymous, clear out their user data
        // and any maps they've saved, so our DB doesn't get orphans.
        manager.ClearStack(new Startup());
      }

      if (GUILayout.Button(StringConstants.ButtonOkay)) {
        manager.PopState();
      }
    }
  }
}
