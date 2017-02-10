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

  // State to bypass the usual UI, and just jump straight into a game.
  // When testing VR, this is loaded instead of the main menu.
  // Used for testing VR gameplay, while we work on making the UI more
  // VR friendly.
  // TODO(ccornell) Remove once we get menus!  [daydream scaffolding]
  class DaydreamLevelLoader : BaseState {

    const string LevelDirectoryJson = "LevelList";
    const int LevelToLoad = 0;

    public override void Initialize() {
      LevelMap currentLevel;
      LevelSelect.LevelDirectory levelDir;
      TextAsset json = Resources.Load(LevelDirectoryJson) as TextAsset;
      levelDir = JsonUtility.FromJson<LevelSelect.LevelDirectory>(json.ToString());

      json = Resources.Load(levelDir.levels[LevelToLoad].filename) as TextAsset;
      currentLevel = JsonUtility.FromJson<LevelMap>(json.ToString());
      currentLevel.DatabasePath = null;
      CommonData.gameWorld.DisposeWorld();
      CommonData.gameWorld.SpawnWorld(currentLevel);
      manager.PushState(new Gameplay());
    }

    // Clean up when we exit the state.
    public override StateExitValue Cleanup() {
      CommonData.gameWorld.DisposeWorld();
      return null;
    }

  }
}
