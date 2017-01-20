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

  // State invoked whenever a new message is received.
  // Is responsible for decoding the message, and then
  // triggering whatever actions are necesary.
  class InviteReceived : BaseState {

    Firebase.Invites.InviteReceivedEventArgs args;
    string mapId;

    public InviteReceived(Firebase.Invites.InviteReceivedEventArgs args) {
      this.args = args;
    }

    public override void Resume(StateExitValue results) {
      if (results != null) {
        if (results.sourceState == typeof(WaitingForDBLoad<string>)) {
          var resultData = results.data as WaitingForDBLoad<string>.Results;

          CommonData.currentUser.data.AddSharedMap(resultData.results, mapId);
          CommonData.currentUser.PushData();

          // Now tell them about it!
          manager.SwapState(new BasicDialog(string.Format(StringConstants.SharedMapUserMessage,
            resultData.results)));
        } else {
          manager.SwapState(new BasicDialog("Got back something weird..."));
        }

      }
    }

    public override void Initialize() {
      // TODO(ccornell): This is a hack - need to make a more robust parsing.
      string[] splitString = args.DeepLink.Query.Split(new string[] { "?mapid=" }, 2, System.StringSplitOptions.None);
      if (splitString.Length != 2) {
        Debug.LogError("Received an unkown invite format: " + args.DeepLink.Query);
        manager.PopState();
        return;
      }
      mapId = splitString[1];

      // Otherwise, fetch the map name, and tell them cool stuff.
      manager.PushState(new WaitingForDBString(CommonData.DBMapTablePath + mapId + "/name"));
    }

    public override void OnGUI() {
      GUI.skin = CommonData.prefabs.guiSkin;
      GUILayout.BeginVertical();
      GUILayout.EndVertical();
    }
  }
}
