using UnityEngine;
using System.Collections;

namespace Hamster.MapObjects {

  // General base-class for objects on the map.
  public class StartPosition : MapObject {

    static Vector3 kPlayerStartOffset = new Vector3(0, 5, 0);

    // Populated by the inspector:
    // Prefab to use when spawning a new player avatar at level start.
    public GameObject playerPrefab;

    public override void Update() {
      if (CommonData.mainGame.isGameRunning()) {
        if (CommonData.mainGame.player == null) {
          GameObject player = CommonData.mainGame.SpawnPlayer();
          player.transform.position = transform.position + kPlayerStartOffset;
          player.transform.rotation = Quaternion.identity;
        }
      } else {
        if (CommonData.mainGame.player != null) {
          CommonData.mainGame.DestroyPlayer();
        }
      }
    }
  }
}
