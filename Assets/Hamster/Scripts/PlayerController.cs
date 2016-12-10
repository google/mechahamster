using UnityEngine;
using System.Collections;

namespace Hamster {

  // Class to controll the player's avatar.  (The ball)
  public class PlayerController : MonoBehaviour {
    InputControllers.BasePlayerController inputController;

    void Start() {
      inputController = new InputControllers.MultiInputController();
    }

    // Height of the kill-plane.
    // If the player's y-coordinate ever falls below this, it is treated as
    // a loss/failure.
    const float kFellOffLevelHeight = -10.0f;
    const float kMaxVelocity = 20f;
    const float kMaxVelocitySquared = kMaxVelocity * kMaxVelocity;

    // Since we're doing physics work, we use FixedUpdate instead of Update.
    void FixedUpdate() {
      Rigidbody rigidBody = GetComponent<Rigidbody>();

      Vector2 input = inputController.GetInputVector();
      rigidBody.AddForce(new Vector3(input.x, 0, input.y));

      if (transform.position.y < kFellOffLevelHeight) {
        CommonData.mainGame.DestroyPlayer();
      }
    }
  }
}
