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

    // Whether the best replay record of the current level is available in the storage.
    // If true, the option to download and to play the record will be presented to the player.
    private bool isBestReplayAvailable {
      get {
        return !string.IsNullOrEmpty(bestReplayPath);
      }
    }

    // The storage path to the best replay record stored in the database
    private string bestReplayPath = null;

    // Downloaded replay data
    private ReplayData bestReplayData = null;

    // Prefab ID to spawn the object for replay animation
    private const string replayPrefabID = "ReplayPlayer";

    // Reference to spawned replay animator
    private ReplayAnimator replayAnimator = null;

    // The state of a replay record
    private enum ReplayState {
      None,         // No record downloaded
      Stopped,      // The record downloaded but not played
      Downloading,  // Downloading the record
      Playing       // Playing the record
    }

    // The state of the best replay record for current level
    private ReplayState bestReplayState = ReplayState.None;

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

        // Subscribe player spawn event in order to reset replay data whenever it is triggered.
        CommonData.mainGame.PlayerSpawnedEvent.AddListener(OnPlayerSpawned);
      }
      Screen.sleepTimeout = SleepTimeout.NeverSleep;

      dialogComponent = SpawnUI<Menus.FloatingButtonGUI>(StringConstants.PrefabFloatingButton);
      PositionButton();

      CommonData.gameWorld.MergeMeshes();

      // Retrieve path to the best replay record
      if (gameplayRecordingEnabled) {
        TimeDataUtil.GetBestSharedReplayPathAsync(CommonData.gameWorld.worldMap)
          .ContinueWith(task => {
          bestReplayPath = task.Result;
        });
      }
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
      DestroyReplayAnimator();

      if (gameplayRecordingEnabled) {
        CommonData.mainGame.PlayerSpawnedEvent.RemoveListener(OnPlayerSpawned);
      }

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

    // The event triggered when the player is spawned.
    // This is primarily to reset replay data recording whenever the player respawns
    // However, if saveReplayToFile is set to record replay data for testing, everything
    // will be recorded even if the player dies several times before success.
    void OnPlayerSpawned() {
      if (gameplayRecorder != null && !saveReplayToFile) {
        fixedUpdateTimestamp = 0;
        gameplayRecorder.Reset(CommonData.gameWorld.worldMap.name);
      }
    }

    // Moves the "back" button to upper right and "replay" button to upper left
    void PositionButton() {
      Camera camera = CommonData.mainCamera.GetComponentInChildren<Camera>();

      GameObject backButton = dialogComponent.FloatingButton.gameObject;
      RectTransform backRt = backButton.GetComponent<RectTransform>();
      Vector2 backLocalLowerLeft = backRt.anchorMin + backRt.offsetMin;
      Vector2 backLocalUpperRight = backRt.anchorMax + backRt.offsetMax;

      // Locations of the corners of the button, in screen space.
      Vector2 backScreenLowerLeft =
          camera.WorldToScreenPoint(backRt.TransformPoint(backLocalLowerLeft));
      Vector2 back_screenUpperRight =
          camera.WorldToScreenPoint(backRt.TransformPoint(backLocalUpperRight));

      Vector2 back_localDimension = new Vector2(
        Mathf.Abs(backLocalUpperRight.x - backLocalLowerLeft.x),
        Mathf.Abs(backLocalUpperRight.y - backLocalLowerLeft.y));

      float pixelsToLocalUnits = back_localDimension.x /
        Mathf.Abs(backScreenLowerLeft.x - back_screenUpperRight.x);

      // Move back button to upper right corner
      backButton.transform.localPosition = new Vector3(
        backButton.transform.localPosition.x +
        (Screen.width * pixelsToLocalUnits - back_localDimension.x) / 2.0f,
        backButton.transform.localPosition.y +
        (Screen.height * pixelsToLocalUnits - back_localDimension.y) / 2.0f,
        backButton.transform.localPosition.z);

      // Move replay button to upper left corner
      GameObject replayButton = dialogComponent.ReplayButton.gameObject;
      if (replayButton) {
        RectTransform replayRt = replayButton.GetComponent<RectTransform>();
        Vector2 replayLocalLowerLeft = replayRt.anchorMin + replayRt.offsetMin;
        Vector2 replayLocalUpperRight = replayRt.anchorMax + replayRt.offsetMax;

        Vector2 replayLocalDimension = new Vector2(
          Mathf.Abs(replayLocalUpperRight.x - replayLocalLowerLeft.x),
          Mathf.Abs(replayLocalUpperRight.y - replayLocalLowerLeft.y));

        replayButton.transform.localPosition = new Vector3(
          replayButton.transform.localPosition.x -
          (Screen.width * pixelsToLocalUnits - replayLocalDimension.x) / 2.0f,
          replayButton.transform.localPosition.y +
          (Screen.height * pixelsToLocalUnits - replayLocalDimension.y) / 2.0f,
          replayButton.transform.localPosition.z);

        // Hide replay button by default
        replayButton.SetActive(false);
      }
    }

    public override void HandleUIEvent(GameObject source, object eventData) {
      if (source == dialogComponent.FloatingButton.gameObject) {
        ExitGameplay();
      } else if (source == dialogComponent.ReplayButton.gameObject) {
        ProcessReplayButtonEvent();
      }
    }

    void ExitGameplay() {
      CommonData.gameWorld.ResetMap();
      manager.PopState();
    }

    void SetReplayState(ReplayState newState) {
      if (this.bestReplayState == newState) {
        return;
      }

      switch (newState) {
        case ReplayState.Stopped:
          dialogComponent.ReplayButton.gameObject.SetActive(true);
          dialogComponent.ReplayButtonText.text = StringConstants.ButtonReplayPlay;
          DestroyReplayAnimator();
          break;
        case ReplayState.Downloading:
          dialogComponent.ReplayButton.gameObject.SetActive(true);
          dialogComponent.ReplayButtonText.text = StringConstants.ButtonReplayWait;
          break;
        case ReplayState.Playing:
          dialogComponent.ReplayButton.gameObject.SetActive(true);
          dialogComponent.ReplayButtonText.text = StringConstants.ButtonReplayStop;
          SpawnReplayAnimator();
          break;
        default:
          dialogComponent.ReplayButton.gameObject.SetActive(false);
          dialogComponent.ReplayButtonText.text = "";
          break;
      }

      this.bestReplayState = newState;
    }

    void ProcessReplayButtonEvent() {
      if (!isBestReplayAvailable) {
        return;
      }

      switch (this.bestReplayState) {
        case ReplayState.Stopped:
          if (this.bestReplayData == null) {
            SetReplayState(ReplayState.Downloading);
            ReplayData.DownloadReplayRecordAsync(bestReplayPath).ContinueWith(task => {
              if (!task.IsFaulted && !task.IsCanceled && task.Result != null) {
                this.bestReplayData = task.Result;
              }
            });
          } else {
            SetReplayState(ReplayState.Playing);
          }
          break;
        case ReplayState.Playing:
          SetReplayState(ReplayState.Stopped);
          break;
        case ReplayState.Downloading:
        default:
          break;
      }
    }

    void SpawnReplayAnimator() {
      // Make sure there is only one copy of replay animator
      DestroyReplayAnimator();

      GameObject replayObj =
        Object.Instantiate(CommonData.prefabs.lookup[replayPrefabID].prefab) as GameObject;
      if (replayObj != null) {
        replayAnimator = replayObj.GetComponent<ReplayAnimator>();
        if (replayAnimator != null) {
          replayAnimator.SetReplayData(bestReplayData);
          replayAnimator.Play();
          replayAnimator.FinishEvent.AddListener(() => {
            SetReplayState(ReplayState.Stopped);
          });
        }
      }
    }

    void DestroyReplayAnimator() {
      if (replayAnimator != null) {
        GameObject.Destroy(replayAnimator.gameObject);
        replayAnimator = null;
      }
    }

    // Called once per frame when the state is active.
    public override void FixedUpdate() {
      if (Input.GetKeyDown(KeyCode.Escape)) {
        ExitGameplay();
        return;
      }

      // Change replay state in Main thread
      if (this.bestReplayState == ReplayState.None && isBestReplayAvailable) {
        SetReplayState(ReplayState.Stopped);
      }
      if (this.bestReplayState == ReplayState.Downloading && this.bestReplayData != null) {
        SetReplayState(ReplayState.Playing);
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
            if (Social.localUser.authenticated) {
              Social.ReportProgress(GPGSIds.achievement_game_starter, 100.0f, (bool success) => {
                Debug.Log("Game starter achievement unlocked. Sucess: " + success);
              });
            }
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
