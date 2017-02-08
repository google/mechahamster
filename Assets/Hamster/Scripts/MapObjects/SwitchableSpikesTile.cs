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

  // Manages spikes that knock the player away, and are adjustable by switches.
  public class SwitchableSpikesTile : MapObject, ISwitchable {
    // Whether the spikes start in the up position when the level begins.
    public bool EnabledAtStart = false;

    public float ExplosionForce = 10.0f;
    public float ExplosionRadius = 2.0f;
    public float ExplosionUpwardsModifier = -0.5f;

    // Whether the spikes are currently active.
    public bool SpikesActive { get; private set; }

    private void Start() {
      ForceState(EnabledAtStart);
    }

    public override void Reset() {
      ForceState(EnabledAtStart);
    }

    // Sets the spikes state, other then the animation data.
    void SetNonAnimationState(bool state) {
      SpikesActive = state;
      Collider collider = GetComponent<Collider>();
      if (collider != null) {
        collider.enabled = state;
      }
    }

    // Force the spikes into a certain state, skipping animation.
    void ForceState(bool state) {
      SetNonAnimationState(state);
      foreach (Animator animator in transform.root.GetComponentsInChildren<Animator>()) {
        animator.Play(state ? StringConstants.AnimationSpikesIdleUp :
          StringConstants.AnimationSpikesIdleDown);
      }
    }

    // Transition the spikes into a certain state, playing the animation.
    void TransitionState(bool state) {
      SetNonAnimationState(state);
      foreach (Animator animator in transform.root.GetComponentsInChildren<Animator>()) {
        animator.SetTrigger(state ? StringConstants.AnimationSpikesUp :
          StringConstants.AnimationSpikesDown);
        // Reset the other trigger, to prevent both happening in the same frame.
        animator.ResetTrigger(state ? StringConstants.AnimationSpikesDown :
          StringConstants.AnimationSpikesUp);
      }
    }

    // When a switch is triggered, toggle the spikes.
    public void OnSwitchTriggered() {
      TransitionState(!SpikesActive);
    }

    // When the ball rolls into the spikes, send it flying with a force.
    void OnTriggerEnter(Collider collider) {
      if (collider.GetComponent<PlayerController>() != null) {
        Rigidbody rigidbody = collider.GetComponent<Rigidbody>();
        rigidbody.AddExplosionForce(ExplosionForce, transform.position,
          ExplosionRadius, ExplosionUpwardsModifier, ForceMode.Impulse);
      }
    }
  }
}
