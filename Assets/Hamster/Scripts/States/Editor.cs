using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Firebase.Unity.Editor;


namespace Hamster.States {
  class Editor : BaseState {
    LevelMap currentLevel;

    Vector2 scrollViewPosition;
    int mapToolSelection = 0;

    // This is a placeholder while I swap in the selector.
    string mapId = "unnamed_map";

    // Initialization method.  Called after the state
    // is added to the stack.
    public override void Initialize() {
      // Set up our map to edit, and populate the data
      // structure with the necessary IDs.
      mapId = CommonData.currentUser.GetUniqueKey();
      currentLevel = new LevelMap();

      CommonData.gameWorld.worldMap.mapId = mapId;
      CommonData.gameWorld.worldMap.ownerId = CommonData.currentUser.data.id;

      Time.timeScale = 0.0f;
    }

    // Clean up when we exit the state.
    public override StateExitValue Cleanup() {
      CommonData.gameWorld.DisposeWorld();
      return null;
    }

    // Resume the state.  Called when the state becomes active
    // when the state above is removed.  That state may send an
    // optional object containing any results/data.  Results
    // can also just be null, if no data is sent.
    public override void Resume(StateExitValue results) {
      Time.timeScale = 0.0f;
      if (results != null) {
        if (results.sourceState == typeof(WaitingForDBLoad<LevelMap>)) {
          var resultData = results.data as WaitingForDBLoad<LevelMap>.Results;
          if (resultData.wasSuccessful) {
            currentLevel = resultData.results;
            CommonData.gameWorld.DisposeWorld();
            CommonData.gameWorld.SpawnWorld(currentLevel);
            Debug.Log("Map load complete!");
          } else {
            Debug.LogWarning("Map load complete, but not successful...");
          }
        }
      }
    }

    // Called once per frame when the state is active.
    public override void Update() {
      if (Input.GetMouseButton(0) && GUIUtility.hotControl == 0) {
        string brushElementType = CommonData.prefabs.prefabNames[mapToolSelection];
        float rayDist;
        Ray cameraRay = CommonData.mainCamera.ScreenPointToRay(Input.mousePosition);
        if (CommonData.kZeroPlane.Raycast(cameraRay, out rayDist)) {
          MapElement element = new MapElement();
          Vector3 pos = cameraRay.GetPoint(rayDist);
          pos.x = Mathf.RoundToInt(pos.x);
          pos.y = Mathf.RoundToInt(pos.y);
          pos.z = Mathf.RoundToInt(pos.z);
          element.position = pos;
          element.type = brushElementType;

          CommonData.gameWorld.PlaceTile(element);
        }
      }
    }

    // Called once per frame for GUI creation, if the state is active.
    public override void OnGUI() {
      GUI.skin = CommonData.prefabs.guiSkin;
      GUILayout.BeginHorizontal();

      scrollViewPosition = GUILayout.BeginScrollView(scrollViewPosition);

      mapToolSelection = GUILayout.SelectionGrid(
          mapToolSelection, CommonData.prefabs.prefabNames, 1);

      GUILayout.EndScrollView();

      if (GUILayout.Button(StringConstants.ButtonSave)) {
        manager.PushState(new SaveMap());
      }
      if (GUILayout.Button(StringConstants.ButtonLoad)) {
        CommonData.gameWorld.DisposeWorld();
        manager.PushState(new LoadMap());
      }
      if (GUILayout.Button(StringConstants.ButtonClear)) {
        CommonData.gameWorld.DisposeWorld();
      }
      if (GUILayout.Button(StringConstants.ButtonPlay)) {
        manager.PushState(new Gameplay());
      }
      if (GUILayout.Button(StringConstants.ButtonBack)) {
        manager.SwapState(new MainMenu());
      }
      // TODO(ccornell): Remove this!  Export is just here to
      // make it easy for me to generate/edit prepackaged levels
      // during devleopment!
      if (GUILayout.Button("export")) {
        manager.PushState(new ExportMap(CommonData.gameWorld.worldMap));
      }
      GUILayout.EndHorizontal();
    }
  }
}
