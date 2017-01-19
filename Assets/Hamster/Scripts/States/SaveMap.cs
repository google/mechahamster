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

  // State for handling the "save map" menu.  Allows the user to specify
  // a name for the map, and gives them the option to either save the current
  // map as a new entry in the database, or update a previous version.  (If one
  // exists.)
  class SaveMap : BaseState {

    Vector2 scrollViewPosition;
    string mapName;
    bool mapAlreadySaved = false;

    // Initialization method.  Called after the state
    // is added to the stack.
    public override void Initialize() {
      Time.timeScale = 0.0f;
      mapName = CommonData.gameWorld.worldMap.name;

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
      GUI.skin = CommonData.prefabs.guiSkin;
      GUILayout.BeginVertical();

      GUILayout.Label(StringConstants.LabelSaveMap);

      GUILayout.BeginHorizontal();
      GUILayout.Label(StringConstants.LabelName);
      mapName = GUILayout.TextField(mapName, GUILayout.Width(Screen.width / 2));
      GUILayout.EndHorizontal();

      if (mapAlreadySaved) {
        if (GUILayout.Button(StringConstants.ButtonSaveUpdate)) {
          SaveMapToDB(CommonData.gameWorld.worldMap.mapId);
        }
      }
      if (GUILayout.Button(StringConstants.ButtonSaveInNew)) {
        SaveMapToDB(null);
      }

      if (GUILayout.Button(StringConstants.ButtonCancel)) {
        manager.PopState();
      }

      GUILayout.EndVertical();
    }

    // Save the current map to the database.  If no mapID is provided,
    // a new id is created.  Otherwise, it saves over the existing ID.
    void SaveMapToDB(string mapId) {
      if (mapId == null) {
        mapId = CommonData.currentUser.GetUniqueKey();
      }
      DBStruct<LevelMap> dbLevel =
          new DBStruct<LevelMap>(CommonData.DBMapTablePath + mapId, CommonData.app);

      LevelMap currentLevel = CommonData.gameWorld.worldMap;

      currentLevel.SetProperties(mapName, mapId, CommonData.currentUser.data.id);

      dbLevel.Initialize(currentLevel);
      dbLevel.PushData();

      CommonData.currentUser.data.AddMap(currentLevel.name, currentLevel.mapId);
      CommonData.currentUser.PushData();
      manager.PopState();
    }
  }
}
