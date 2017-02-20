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

namespace Hamster.MapObjects {

  // When activated, alerts all other map objects.
  public class SwitchTile : MapObject {
    public int DownCount { get; private set; }

    private void Start() {
      DownCount = 0;
    }

    public override void Reset() {
      DownCount = 0;
      // Reset the animation back to default.
      foreach (Animator animator in transform.root.GetComponentsInChildren<Animator>()) {
        animator.Play(StringConstants.AnimationSwitchIdleState);
      }
    }

    void OnTriggerEnter(Collider collider) {
      if (collider.GetComponent<PlayerController>() != null) {
        ++DownCount;
        // If no one was pressing the switch down before, trigger it.
        if (DownCount == 1) {
          // Set the switch animation to be pressed down.
          foreach (Animator animator in transform.root.GetComponentsInChildren<Animator>()) {
            animator.SetBool(StringConstants.AnimationSwitch, true);
          }

          CommonData.gameWorld.OnSwitchTriggered();
        }
      }
    }

    void OnTriggerExit(Collider collider) {
      if (collider.GetComponent<PlayerController>() != null) {
        --DownCount;
        // If no one is holding down the switch, release it.
        if (DownCount == 0) {
          foreach (Animator animator in transform.root.GetComponentsInChildren<Animator>()) {
            animator.SetBool(StringConstants.AnimationSwitch, false);
          }
        }
      }
    }
  }
}
