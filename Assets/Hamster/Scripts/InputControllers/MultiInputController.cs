using UnityEngine;
using System.Collections.Generic;

namespace Hamster.InputControllers {

  // Class for compositing multile input sources.
  // Useful for normalizing and combining different input sources.
  public class MultiInputController : BasePlayerController {

    List<BasePlayerController> controllerList = new List<BasePlayerController>() {
    new KeyboardController(),
    new TiltController(),
};

    public override Vector2 GetInputVector() {
      Vector2 result = Vector2.zero;
      foreach (BasePlayerController controller in controllerList) {
        result += controller.GetInputVector();
      }
      return result;
    }
  }
}