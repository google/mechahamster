using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Firebase.Unity.Editor;


namespace Hamster.States {
  // Utility state, for fetching strings.  Is basically
  // just a specialization of WaitingForDBLoad, but it needs
  // some minor changes to how the results are parsed, since
  // they're not technically valid json.
  class WaitingForDBString : WaitingForDBLoad<string> {
    public WaitingForDBString(string path) : base(path) { }

    protected override void HandleResult(
        System.Threading.Tasks.Task<Firebase.Database.DataSnapshot> task) {
      if (task.IsFaulted) {
        Debug.LogError("Database exception!  Path = [" + path + "]\n"
          + task.Exception);
      } else if (task.IsCompleted) {
        string json = task.Result.GetRawJsonValue();
        if (json.Length > 2) {
          result = json.Substring(1, json.Length - 2);
          wasSuccessful = true;
        }
      }
      isComplete = true;
    }
  }
}
