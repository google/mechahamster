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

    string mapName = StringConstants.kDefaultMapName;

    // Called once per frame for GUI creation, if the state is active.
    public override void OnGUI() {
      GUI.skin = CommonData.prefabs.guiSkin;
      GUILayout.BeginVertical();

      GUILayout.Label(StringConstants.kLabelLoadMap);

      GUILayout.Label(StringConstants.kLabelOverwrite);
      string selectedId = null;
      scrollViewPosition = GUILayout.BeginScrollView(scrollViewPosition);
      foreach (MapListEntry mapEntry in CommonData.currentUser.data.maps) {
        if (GUILayout.Button(mapEntry.name)) {
          selectedId = mapEntry.mapId;
        }
      }
      GUILayout.EndScrollView();
      if (GUILayout.Button(StringConstants.kButtonNameCancel)) {
        manager.PopState();
      }
      GUILayout.EndVertical();

      if (selectedId != null) {
        manager.SwapState(
          new WaitingForDBLoad<LevelMap>(CommonData.kDBMapTablePath + selectedId));
      }
    }
  }
}
