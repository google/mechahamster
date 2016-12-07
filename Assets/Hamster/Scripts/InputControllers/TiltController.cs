using UnityEngine;
using System.Collections;

namespace Hamster.InputControllers {

  // Class for accelerometer controller interfaces.
  // Responsible for returning a 2d vector representing the
  // player's movement, based on device accelerometers.  (i. e. tilting)
  public class TiltController : BasePlayerController {
    // Scalar, for modifying the x/z components of the accelerometer
    // velocity, to generate the final player velocity from tilting
    // the device.
    const float kTiltVelocity = 16.0f;


    public override Vector2 GetInputVector() {
      Vector3 tiltInput = Input.acceleration.normalized * kTiltVelocity;
      return new Vector3(tiltInput.x, tiltInput.y);
    }
  }

}