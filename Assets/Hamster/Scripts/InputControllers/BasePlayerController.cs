using UnityEngine;
using System.Collections;

// Base Class for player controller interfaces.
// Responsible for returning a 2-d vector representing
// the player's movement.  (Abstracted to make it easy to write
// new ones for different control schemes.)
public class BasePlayerController {

  public virtual Vector2 GetInputVector() {
    return Vector2.zero;
  }

}
