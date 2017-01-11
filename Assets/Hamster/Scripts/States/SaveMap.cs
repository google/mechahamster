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
          new DBStruct<LevelMap>(CommonData.kDBMapTablePath + mapId, CommonData.app);

      LevelMap currentLevel = CommonData.gameWorld.worldMap;

      currentLevel.name = mapName;
      currentLevel.mapId = mapId;
      currentLevel.ownerId = CommonData.currentUser.data.id;

      dbLevel.Initialize(currentLevel);
      dbLevel.PushData();

      CommonData.currentUser.data.addMap(currentLevel);
      CommonData.currentUser.PushData();
      manager.PopState();
    }
  }
}
