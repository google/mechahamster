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
    public Vector3 kViewAngleVector;
    // Distance between the camera and the ground.
    public float kViewDistance = 10.0f;
    // How fast you pan in the editor.
    public float kEditorScrollSpeed = 0.3f;
    // How fast the camera zooms to its new target:
    public float kCameraZoom = 0.05f;

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
      kViewAngleVector.Normalize();
    }

    // Pans the camera in a direction during edit mode.
    public void PanCamera(Vector3 direction) {
      // Need to use time.realtimeSinceStartup instead of Time.DeltaTime, because
      // the way we pause the physics simulation (and hence the game, while we're
      // in editor mode) is by setting the Timescale to 0.
      editorCam += direction * kEditorScrollSpeed * mainGame.TimeSinceLastUpdate;
    }

    void LateUpdate() {
      if (mainGame.isGameRunning()) {
        // Gameplay mode:  Camera should follow the player:
        if (player == null)
          player = FindObjectOfType<PlayerController>();
        if (player != null) {
          transform.position = player.transform.position +
              kViewAngleVector * kViewDistance;
          transform.LookAt(player.transform.position, kUpVector);
        }
      } else if (MouseControlsEditorCamera && Input.GetMouseButton(0)) {
        Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        float dist;
        if (CommonData.kZeroPlane.Raycast(ray, out dist)) {
          if (Input.GetMouseButtonDown(0)) {
            // Save where the mouse went down, as we want that to stay under the mouse.
            pinnedLocation = ray.GetPoint(dist);
          } else {
            // Move the camera based on how far the mouse was dragged.
            Vector3 diff = pinnedLocation - ray.GetPoint(dist);
            transform.position += diff;
            editorCam += diff;
          }
        }
      } else {
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

        Vector3 cameraTarget = editorCam + kViewAngleVector * kViewDistance;

        transform.position = cameraTarget * kCameraZoom
            + transform.position * (1.0f - kCameraZoom);

        transform.LookAt(transform.position - kViewAngleVector, kUpVector);
      }
    }
  }
}
