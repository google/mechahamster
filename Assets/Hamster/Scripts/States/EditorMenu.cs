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
      Time.timeScale = 0.0f;

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
#endif
      GUILayout.EndVertical();
      GUILayout.EndArea();
    }
  }
}
