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
  class Editor : BaseState {
    LevelMap currentLevel;

    Vector2 scrollViewPosition;
    int mapToolSelection = 0;

    // Rotation is in 90 degree increments.
    int currentOrientation = 0;

    // Strings to graphically represent orientation.  Placeholder
    // until we get art.
    static string[] orientationStrings = new string[] { "^", ">", "v", "<" };

    // Special tools that get prepended to the list.
    enum SpecialTools {
      Camera = 0,
    }

    private string[] tools = null;
    string[] Tools {
      get {
        if (tools == null) {
          List<string> toolList = new List<string>();
          // We want the Special Tools to be listed at the top.
          toolList.AddRange(System.Enum.GetNames(typeof(SpecialTools)));
          toolList.AddRange(CommonData.prefabs.prefabNames);
          tools = toolList.ToArray();
        }
        return tools;
      }
    }

    // Tell the camera controller if the special tool Camera is selected.
    private void UpdateCameraController() {
      CameraController c = CommonData.mainCamera.GetComponent<CameraController>();
      if (c != null) {
        c.MouseControlsEditorCamera = mapToolSelection == (int)SpecialTools.Camera;
      }
    }

    // Initialization method.  Called after the state
    // is added to the stack.
    public override void Initialize() {
      // Set up our map to edit, and populate the data
      // structure with the necessary IDs.
      string mapId = CommonData.currentUser.GetUniqueKey();
      currentLevel = new LevelMap();

      CommonData.gameWorld.worldMap.SetProperties(StringConstants.DefaultMapName,
          mapId, CommonData.currentUser.data.id, null);

      UpdateCameraController();
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
      if (results != null) {
        if (results.sourceState == typeof(WaitingForDBLoad<LevelMap>)) {
          var resultData = results.data as WaitingForDBLoad<LevelMap>.Results;
          if (resultData.wasSuccessful) {
            currentLevel = resultData.results;
            currentLevel.DatabasePath = resultData.path;
            CommonData.gameWorld.DisposeWorld();
            CommonData.gameWorld.SpawnWorld(currentLevel);
            Debug.Log("Map load complete!");
          } else {
            Debug.LogWarning("Map load complete, but not successful...");
          }
        }
      }
      UpdateCameraController();
    }

    // Called once per frame when the state is active.
    public override void Update() {
      if (Input.GetMouseButton(0) && GUIUtility.hotControl == 0) {
        int specialToolCount = System.Enum.GetNames(typeof(SpecialTools)).Length;
        if (mapToolSelection >= specialToolCount) {
          int selection = mapToolSelection - specialToolCount;
          string brushElementType = CommonData.prefabs.prefabNames[selection];
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
            element.orientation = currentOrientation;

            CommonData.gameWorld.PlaceTile(element);
          }
        }
      }
    }

    // Called once per frame for GUI creation, if the state is active.
    public override void OnGUI() {
      GUI.skin = CommonData.prefabs.guiSkin;
      GUILayout.BeginHorizontal();

      scrollViewPosition = GUILayout.BeginScrollView(scrollViewPosition);

      int previousSelection = mapToolSelection;
      mapToolSelection = GUILayout.SelectionGrid(mapToolSelection, Tools, 1);

      // If selecting or unselecting Camera, we need to update the controller.
      if (previousSelection != mapToolSelection &&
          (previousSelection == (int)SpecialTools.Camera ||
           mapToolSelection == (int)SpecialTools.Camera)) {
        UpdateCameraController();
      }

      GUILayout.EndScrollView();

      if (GUILayout.Button(
        StringConstants.ButtonOrientation + orientationStrings[currentOrientation])) {
        currentOrientation = (currentOrientation + 1) % 4;
      }

      if (GUILayout.Button(StringConstants.ButtonPlay)) {
        manager.PushState(new Gameplay());
      }

      if (GUILayout.Button(StringConstants.ButtonMenu)) {
        manager.PushState(new EditorMenu());
      }
      GUILayout.EndHorizontal();
    }
  }
}
