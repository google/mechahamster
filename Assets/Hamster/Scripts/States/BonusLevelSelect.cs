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
using System.Collections.Generic;

namespace Hamster.States {
  // Select a map that has been shared!  Uses the general level select template. (BaseLevelSelect)
  class BonusLevelSelect : BaseLevelSelect {

    Dictionary<int, string> levelMap;

    // Initialization method.  Called after the state is added to the stack.
    public override void Initialize() {
      List<MapListEntry> levelDir = CommonData.currentUser.data.bonusMaps;
      string[] levelNames = new string[levelDir.Count];
      levelMap = new Dictionary<int, string>();

      // Generate a list of level names.
      int index = 0;
      foreach (MapListEntry mapListEntry in levelDir) {
        levelMap.Add(index, mapListEntry.mapId);
        levelNames[index] = mapListEntry.name;
        index++;
      }
      MenuStart(levelNames, StringConstants.BonusLevelScreenTitle);
    }

    // Called whenever a level is selected in the menu.
    protected override void LoadLevel(int i) {
      manager.PushState(new WaitingForDBLoad<LevelMap>(
          CommonData.DBBonusMapTablePath + levelMap[i]));
    }

    public override void Resume(StateExitValue results) {
      base.Resume(results);
      if (results != null) {
        if (results.sourceState == typeof(WaitingForDBLoad<LevelMap>)) {
          var resultData = results.data as WaitingForDBLoad<LevelMap>.Results;
          if (resultData.wasSuccessful && resultData.results != null) {
            currentLevel = resultData.results;
            currentLevel.DatabasePath = resultData.path;
            CommonData.gameWorld.DisposeWorld();
            CommonData.gameWorld.SpawnWorld(currentLevel);
          } else {
            manager.PushState(new BasicDialog("Unable to load map."));
          }
        }
      }
    }
  }
}
