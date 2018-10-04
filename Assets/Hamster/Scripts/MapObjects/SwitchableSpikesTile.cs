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
using Firebase.RemoteConfig;

namespace Hamster.MapObjects {

  // Manages spikes that knock the player away, and are adjustable by switches.
  public class SwitchableSpikesTile : MapObject, ISwitchable {
    // Whether the spikes start in the up position when the level begins.
    public bool EnabledAtStart = false;

    // The amount of damage caused to the hamster/ball when it hits this spike.
    public int DamageAmount = 0;

    public float ExplosionForce { get; private set; }
    public float ExplosionRadius { get; private set; }
    public float ExplosionUpwardsModifier { get; private set; }

    // Whether the spikes are currently active.
    public bool SpikesActive { get; private set; }

    private void Start() {
      ExplosionForce = (float)FirebaseRemoteConfig.GetValue(
        StringConstants.RemoteConfigSpikesTileForce).DoubleValue;
      ExplosionRadius = (float)FirebaseRemoteConfig.GetValue(
        StringConstants.RemoteConfigSpikesTileRadius).DoubleValue;
      ExplosionUpwardsModifier = (float)FirebaseRemoteConfig.GetValue(
        StringConstants.RemoteConfigSpikesTileUpwardsMod).DoubleValue;

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
        animator.enabled = true;
        animator.Play(state ? StringConstants.AnimationSpikesIdleUp :
          StringConstants.AnimationSpikesIdleDown);
      }
    }

    // Transition the spikes into a certain state, playing the animation.
    void TransitionState(bool state) {
      SetNonAnimationState(state);
      foreach (Animator animator in transform.root.GetComponentsInChildren<Animator>()) {
        animator.enabled = true;
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
      PlayerController pc = collider.GetComponent<PlayerController>();
      if (pc != null) {
        pc.Hit(DamageAmount);
        Rigidbody rigidbody = collider.GetComponent<Rigidbody>();
        rigidbody.AddExplosionForce(ExplosionForce, transform.position,
          ExplosionRadius, ExplosionUpwardsModifier, ForceMode.Impulse);
      }
    }

    // If using a collision shape instead, treat it the same as if it was a trigger.
    private void OnCollisionEnter(Collision collision) {
      OnTriggerEnter(collision.collider);
    }
  }
}
