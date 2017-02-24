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
using System.Collections.Generic;

namespace Hamster.States {
  class Editor : BaseState {
    LevelMap currentLevel;
    Menus.EditorGUI menuComponent;
    const string ArrowPrefab = "ArrowButton";

    Dictionary<int, GameObject> toolButtons = new Dictionary<int, GameObject>();
    int currentPage = 0;

    // Maximum distance away we can place objects in the editor.
    const int MaxEditorDistance = 50;

    const int UpArrowId = -1;
    const int DownArrowId = -2;

    const int ButtonsPerPage = 6;

    const float ButtonHighlightScale = 1.2f;

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

    // Initialization method.  Called after the state
    // is added to the stack.
    public override void Initialize() {
      menuComponent = SpawnUI<Menus.EditorGUI>(StringConstants.PrefabEditorMenu);
      // Set up our map to edit, and populate the data
      // structure with the necessary IDs.
      string mapId = CommonData.currentUser.GetUniqueKey();
      currentLevel = new LevelMap();

      CommonData.gameWorld.worldMap.SetProperties(StringConstants.DefaultMapName,
          mapId, CommonData.currentUser.data.id, null);

      UpdateCameraController();
      CommonData.mainCamera.mode = CameraController.CameraMode.Editor;

      SpawnToolButtons(0);
      UpdateOrientationIndicator();
    }

    public override void Suspend() {
      CommonData.mainCamera.mode = CameraController.CameraMode.Menu;
      menuComponent.gameObject.SetActive(false);
    }

    // Clean up when we exit the state.
    public override StateExitValue Cleanup() {
      DestroyUI();
      CommonData.mainCamera.mode = CameraController.CameraMode.Menu;
      CommonData.gameWorld.DisposeWorld();
      return null;
    }

    // Resume the state.  Called when the state becomes active
    // when the state above is removed.  That state may send an
    // optional object containing any results/data.  Results
    // can also just be null, if no data is sent.
    public override void Resume(StateExitValue results) {
      menuComponent.gameObject.SetActive(true);
      CommonData.mainCamera.mode = CameraController.CameraMode.Editor;
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

    // Tell the camera controller if the special tool Camera is selected.
    private void UpdateCameraController() {
      if (mapToolSelection == (int)SpecialTools.Camera) {
        CommonData.mainCamera.MouseControlsEditorCamera = true;
        CommonData.mainCamera.mode = CameraController.CameraMode.Dragging;
      } else {
        CommonData.mainCamera.MouseControlsEditorCamera = false;
        CommonData.mainCamera.mode = CameraController.CameraMode.Editor;
      }
    }

    // Removes all buttons from the screen.
    void ClearCurrentButtons() {
      // Preserve the highlight, in case it's attached to a button we're about to destroy:
      menuComponent.Highlight.transform.SetParent(menuComponent.transform, false);
      foreach (KeyValuePair<int, GameObject> pair in toolButtons) {
        GameObject.Destroy(pair.Value);
      }
      toolButtons.Clear();
    }

    // Update the button highlights and make sure that only the selection has one.
    void UpdateButtonHighlights() {
      foreach (KeyValuePair<int, GameObject> pair in toolButtons) {
        Menus.GUIEditorButton button = pair.Value.GetComponent<Menus.GUIEditorButton>();
        // Attach the highlight to the appropriate button:
        if (button.buttonId == mapToolSelection) {
          menuComponent.Highlight.gameObject.SetActive(true);
          menuComponent.Highlight.transform.SetParent(button.transform, false);
          menuComponent.Highlight.transform.position =
              button.transform.position - button.transform.TransformVector(new Vector3(0, 0, 1));
          return;
        }
      }
      // No highlighted button found.  Hide the highlight.
      menuComponent.Highlight.transform.SetParent(menuComponent.transform, false);
      menuComponent.Highlight.gameObject.SetActive(false);
    }

    // Creates one page worth of tool buttons for a given page number.  Sets their
    // properties, and makes sure they're in the correct part of the window.
    // Also removes any existing buttons in the panel.
    void SpawnToolButtons(int page) {
      ClearCurrentButtons();
      int maxButtonIndex = (currentPage + 1) * ButtonsPerPage;
      if (maxButtonIndex > Tools.Length - 1) maxButtonIndex = Tools.Length - 1;

      GameObject arrowButtonPrefab = CommonData.prefabs.lookup[ArrowPrefab].buttonPrefab;
      if (page > 0) {
        SpawnButton(arrowButtonPrefab, UpArrowId);
      }

      for (int i = currentPage * ButtonsPerPage; i < maxButtonIndex; i++) {
        GameObject buttonPrefab = CommonData.prefabs.lookup[Tools[i]].buttonPrefab;
        if (buttonPrefab == null) {
          buttonPrefab = CommonData.prefabs.lookup[Tools[0]].buttonPrefab;
        }
        SpawnButton(buttonPrefab, i);
      }

      if (maxButtonIndex < Tools.Length) {
          GameObject button = SpawnButton(arrowButtonPrefab, DownArrowId);
          RectTransform rt = button.GetComponent<RectTransform>();
          rt.localRotation = Quaternion.AngleAxis(180, Vector3.forward);
      }
      UpdateButtonHighlights();
    }

    // Utility function to spawn a single button and add it to the tool panel.
    GameObject SpawnButton(GameObject buttonPrefab, int buttonId) {
      GameObject button = GameObject.Instantiate(
          buttonPrefab, menuComponent.ToolPanel.transform, false);

      Menus.GUIEditorButton component = button.GetComponent<Menus.GUIEditorButton>();
      if (component != null) {
        component.buttonId = buttonId;
        toolButtons[buttonId] = button;
      } else {
        Debug.LogError("Button prefab had no LevelSelectButtionGUI component.");
      }
      return button;
    }

    // Handle button clicks etc, and act on them.
    public override void HandleUIEvent(GameObject source, object eventData) {
      if (source == menuComponent.MainButton.gameObject) {
        manager.SwapState(new MainMenu());
      } else if (source == menuComponent.ClearButton.gameObject) {
        CommonData.gameWorld.DisposeWorld();
      } else if (source == menuComponent.LoadButton.gameObject) {
        manager.PushState(new LoadMap());
      } else if (source == menuComponent.SaveButton.gameObject) {
        manager.PushState(new SaveMap());
      } else if (source == menuComponent.ShareButton.gameObject) {
        manager.PushState(new SendInvite());
      } else if (source == menuComponent.PlayButton.gameObject) {
        manager.PushState(new Gameplay());
      } else if (source == menuComponent.RotateButton.gameObject) {
        currentOrientation = (currentOrientation + 1) % 4;
        UpdateOrientationIndicator();
      } else {
        Menus.GUIEditorButton editorButton = source.GetComponent<Menus.GUIEditorButton>();
        if (editorButton!= null) {
          int previousSelection = mapToolSelection;
          if (editorButton.buttonId == DownArrowId) {
            SpawnToolButtons(++currentPage);
          } else if (editorButton.buttonId == UpArrowId) {
            SpawnToolButtons(--currentPage);
          } else {
            mapToolSelection = editorButton.buttonId;
            UpdateButtonHighlights();
          }

          // If selecting or unselecting Camera, we need to update the controller.
          if (previousSelection != mapToolSelection &&
              (previousSelection == (int)SpecialTools.Camera ||
               mapToolSelection == (int)SpecialTools.Camera)) {
            UpdateCameraController();
          }
        }
      }
    }

    // Updates the little triangle indicator to point in the correct direction.
    void UpdateOrientationIndicator() {
      menuComponent.TileRotationIcon.localRotation =
          Quaternion.AngleAxis(-90 * currentOrientation, Vector3.forward);
    }


    // Tests to see if the pointer is currently over any UI.
    // (Because we don't want to place world elements if they're just trying
    // to click a UI button.)
    bool IsClickBlockedByUI() {
      return (Menus.GUIButton.allActiveButtons.Count > 0);
    }

    public override void Update() {
      Ray selectionRay;
      float rayDist;
      if (!CommonData.inVrMode) {
        Camera camera = CommonData.mainCamera.GetComponentInChildren<Camera>();
        selectionRay = camera.ScreenPointToRay(Input.mousePosition);
      } else {
        // To make this function in the editor, set selection ray to a valid laser
        // pointer here.
        selectionRay = new Ray(Vector3.zero, Vector3.up);
      }

      if (Input.GetMouseButton(0) && !IsClickBlockedByUI()) {
        int specialToolCount = System.Enum.GetNames(typeof(SpecialTools)).Length;
        if (mapToolSelection >= specialToolCount) {
          int selection = mapToolSelection - specialToolCount;
          string brushElementType = CommonData.prefabs.prefabNames[selection];

          if (CommonData.kZeroPlane.Raycast(selectionRay, out rayDist) &&
                rayDist < MaxEditorDistance) {
            MapElement element = new MapElement();
            Vector3 pos = selectionRay.GetPoint(rayDist);
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

  }
}
