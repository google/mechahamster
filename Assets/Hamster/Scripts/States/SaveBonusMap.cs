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

  // State for creating new Bonus Maps.
  // Not intended for players - this is a developer tool.
  // Very similar to the basic save dialog, except it lets you
  // set the map ID directly, and saves to the bonus map list,
  // instead of the general one.
  class SaveBonusMap : BaseState {

    Vector2 scrollViewPosition;
    string mapName;
    string mapId = StringConstants.DefaultMapId;

    // Initialization method.  Called after the state
    // is added to the stack.
    public override void Initialize() {
      mapName = CommonData.gameWorld.worldMap.name;
    }

    // Called once per frame for GUI creation, if the state is active.
    public override void OnGUI() {
      GUI.skin = CommonData.prefabs.guiSkin;
      GUILayout.BeginVertical();

      GUILayout.Label(StringConstants.LabelSaveBonusMap);

      GUILayout.BeginHorizontal();
      GUILayout.Label(StringConstants.LabelName);
      mapName = GUILayout.TextField(mapName, GUILayout.Width(Screen.width / 2));
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      GUILayout.Label(StringConstants.LabelMapId);
      mapId = GUILayout.TextField(mapId, GUILayout.Width(Screen.width / 2));
      GUILayout.EndHorizontal();

      if (GUILayout.Button(StringConstants.ButtonSaveInNew)) {
        SaveMapToDB();
      }

      if (GUILayout.Button(StringConstants.ButtonCancel)) {
        manager.PopState();
      }

      GUILayout.EndVertical();
    }

    // Save the current map to the database.  If no mapID is provided,
    // a new id is created.  Otherwise, it saves over the existing ID.
    void SaveMapToDB() {
      string path = CommonData.DBBonusMapTablePath + mapId;
      DBStruct<LevelMap> dbLevel = new DBStruct<LevelMap>(path, CommonData.app);

      LevelMap currentLevel = CommonData.gameWorld.worldMap;

      currentLevel.SetProperties(mapName, mapId, CommonData.currentUser.data.id, path);
      CommonData.gameWorld.OnSave();

      dbLevel.Initialize(currentLevel);
      dbLevel.PushData();

      CommonData.currentUser.data.AddMap(currentLevel.name, currentLevel.mapId);
      CommonData.currentUser.PushData();
      manager.PopState();
    }
  }
}
