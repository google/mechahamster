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
  public class Gameplay : BaseState {

    public enum GameplayMode {
      Gameplay,
      Editor,
      TestLoop,
    }

    GameplayMode mode;

    public Gameplay(GameplayMode mode) {
      this.mode = mode;
    }

    Menus.FloatingButtonGUI dialogComponent;

    // Number of fixedupdates that have happened so far in this
    // gameplay session.  Used for replay synchronization.
    public int fixedUpdateTimestamp { get; private set;  }

    // Gameplay Recording Feature Flag
    bool gameplayRecordingEnabled = false;

    // Are we saving gameplay replay to local file?
    // This is never set true in the game.  It is intended as a
    // temporary option for developers to record levels.
    const bool saveReplayToFile = false;

    // The file name to save the replay under.
    const string gameplayReplayFileName = "test_replay.bytes";

    // Data structure that handles the actual recording of the gameplay.
    private GameplayRecorder gameplayRecorder;

    // Initialization method.  Called after the state
    // is added to the stack.
    public override void Initialize() {
      CommonData.mainGame.SelectAndPlayMusic(CommonData.prefabs.gameMusic, true);
      fixedUpdateTimestamp = 0;
      if (CommonData.vrPointer != null) {
        CommonData.vrPointer.SetActive(false);
      }
      Time.timeScale = 1.0f;
      double gravity_y =
        Firebase.RemoteConfig.FirebaseRemoteConfig.GetValue(
            StringConstants.RemoteConfigPhysicsGravity).DoubleValue;
      Physics.gravity = new Vector3(0, (float)gravity_y, 0);
      CommonData.gameWorld.ResetMap();
      Utilities.HideDuringGameplay.OnGameplayStateChange(true);
      CommonData.mainCamera.mode = CameraController.CameraMode.Gameplay;

      gameplayRecordingEnabled = Firebase.RemoteConfig.FirebaseRemoteConfig.GetValue(
        StringConstants.RemoteConfigGameplayRecordingEnabled).BooleanValue;

      if (gameplayRecordingEnabled) {
        gameplayRecorder = new GameplayRecorder(CommonData.gameWorld.worldMap.name, 1);
      }
      Screen.sleepTimeout = SleepTimeout.NeverSleep;

      dialogComponent = SpawnUI<Menus.FloatingButtonGUI>(StringConstants.PrefabFloatingButton);
      PositionButton();

      CommonData.gameWorld.MergeMeshes();
    }

    // Resume the state.  Called when the state becomes active
    // when the state above is removed.  That state may send an
    // optional object containing any results/data.  Results
    // can also just be null, if no data is sent.
    public override void Resume(StateExitValue results) {
      ShowUI();
      CommonData.mainGame.SelectAndPlayMusic(CommonData.prefabs.gameMusic, true);
      if (CommonData.vrPointer != null) {
        CommonData.vrPointer.SetActive(false);
      }
      Time.timeScale = 1.0f;
      CommonData.mainCamera.mode = CameraController.CameraMode.Gameplay;
      Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    public override StateExitValue Cleanup() {
      DestroyUI();
      if (CommonData.vrPointer != null) {
        CommonData.vrPointer.SetActive(true);
      }
      CommonData.mainCamera.mode = CameraController.CameraMode.Menu;
      Utilities.HideDuringGameplay.OnGameplayStateChange(false);
      Time.timeScale = 0.0f;
      Screen.sleepTimeout = SleepTimeout.SystemSetting;
      return new StateExitValue(typeof(Gameplay));
    }

    public override void Suspend() {
      HideUI();
      if (CommonData.vrPointer != null) {
        CommonData.vrPointer.SetActive(true);
      }
      Time.timeScale = 0.0f;
      CommonData.mainCamera.mode = CameraController.CameraMode.Menu;
      Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    // Moves the GUI to the upper right.
    void PositionButton() {
      Camera camera = CommonData.mainCamera.GetComponentInChildren<Camera>();
      RectTransform rt = gui.GetComponent<RectTransform>();
      // Locations of the corners of the button, in world units.
      Vector3 worldLowerLeft = rt.TransformPoint(rt.anchorMin + rt.offsetMin);
      Vector3 worldupperRight = rt.TransformPoint(rt.anchorMax + rt.offsetMax);

      // Locations of the corners of the button, in screen space.
      Vector2 screenLowerLeft =
          camera.WorldToScreenPoint(worldLowerLeft);
      Vector2 screenUpperRight =
          camera.WorldToScreenPoint(worldupperRight);

      // Dimensions of the button in world units:
      float worldWidth = Mathf.Abs(worldLowerLeft.x - worldupperRight.x);
      float worldHeight = Mathf.Abs(worldLowerLeft.y - worldupperRight.y);

      float pixelsToWorldUnits = worldWidth / Mathf.Abs(screenLowerLeft.x - screenUpperRight.x);

      gui.transform.localPosition = new Vector3(
        gui.transform.localPosition.x + (Screen.width * pixelsToWorldUnits - worldWidth) / 2.0f,
        gui.transform.localPosition.y + (Screen.height * pixelsToWorldUnits - worldHeight) / 2.0f,
        gui.transform.localPosition.z);
    }

    public override void HandleUIEvent(GameObject source, object eventData) {
      if (source == dialogComponent.FloatingButton.gameObject) {
        ExitGameplay();
      }
    }

    void ExitGameplay() {
      CommonData.gameWorld.ResetMap();
      manager.PopState();
    }

    // Called once per frame when the state is active.
    public override void FixedUpdate() {
      if (Input.GetKeyDown(KeyCode.Escape)) {
        ExitGameplay();
        return;
      }
      if (CommonData.mainGame.PlayerController != null) {
        if (gameplayRecordingEnabled) {
          gameplayRecorder.Update(CommonData.mainGame.PlayerController, fixedUpdateTimestamp);
        }

        // If the goal was reached, then we want to finish the Gameplay state.
        if (CommonData.mainGame.PlayerController.ReachedGoal) {
          if (gameplayRecordingEnabled) {
            // Save to local file
            if (saveReplayToFile) {
              gameplayRecorder.OutputToFile(gameplayReplayFileName);
            }

            CommonData.gameWorld.PreviousReplayData = gameplayRecorder.CreateReplayData();
          }

          if (mode == GameplayMode.TestLoop) {
            CommonData.mainGame.SelectAndPlayMusic(CommonData.prefabs.menuMusic, true);
            manager.PopState();
          } else {
            CommonData.mainGame.SelectAndPlayMusic(CommonData.prefabs.winMusic, false);
            manager.SwapState(new LevelFinished(mode));
          }
          return;
        }
      }
      fixedUpdateTimestamp++;
    }

    // Set the player's position directly.  Only really used by replay
    // playerinputcontrollers, to compenstate for drift in the physics playback.
    public void SetPlayerPosition(Vector3 position, Quaternion rotation,
        Vector3 velocity, Vector3 angularVelocity) {
      if (CommonData.mainGame.PlayerController != null) {
        Transform transform = CommonData.mainGame.PlayerController.GetComponent<Transform>();
        Rigidbody rigidbody = CommonData.mainGame.PlayerController.GetComponent<Rigidbody>();
        rigidbody.position = position;
        rigidbody.rotation = rotation;
        rigidbody.velocity = velocity;
        rigidbody.angularVelocity = angularVelocity;
      }
    }

  }
}
