using UnityEngine;
using System.Collections;

/// <summary>
/// Class for controlling and orienting the camera.
/// Keeps the camera focused on the player and moves it along with them.
/// If there is no player, it focuses on the origin instead.
/// </summary>
public class CameraController : MonoBehaviour {

  PlayerController player;
  MainGame mainGame;
  public Vector3 editorCam = new Vector3(0, 0, 0);
  public Vector3 kViewAngleVector = new Vector3(0.0f, 3.0f, -1.9f).normalized;
  static Vector3 kUpVector = new Vector3(0, 1, 1);
  // Distance (in world units) between camera and target.
  public float kViewDistance = 10.0f;

  void Start() {
    mainGame = FindObjectOfType<MainGame>();
  }

  void LateUpdate() {
    if (mainGame.isGameRunning()) {
      if (player == null) {
        player = FindObjectOfType<PlayerController>();
        if (player != null) {
          transform.position = player.transform.position + kViewAngleVector * kViewDistance;
          transform.LookAt(player.transform.position, kUpVector);
        }
        else {
          transform.position = kViewAngleVector * kViewDistance;
          transform.LookAt(Vector3.zero, kUpVector);
        }
      }
      else {
        transform.position = player.transform.position + kViewAngleVector * kViewDistance;
      }
    }
    else {
      transform.position = editorCam + kViewAngleVector * kViewDistance;
      transform.LookAt(editorCam, new Vector3(0, 1, 1));
    }

  }
}
