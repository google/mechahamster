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
  // State for handling menu UI from within the editor.  Handles
  // basic map operations like loading/saving, clearing, sharing,
  // and exiting to the main menu.
  class EditorMenu : BaseState {

    bool mapAlreadySaved = false;

    // Initialization method.  Called after the state
    // is added to the stack.
    public override void Initialize() {
      // Check if this map has already been saved, or if it's a new map:
      foreach (MapListEntry entry in CommonData.currentUser.data.maps) {
        if (entry.mapId == CommonData.gameWorld.worldMap.mapId) {
          mapAlreadySaved = true;
          break;
        }
      }
    }

    // Called once per frame for GUI creation, if the state is active.
    public override void OnGUI() {
      const float MenuMarginHorizontal = 0.25f;
      const float MenuMarginVertical = 0.0f;

      float horizontalMargin = MenuMarginHorizontal * Screen.width;
      float verticalMargin = MenuMarginVertical * Screen.height;

      UnityEngine.GUIStyle centeredStyle = GUI.skin.GetStyle("Label");

      GUILayout.BeginArea(new Rect(horizontalMargin, verticalMargin,
        Screen.width - horizontalMargin * 2, Screen.height - verticalMargin * 2));

      centeredStyle.alignment = TextAnchor.UpperCenter;

      GUILayout.BeginVertical();
      GUI.skin = CommonData.prefabs.guiSkin;

      if (GUILayout.Button(StringConstants.ButtonSave)) {
        manager.SwapState(new SaveMap());
      }
      if (GUILayout.Button(StringConstants.ButtonLoad)) {
        CommonData.gameWorld.DisposeWorld();
        manager.SwapState(new LoadMap());
      }
      if (mapAlreadySaved) {
        // You can only share maps once they've been saved to the database.
        if (GUILayout.Button(StringConstants.ButtonInvite)) {
          manager.SwapState(new SendInvite());
        }
      }

      if (GUILayout.Button(StringConstants.ButtonClear)) {
        CommonData.gameWorld.DisposeWorld();
        manager.PopState();
      }

      if (GUILayout.Button(StringConstants.ButtonMainMenu)) {
        manager.PopState();
        manager.SwapState(new MainMenu());
      }
      if (GUILayout.Button(StringConstants.ButtonCancel)) {
        manager.PopState();
      }
#if UNITY_EDITOR
      // This button is a debug function, for easily exporting premade
      // maps during development.  It should never show up on
      // actual release builds.
      if (GUILayout.Button("export")) {
        manager.PushState(new ExportMap(CommonData.gameWorld.worldMap));
      }

      // This button is a developer tool, for easily creating bonus levels.
      // It is not intended to be public-facing.
      if (GUILayout.Button("Save as Bonus Map")) {
        manager.PushState(new SaveBonusMap());
      }
#endif
      GUILayout.EndVertical();
      GUILayout.EndArea();
    }
  }
}
