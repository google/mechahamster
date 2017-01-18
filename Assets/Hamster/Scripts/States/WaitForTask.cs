using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Firebase.Unity.Editor;


namespace Hamster.States {
  // Utility state, for fetching structures from the database.
  // Returns the result in the result struct.
  class WaitForTask : BaseState {

    static int LabelHeight = 100;
    static int LabelWidth = 800;

    protected bool isComplete = false;
    string waitText;
    System.Threading.Tasks.Task task;

    Firebase.Database.FirebaseDatabase database;

    public WaitForTask(System.Threading.Tasks.Task task, string waitText = "") {
      this.waitText = waitText;
      this.task = task;
    }

    // Initialization method.  Called after the state
    // is added to the stack.
    public override void Initialize() {
      Time.timeScale = 0.0f;
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
      if (task.IsCompleted) {
        manager.PopState();
      }
    }

    public override StateExitValue Cleanup() {
      return new StateExitValue(typeof(WaitForTask), new Results(task));
    }

    // Called once per frame for GUI creation, if the state is active.
    public override void OnGUI() {
      GUI.skin = CommonData.prefabs.guiSkin;
      UnityEngine.GUIStyle centeredStyle = GUI.skin.GetStyle("Label");
      centeredStyle.alignment = TextAnchor.UpperCenter;
      GUI.Label(new Rect(Screen.width / 2 - LabelWidth/2,
        Screen.height / 2 - LabelHeight/2, LabelWidth, LabelHeight), waitText, centeredStyle);
    }

    // Class for encapsulating the results of the database load, as
    // well as information about whether the load was successful
    // or not.
    public class Results {
      public System.Threading.Tasks.Task task;

      public Results(System.Threading.Tasks.Task task) {
        this.task = task;
      }
    }
  }
}
