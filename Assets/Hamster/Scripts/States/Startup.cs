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
  // Startup state - handles one-time startup tasks, before kicking off the main
  // menu state.
  class Startup : BaseState {

    Firebase.Auth.FirebaseAuth auth;

    // Initialization method.  Called after the state
    // is added to the stack.
    public override void Initialize() {
      Time.timeScale = 0.0f;
      // When the game starts up, it needs to either download the user data
      // or create a new profile.
      auth = Firebase.Auth.FirebaseAuth.DefaultInstance;

      // We go through the full auth system, to request a user id.
      // If we need to sign in, do that.  Otherwise, if we know who we are,
      // so fetch the user data.
      if (auth.CurrentUser == null) {
        if (CommonData.inVrMode || CommonData.testLab.IsTestingScenario) {
              manager.PushState(new WaitForTask(auth.SignInAnonymouslyAsync(),
                  StringConstants.LabelSigningIn, true));
        } else {
          if (GooglePlayServicesSignIn.CanAutoSignIn()) {
            manager.PushState(
              new WaitForTask(GooglePlayServicesSignIn.SignIn(),
                StringConstants.LabelSigningIn, true));
          } else {
            manager.PushState(new ChooseSignInMenu());
          }
        }
      } else {
        manager.PushState(new States.FetchUserData(auth.CurrentUser.UserId));
      }
    }

    // Start gathering analytic data.
    void InitializeAnalytics() {
      Firebase.Analytics.FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);

      // Set the user's sign up method.
      Firebase.Analytics.FirebaseAnalytics.SetUserProperty(
        Firebase.Analytics.FirebaseAnalytics.UserPropertySignUpMethod,
        "Google");

      if (CommonData.currentUser != null)
        Firebase.Analytics.FirebaseAnalytics.SetUserId(CommonData.currentUser.data.id);
    }

    // Got back from either fetching userdata from the database, or
    // requesting an anonymous login.
    // If we got back from login, request/start a user with that ID.
    // If we got back from fetching user data, start the game.
    public override void Resume(StateExitValue results) {
      if (results.sourceState == typeof(ChooseSignInMenu) ||
          results.sourceState == typeof(WaitForTask)) {
        // We just got back from trying to sign in anonymously.
        // Did it work?
        if (auth.CurrentUser != null) {
          // Yes!  Continue!
          manager.PushState(new FetchUserData(auth.CurrentUser.UserId));
        } else {
          // Nope.  Couldn't sign in.
          CommonData.isNotSignedIn = true;
          manager.SwapState(new States.MainMenu());
        }
      } else if (results.sourceState == typeof(FetchUserData)) {
        // Just got back from fetching the user.
        // Did THAT work?
        InitializeAnalytics();
        if (CommonData.testLab.IsTestingScenario) {
          CommonData.currentReplayData = StringConstants.TestLoopReplayData;
          manager.SwapState(new States.TestLoop(CommonData.testLab.ScenarioNumber));
        } else {
          manager.SwapState(new States.MainMenu());
          if (CommonData.currentUser == null) {
            //  If we can't fetch data, tell the user.
            manager.PushState(new BasicDialog(StringConstants.CouldNotFetchUserData));
          }
        }
      } else {
        throw new System.Exception("Returned from unknown state: " + results.sourceState);
      }
    }
  }
}
