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

  // Utility state for fetching the user data.  (Or making a new user
  // profile if data could not be fetched.)
  // Basically just does the fetch, and then returns the result to whatever
  // state invoked it.
  class FetchUserData : BaseState {
    private string userID;

    public FetchUserData(string userID) {
      this.userID = userID;
    }

    // This state is basically just a more convenient way to use the WaitingForDBLoad
    // state to get user data, and then handle the logic of what to do with the results.
    public override void Initialize() {
      manager.PushState(
        new WaitingForDBLoad<UserData>(CommonData.DBUserTablePath + userID));
    }

    public override StateExitValue Cleanup() {
      return new StateExitValue(typeof(FetchUserData), null);
    }

    // Resume the state.  Called when the state becomes active
    // when the state above is removed.  That state may send an
    // optional object containing any results/data.  Results
    // can also just be null, if no data is sent.
    public override void Resume(StateExitValue results) {
      if (results != null) {
        if (results.sourceState == typeof(WaitingForDBLoad<UserData>)) {
          var resultData = results.data as WaitingForDBLoad<UserData>.Results;
          if (resultData.wasSuccessful) {
            if (resultData.results != null) {
              // Got some results back!  Use this data.
              CommonData.currentUser = new DBStruct<UserData>(
                  CommonData.DBUserTablePath + userID, CommonData.app);
              CommonData.currentUser.Initialize(resultData.results);
              Debug.Log("Fetched user " + CommonData.currentUser.data.name);
            } else {
              // Make a new user, using default credentials.
              Debug.Log("Could not find user " + userID + " - Creating new profile.");
              UserData temp = new UserData();
              temp.name = StringConstants.DefaultUserName;
              temp.id = userID;
              CommonData.currentUser = new DBStruct<UserData>(
                CommonData.DBUserTablePath + userID, CommonData.app);
              CommonData.currentUser.Initialize(temp);
              CommonData.currentUser.PushData();
            }
          } else {
            // Can't fetch data.  Assume internet problems, stay offline.
            CommonData.currentUser = null;
          }
        }
      }
      // Whether we successfully fetched, or had to make a new user,
      // return control to the calling state.
      manager.PopState();
      if (GooglePlayServicesSignIn.CanAutoSignIn()) {
        manager.PushState(
          new WaitForTask(GooglePlayServicesSignIn.SignIn(),
            StringConstants.LabelSigningIn, true));
      }
    }
  }
}
