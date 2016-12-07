using UnityEngine;
using System.Collections;

namespace Hamster.InputControllers {

  // Class for keyboard controller interfaces.
  // Responsible for returning a 2d vector representing the
  // player's movement, based on keypresses.
  public class KeyboardController : BasePlayerController {
    // Velocity, in world-units-per-second, from holding down
    // a key.
    const float kKeyVelocity = 8.0f;

    public override Vector2 GetInputVector() {
      Vector2 result = Vector2.zero;

      if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) {
        result += new Vector2(-kKeyVelocity, 0);
      }
      if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) {
        result += new Vector2(kKeyVelocity, 0);
      }
      if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) {
        result += new Vector2(0, kKeyVelocity);
      }
      if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) {
        result += new Vector2(0, -kKeyVelocity);
      }
      return result;
    }
  }

}