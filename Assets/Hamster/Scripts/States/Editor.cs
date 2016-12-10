using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Firebase.Unity.Editor;


namespace Hamster.States {
  class Editor : BaseState {

    DBTable<LevelMap> mapTable;

    Vector2 scrollViewPosition;
    int mapToolSelection = 0;

    const string kButtonNameSave = "Save";
    const string kButtonNameLoad = "Load";
    const string kButtonNameClear = "Clear";
    const string kButtonNamePlay = "Play";

    const string kDBMapTablePath = "MapList";

    // Initialization method.  Called after the state
    // is added to the stack.
    public override void Initialize() {
      mapTable = new DBTable<LevelMap>(kDBMapTablePath, CommonData.app);
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
        if (mapTable.data.ContainsKey("test_map")) mapTable.data.Remove("test_map");
        mapTable.Add("test_map", CommonData.gameWorld.worldMap);
        mapTable.PushData();
      }
      if (GUILayout.Button(kButtonNameLoad)) {
        CommonData.gameWorld.DisposeWorld();
        mapTable = new DBTable<LevelMap>(kDBMapTablePath, CommonData.app);
        manager.PushState(new WaitingForDBLoad(mapTable));
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
