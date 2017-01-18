using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Firebase.Unity.Editor;


namespace Hamster.States {
  // Startup state - handles one-time startup tasks, before kicking off the main
  // menu state.
  class Startup : BaseState {

    Firebase.Auth.FirebaseAuth auth;

    const string DefaultDesktopID = "XYZZY";

    public Startup() {
    }

    // Initialization method.  Called after the state
    // is added to the stack.
    public override void Initialize() {
      Time.timeScale = 0.0f;
      // When the game starts up, it needs to either download the user data
      // or create a new profile.
      auth = Firebase.Auth.FirebaseAuth.DefaultInstance;

# if (UNITY_EDITOR)
      // On desktop, we just use the default user id.
      manager.PushState(new States.FetchUserData(DefaultDesktopID));
#else
      // On mobile, we go through the full auth system, to request a user id.
      // If we need to sign in, do that.  Otherwise, if we know who we are,
      // so fetch the user data.
      if (auth.CurrentUser == null) {
        manager.PushState(new SignInMenu());
      } else {
        manager.PushState(new States.FetchUserData(auth.CurrentUser.UserId));
      }
#endif
    }

    // Got back from either fetching userdata from the database, or
    // requesting an anonymous login.
    // If we got back from login, request/start a user with that ID.
    // If we got back from fetching user data, start the game.
    public override void Resume(StateExitValue results) {
      Time.timeScale = 0.0f;

      if (results.sourceState == typeof(SignInMenu)) {
          // We just got back from trying to sign in anonymously.
          // Did it work?
          if (auth.CurrentUser != null) {
          // Yes!  Continue!
          manager.PushState(new FetchUserData(auth.CurrentUser.UserId));
        } else {
          // Nope.  Problems.
          manager.PushState(new FatalError("Could not log in anonymously."));
        }
      }
      if (results.sourceState == typeof(FetchUserData))
        // Just got back from fetching the user.
        // Did THAT work?
        if (CommonData.currentUser.data != null) {
          // Yes.  Ready to start!
          manager.SwapState(new States.MainMenu());
        } else {
          // Nope.  Problems.
          manager.PushState(new FatalError("Could not fetch or create user data."));
        }
    }
  }
}
