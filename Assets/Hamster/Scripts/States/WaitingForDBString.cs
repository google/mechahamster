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
  // Utility state, for fetching strings.  Is basically
  // just a specialization of WaitingForDBLoad, but it needs
  // some minor changes to how the results are parsed, since
  // they're not technically valid json.
  class WaitingForDBString : WaitingForDBLoad<string> {
    public WaitingForDBString(string path) : base(path) { }

    protected override void HandleResult(
        System.Threading.Tasks.Task<Firebase.Database.DataSnapshot> task) {
      if (task.IsFaulted) {
        HandleFaultedFetch(task);
        return;
      } else if (task.IsCompleted) {
        if (task.Result != null) {
          string json = task.Result.GetRawJsonValue();
          // Check for length>2, because the minimum valid
          // string we can get back is "''" - a string containing
          // nothing but two quotes.  (Designating an empty string.)
          if (json.Length > 2) {
            result = json.Substring(1, json.Length - 2);
            wasSuccessful = true;
          }
        }
      }
      isComplete = true;
    }
  }
}
