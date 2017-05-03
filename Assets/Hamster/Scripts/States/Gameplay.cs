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
      Editor
    }

    GameplayMode mode;

    public Gameplay(GameplayMode mode) {
      this.mode = mode;
    }

    // Number of fixedupdates that have happened so far in this
    // gameplay session.  Used for replay synchronization.
    public int fixedUpdateTimestamp { get; private set;  }

    // Are we currently recording for a gameplay playback?
    // This is never set true in the game.  It is intended as a
    // temporary option for developers to record levels.
    const bool recordGameplay = false;

    // The file name to save the replay under.
    const string gameplayReplayFileName = "test_replay.json";

    // Data structure that handles the actual recording of the gameplay.
    private GameplayRecorder gameplayRecorder;

    // Initialization method.  Called after the state
    // is added to the stack.
    public override void Initialize() {
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

      if (recordGameplay) {
        gameplayRecorder = new GameplayRecorder(CommonData.gameWorld.worldMap.name);
      }
    }

    // Resume the state.  Called when the state becomes active
    // when the state above is removed.  That state may send an
    // optional object containing any results/data.  Results
    // can also just be null, if no data is sent.
    public override void Resume(StateExitValue results) {
      if (CommonData.vrPointer != null) {
        CommonData.vrPointer.SetActive(false);
      }
      Time.timeScale = 1.0f;
      CommonData.mainCamera.mode = CameraController.CameraMode.Gameplay;
    }

    public override StateExitValue Cleanup() {
      if (CommonData.vrPointer != null) {
        CommonData.vrPointer.SetActive(true);
      }
      CommonData.mainCamera.mode = CameraController.CameraMode.Menu;
      Utilities.HideDuringGameplay.OnGameplayStateChange(false);
      Time.timeScale = 0.0f;
      return new StateExitValue(typeof(Gameplay));
    }

    public override void Suspend() {
      if (CommonData.vrPointer != null) {
        CommonData.vrPointer.SetActive(true);
      }
      Time.timeScale = 0.0f;
      CommonData.mainCamera.mode = CameraController.CameraMode.Menu;
    }

    // Called once per frame when the state is active.
    public override void FixedUpdate() {
      if (Input.GetKeyDown(KeyCode.Escape)) {
        CommonData.gameWorld.ResetMap();
        manager.PopState();
        return;
      }
      if (CommonData.mainGame.PlayerController != null) {
        if (recordGameplay) {
          gameplayRecorder.Update(CommonData.mainGame.PlayerController, fixedUpdateTimestamp);
        }

        // If the goal was reached, then we want to finish the Gameplay state.
        if (CommonData.mainGame.PlayerController.ReachedGoal) {
          if (recordGameplay) {
            gameplayRecorder.OutputToFile(gameplayReplayFileName);
          }
          manager.SwapState(new LevelFinished(mode));
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

#if UNITY_IOS
    public override void OnGUI() {
      GUI.skin = CommonData.prefabs.guiSkin;
      float buttonWidth = Screen.width * 0.2f;
      float buttonHeight = Screen.height * 0.1f;
      GUILayout.BeginArea(new Rect(Screen.width - buttonWidth - 10, Screen.height - buttonHeight,
        buttonWidth, buttonHeight));
      if (GUILayout.Button (StringConstants.ButtonExit)) {
        manager.PopState ();
      }
    GUILayout.EndArea ();
    }
#endif
  }
}
