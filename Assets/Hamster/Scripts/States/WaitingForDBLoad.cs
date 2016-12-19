using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Firebase.Unity.Editor;


namespace Hamster.States {
  class WaitingForDBLoad<T> : BaseState where T : new() {

    bool isComplete = false;
    bool wasSuccessful = false;
    T result = default(T);
    string path;

    Firebase.Database.FirebaseDatabase database;

    public WaitingForDBLoad(string path) {
      this.path = path;
    }

    // Initialization method.  Called after the state
    // is added to the stack.
    public override void Initialize() {
      Time.timeScale = 0.0f;

      database = Firebase.Database.FirebaseDatabase.GetInstance(CommonData.app);

      database.GetReference(path).GetValueAsync().ContinueWith(task => {
        isComplete = true;
        if (task.IsFaulted) {
          Debug.LogError("Database could not fetch value at [" + path + "] :\n"
            + task.Exception);
        } else if (task.IsCompleted) {
          result = JsonUtility.FromJson<T>(task.Result.GetRawJsonValue());
          wasSuccessful = true;
        }
      });
    }

    // Resume the state.  Called when the state becomes active
    // when the state above is removed.  That state may send an
    // optional object containing any results/data.  Results
    // can also just be null, if no data is sent.
    public override void Resume(StateExitValue results) {
      Time.timeScale = 0.0f;
    }

    // Called once per frame when the state is active.
    public override void Update() {
      if (isComplete) {
        manager.PopState();
      }
    }

    public override StateExitValue Cleanup() {
      return new StateExitValue(typeof(WaitingForDBLoad<T>), new Results(result, wasSuccessful));
    }

    // Called once per frame for GUI creation, if the state is active.
    public override void OnGUI() {
      GUI.skin = CommonData.prefabs.guiSkin;
      UnityEngine.GUIStyle centeredStyle = GUI.skin.GetStyle("Label");
      centeredStyle.alignment = TextAnchor.UpperCenter;
      GUI.Label(new Rect(Screen.width / 2 - 400,
        Screen.height / 2 - 50, 800, 100), "Loading...", centeredStyle);
    }

    // Class for encapsulating the results of the database load, as
    // well as information about whether the load was successful
    // or not.
    public class Results {
      public T results;
      public bool wasSuccessful;

      public Results(T results, bool wasSuccessful) {
        this.results = results;
        this.wasSuccessful = wasSuccessful;
      }
    }

  }
}
