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
  class LoadMap : BaseLevelSelect {
    const string LevelDirectoryJson = "LevelList";
    string[] levelIds;

    // Levels are loaded slightly differently here - when you click
    // a map name, it loads.  (So it's handled in the selection
    // routine instead of the load.)
    protected override void LoadLevel(int index) { }

    protected override void LevelSelected(int index) {
      // Since this is a state swap instead of push, this will
      // also end the menu state and return to the previous state
      // when it concludes.
      manager.SwapState(
        new WaitingForDBLoad<LevelMap>(CommonData.DBMapTablePath + levelIds[index]));
    }

    // Initialization method.  Called after the state is added to the stack.
    public override void Initialize() {
      mapSelection = -1;
      string[] levelNames = new string[CommonData.currentUser.data.maps.Count];
      levelIds = new string[CommonData.currentUser.data.maps.Count];

      int i = 0;
      foreach (MapListEntry mapEntry in CommonData.currentUser.data.maps) {
        levelNames[i] = mapEntry.name;
        levelIds[i] = mapEntry.mapId;
        i++;
      }
      // Necessary to make sure it doesn't try to load a new map in the background.
      currentLoadedMap = 0;

      MenuStart(levelNames, StringConstants.LabelLoadMap);

      // Hide the "top times" and "let's roll!" buttons in this menu:
      menuComponent.TopTimesButton.gameObject.SetActive(false);
      menuComponent.PlayButton.gameObject.SetActive(false);
    }

    protected override void CancelButtonPressed() {
      manager.PopState();
    }

    // Clean up when we exit the state.
    public override StateExitValue Cleanup() {
      DestroyUI();
      return new StateExitValue(typeof(BaseLevelSelect));
    }

  }

}
