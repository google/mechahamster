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
        new States.WaitingForDBLoad<UserData>(CommonData.DBUserTablePath + userID));
    }

    // Resume the state.  Called when the state becomes active
    // when the state above is removed.  That state may send an
    // optional object containing any results/data.  Results
    // can also just be null, if no data is sent.
    public override void Resume(StateExitValue results) {
      Time.timeScale = 0.0f;
      if (results != null) {
        if (results.sourceState == typeof(WaitingForDBLoad<UserData>)) {
          var resultData = results.data as WaitingForDBLoad<UserData>.Results;
          CommonData.currentUser = new DBStruct<UserData>(
            CommonData.DBUserTablePath + userID, CommonData.app);
          if (resultData.wasSuccessful) {
            CommonData.currentUser.Initialize(resultData.results);
            Debug.Log("Fetched user " + CommonData.currentUser.data.name);
          } else {
            // Make a new user, using default credentials.
            Debug.Log("Could not find user " + CommonData.currentUser.data.name +
               " - Creating new profile.");
            UserData temp = new UserData();
            temp.name = StringConstants.DefaultUserName;
            temp.id = userID;
            CommonData.currentUser.Initialize(temp);
            CommonData.currentUser.PushData();
          }
          // Whether we successfully fetched, or had to make a new user,
          // return control to the calling state.
          manager.PopState();
        }
      }
    }

    // Called once per frame for GUI creation, if the state is active.
    public override void OnGUI() {
      GUI.skin = CommonData.prefabs.guiSkin;
      UnityEngine.GUIStyle centeredStyle = GUI.skin.GetStyle("Label");
      centeredStyle.alignment = TextAnchor.UpperCenter;
      GUI.Label(new Rect(Screen.width / 2 - 400,
        Screen.height / 2 - 50, 800, 100), StringConstants.LabelFetchingUserData, centeredStyle);
    }
  }
}
