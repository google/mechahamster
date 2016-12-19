using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Firebase.Unity.Editor;


namespace Hamster.States {
  class Editor : BaseState {
    DBStruct<LevelMap> currentLevel;

    Vector2 scrollViewPosition;
    int mapToolSelection = 0;

    const string kButtonNameSave = "Save";
    const string kButtonNameLoad = "Load";
    const string kButtonNameClear = "Clear";
    const string kButtonNamePlay = "Play";

    // This is a placeholder while I swap in the selector.
    const string kMapName = "test_map";

    // Initialization method.  Called after the state
    // is added to the stack.
    public override void Initialize() {
      currentLevel = new DBStruct<LevelMap>(
          CommonData.kDBMapTablePath + kMapName, CommonData.app);
      Time.timeScale = 0.0f;
    }

    // Cleanup function.  Called just before the state
    // is removed from the stack.  Returns an optional
    // StateExitValue
    public override StateExitValue Cleanup() {
      return null;
    }

    // Resume the state.  Called when the state becomes active
    // when the state above is removed.  That state may send an
    // optional object containing any results/data.  Results
    // can also just be null, if no data is sent.
    public override void Resume(StateExitValue results) {
      Time.timeScale = 0.0f;
      if (results != null && results.sourceState == typeof(WaitingForDBLoad<LevelMap>)) {
        var resultData = results.data as WaitingForDBLoad<LevelMap>.Results;
        if (resultData.wasSuccessful) {
          currentLevel.Initialize(resultData.results);
          CommonData.gameWorld.DisposeWorld();
          CommonData.gameWorld.SpawnWorld(currentLevel.data);
          Debug.Log("Map load complete!");
        } else {
          Debug.LogWarning("Map load complete, but not successful...");
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

      if (GUILayout.Button(kButtonNameSave)) {
        currentLevel.Initialize(CommonData.gameWorld.worldMap);
        currentLevel.PushData();
      }
      if (GUILayout.Button(kButtonNameLoad)) {
        CommonData.gameWorld.DisposeWorld();
        manager.PushState(new WaitingForDBLoad<LevelMap>(CommonData.kDBMapTablePath + kMapName));
      }
      if (GUILayout.Button(kButtonNameClear)) {
        CommonData.gameWorld.DisposeWorld();
      }
      if (GUILayout.Button(kButtonNamePlay)) {
        manager.PushState(new Gameplay());
      }
      GUILayout.EndHorizontal();
    }
  }
}
