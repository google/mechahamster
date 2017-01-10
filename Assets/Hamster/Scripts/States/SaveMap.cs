using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Firebase.Unity.Editor;


namespace Hamster.States {
  class SaveMap : BaseState {

    private LevelMap currentLevel;

    public SaveMap(LevelMap currentLevel) {
      this.currentLevel = currentLevel;
    }

    // Initialization method.  Called after the state
    // is added to the stack.
    public override void Initialize() {
      Time.timeScale = 0.0f;
    }

    string mapName = StringConstants.kDefaultMapName;

    // Called once per frame for GUI creation, if the state is active.
    public override void OnGUI() {
      GUI.skin = CommonData.prefabs.guiSkin;
      GUILayout.BeginVertical();

      GUILayout.BeginHorizontal();
      GUILayout.Label(StringConstants.kLabelName);
      mapName = GUILayout.TextField(mapName, GUILayout.Width(Screen.width/2));
      GUILayout.EndHorizontal();

      if (GUILayout.Button(StringConstants.kButtonSaveInNew)) {
        SaveMapToDB(null);
      }
      GUILayout.Label(StringConstants.kLabelOverwrite);
      string selectedId = null;
      foreach (MapListEntry mapEntry in CommonData.currentUser.data.maps) {
        if (GUILayout.Button(mapEntry.name)) {
          selectedId = mapEntry.mapId;
        }
      }
      if (selectedId != null) {
        SaveMapToDB(selectedId);
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
