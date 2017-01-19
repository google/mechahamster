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
  // State for handling UI for loading new maps.  Presents the user with
  // a list of all maps associated with the current userID, and loads any
  // map selected.
  class LoadMap : BaseState {

    Vector2 scrollViewPosition;

    // Initialization method.  Called after the state
    // is added to the stack.
    public override void Initialize() {
      Time.timeScale = 0.0f;
    }

    // Called once per frame for GUI creation, if the state is active.
    public override void OnGUI() {
      GUI.skin = CommonData.prefabs.guiSkin;
      GUILayout.BeginVertical();

      GUILayout.Label(StringConstants.LabelLoadMap);

      GUILayout.Label(StringConstants.LabelOverwrite);
      string selectedId = null;
      scrollViewPosition = GUILayout.BeginScrollView(scrollViewPosition);
      foreach (MapListEntry mapEntry in CommonData.currentUser.data.maps) {
        if (GUILayout.Button(mapEntry.name)) {
          selectedId = mapEntry.mapId;
        }
      }
      GUILayout.EndScrollView();
      if (GUILayout.Button(StringConstants.ButtonCancel)) {
        manager.PopState();
      }
      GUILayout.EndVertical();

      if (selectedId != null) {
        manager.SwapState(
          new WaitingForDBLoad<LevelMap>(CommonData.DBMapTablePath + selectedId));
      }
    }
  }
}
