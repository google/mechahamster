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

    Menus.SaveMapGUI dialogComponent;
    Vector2 scrollViewPosition;

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
      dialogComponent = SpawnUI<Menus.SaveMapGUI>(StringConstants.PrefabSaveMapMenu);
      dialogComponent.MapName.text = CommonData.gameWorld.worldMap.name;
      // Only display the plain "save" button if they've already
      // saved the map once.
      dialogComponent.SaveButton.gameObject.SetActive(mapAlreadySaved);
    }

    public override void Resume(StateExitValue results) {
      ShowUI();
    }

    public override void Suspend() {
      HideUI();
    }

    public override StateExitValue Cleanup() {
      DestroyUI();
      return null;
    }

    public override void HandleUIEvent(GameObject source, object eventData) {
      if (source == dialogComponent.SaveAsNewButton.gameObject) {
        SaveMapToDB(null);
      } else if (source == dialogComponent.SaveButton.gameObject) {
        SaveMapToDB(CommonData.gameWorld.worldMap.mapId);
      } else if (source == dialogComponent.CancelButton.gameObject) {
        manager.PopState();
      }
    }

    // Save the current map to the database.  If no mapID is provided,
    // a new id is created.  Otherwise, it saves over the existing ID.
    void SaveMapToDB(string mapId) {
      if (mapId == null) {
        mapId = CommonData.currentUser.GetUniqueKey();
      }
      string path = CommonData.DBMapTablePath + mapId;
      DBStruct<LevelMap> dbLevel = new DBStruct<LevelMap>(path, CommonData.app);

      LevelMap currentLevel = CommonData.gameWorld.worldMap;

      currentLevel.SetProperties(dialogComponent.MapName.text,
          mapId, CommonData.currentUser.data.id, path);
      CommonData.gameWorld.OnSave();

      dbLevel.Initialize(currentLevel);
      dbLevel.PushData();

      Firebase.Analytics.FirebaseAnalytics.LogEvent(StringConstants.AnalyticsEventMapCreated,
        StringConstants.AnalyticsParamMapId, CommonData.gameWorld.worldMap.mapId);

      if (Social.localUser.authenticated) {
        Social.ReportProgress(GPGSIds.achievement_map_maker, 100.0f, (bool success) => {
          Debug.Log("Edit a game achiement unlocked. Sucess: " + success);
        });
      }

      CommonData.currentUser.data.AddMap(currentLevel.name, currentLevel.mapId);
      CommonData.currentUser.PushData();
      manager.PopState();
    }
  }
}
