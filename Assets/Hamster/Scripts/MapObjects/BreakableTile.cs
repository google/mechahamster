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

using Hamster.Utilities;
using System.Collections;
using UnityEngine;

namespace Hamster.MapObjects {

  // When activated, breaks the tile away from underneath it.
  public class BreakableTile : MapObject {

    // How long to wait between collision and breaking the tile, in seconds.
    public float BreakDelayTime = 0.5f;
    // How long to wait after breaking the tile to hide the renderables, in seconds.
    public float HideRenderableStartTime = 0.15f;
    // How long to shrink the renderables before hiding, in seconds.
    public float HideRenderableShrinkTime = 0.25f;

    // Tracks if the tile is breaking away, to prevent multiple triggers.
    private bool IsBroken = false;

    public override void Reset() {
      // Since we use coroutines to drive the behavior, we need to stop them.
      StopAllCoroutines();

      IsBroken = false;

      // Reactivate all the children.
      gameObject.ForEachRootChildOfType<Renderer>(r => {
          r.enabled = true;
          r.transform.localScale = Vector3.one;
        });
      gameObject.ForEachRootChildOfType<Collider>(c => c.enabled = true);

      foreach (Animator animator in transform.root.GetComponentsInChildren<Animator>()) {
        animator.enabled = true;
        animator.SetTrigger(StringConstants.AnimationBreakIdleState);
      }
    }

    void OnTriggerEnter(Collider collider) {
      if (collider.GetComponent<PlayerController>() != null && !IsBroken) {
        IsBroken = true;
        StartCoroutine(DelayedBreakaway());
      }
    }

    // Waits the appropriate amount of time, then triggers the tile breaking away.
    IEnumerator DelayedBreakaway() {
      yield return new WaitForSeconds(BreakDelayTime);

      // Start the animation of the tile breaking away.
      foreach (Animator animator in transform.root.GetComponentsInChildren<Animator>()) {
        animator.enabled = true;
        animator.SetTrigger(StringConstants.AnimationBreakTile);
      }

      // Disable any colliders.
      gameObject.ForEachRootChildOfType<Collider>(c => c.enabled = false);
      // After starting the animation, we want to eventually hide the renderables.
      StartCoroutine(DelayedHideRenderables());
    }

    // Hides the renderables of the tile, for use after it has broken away.
    IEnumerator DelayedHideRenderables() {
      yield return new WaitForSeconds(HideRenderableStartTime);

      // First, shrink the renderables over time, so it is less jarring when they disappear.
      for (float elapsedTime = Time.deltaTime; elapsedTime < HideRenderableShrinkTime;
           elapsedTime += Time.deltaTime) {
        Vector3 scale = (1.0f - (elapsedTime / HideRenderableShrinkTime)) * Vector3.one;
        gameObject.ForEachRootChildOfType<Renderer>(r => r.transform.localScale = scale);
        yield return null;
      }

      // Hide all the renderables.
      gameObject.ForEachRootChildOfType<Renderer>(r => r.enabled = false);
    }
  }
}
