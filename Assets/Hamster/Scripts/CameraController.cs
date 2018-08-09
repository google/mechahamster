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

namespace Hamster {
  /// <summary>
  /// Class for controlling and orienting the camera.
  /// Keeps the camera focused on the player and moves it along with them.
  /// If there is no player, it focuses on the origin instead.
  /// </summary>
  public class CameraController : MonoBehaviour {

    // Set by the inspector:
    // Default camera view angle in degrees.
    public float ViewAngle = -40.0f;
    // Distance between the camera and the ground.
    public float ViewDistance = 10.0f;
    // How fast you pan in the editor.
    public float EditorScrollSpeed = 8.0f;
    // How fast the camera zooms to its new target:
    public float CameraZoom = 0.05f;
    // Height of the object the camera focuses on.  (Normally
    // the center of the hamster ball.)
    public float CameraFocusHeight = 0.5f;

    // The height is a lowered in VR mode.
    const float VRHeightScalar = 0.75f;

    // Values representing various modes the camera can be in.
    public enum CameraMode {
      Gameplay,
      Menu,
      Editor,
      Dragging
    }

    // Current behavior mode the camera is in.
    public CameraMode mode = CameraMode.Menu;

    private MainGame mainGame;
    private Vector3 editorCam = new Vector3(0, 0, 0);
    private static Vector3 kUpVector = new Vector3(0, 1, 1);
    PlayerController player;

    // The location used for controlling the camera with mouse drag.
    private Vector3 pinnedLocation;

    // Should be editor camera be controlled via mouse drag.
    public bool MouseControlsEditorCamera { get; set; }

    void Start() {
      mainGame = FindObjectOfType<MainGame>();
      // Needs to be normalized because it was set via the inspector.
      if (CommonData.inVrMode) {
        float VRHeightScalar;
        try {
          VRHeightScalar =
          (float)Firebase.RemoteConfig.FirebaseRemoteConfig.GetValue(
          StringConstants.RemoteConfigVRHeightScale).DoubleValue;
        } catch (System.Exception) {
          // If the RemoteConfig failed, use a sensible value.
          VRHeightScalar = 0.65f;
        }
      }
    }

    // Pans the camera in a direction during edit mode.
    public void PanCamera(Vector3 direction) {
      // Need to use time.realtimeSinceStartup instead of Time.DeltaTime, because
      // the way we pause the physics simulation (and hence the game, while we're
      // in editor mode) is by setting the Timescale to 0.
      editorCam += direction * EditorScrollSpeed * mainGame.TimeSinceLastUpdate;
    }

    public void MoveCameraTo(Vector3 newPos) {
      editorCam = newPos;
    }

    void LateUpdate() {
      Vector3 viewAngleVector = Quaternion.Euler(-ViewAngle, 0, 0) * -Vector3.forward;

      switch (mode) {
        case CameraMode.Gameplay:
          // Gameplay mode:  Camera should follow the player:
          if (player == null)
            player = FindObjectOfType<PlayerController>();
          if (player != null) {
            Vector3 camTarget = player.transform.position;
            camTarget.y = CameraFocusHeight;
            transform.position = camTarget + viewAngleVector * ViewDistance;
            if (!CommonData.inVrMode) {
              transform.LookAt(player.transform.position, kUpVector);
            }
          }
          break;
        case CameraMode.Dragging:
          Ray ray = GetComponentInChildren<Camera>().ScreenPointToRay(Input.mousePosition);
          float dist;
          if (CommonData.kZeroPlane.Raycast(ray, out dist)) {
            if (Input.GetMouseButtonDown(0)) {
              // Save where the mouse went down, as we want that to stay under the mouse.
              pinnedLocation = ray.GetPoint(dist);
            } else if (Input.GetMouseButton(0)) {
              // Move the camera based on how far the mouse was dragged.
              Vector3 diff = pinnedLocation - ray.GetPoint(dist);
              transform.position += diff;
              editorCam += diff;
            }
          }
          break;
        case CameraMode.Editor:
          // Editor mode:  Camera should be user-controlled.
          if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) {
            PanCamera(Vector3.forward);
          }
          if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) {
            PanCamera(Vector3.back);
          }
          if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) {
            PanCamera(Vector3.left);
          }
          if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) {
            PanCamera(Vector3.right);
          }

          Vector3 cameraTarget = editorCam + viewAngleVector * ViewDistance;

          transform.position = cameraTarget * CameraZoom
              + transform.position * (1.0f - CameraZoom);

          if (!CommonData.inVrMode) {
            transform.LookAt(transform.position - viewAngleVector, kUpVector);
          }
          break;
        case CameraMode.Menu:
          // Menu mode.  User is in a menu.
          Vector3 menuCameraTarget =
            editorCam + viewAngleVector * ViewDistance;

          transform.position = menuCameraTarget * CameraZoom
              + transform.position * (1.0f - CameraZoom);

          if (!CommonData.inVrMode) {
            transform.LookAt(transform.position - viewAngleVector, kUpVector);
          }
          break;
      }
    }
  }
}
