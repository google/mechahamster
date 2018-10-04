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
using Hamster.Utilities;
using Firebase.RemoteConfig;

namespace Hamster.MapObjects {

  // When activated, triggers a Launch Mine event that when finished, creates an
  // explosion to propel the player ball.
  public class MineTile : MapObject {
    // The child object that corresponds to the mine ball that is launched.
    public GameObject Mine;
    // The explosion prefab to spawn when the mine explodes.
    public GameObject ExplosionPrefab;
    // The shrapnel prefab to spawn when the mine explodes.
    public GameObject ShrapnelPrefab;

    // The amount of time after the tile is used that it can't be used again, in seconds.
    public static float TotalCooldownTime = 1.0f;

    // The amount of damage caused to the hamster/ball when this mine explodes.
    public int DamageAmount = 0;

    // The cooldown timer for the tile, in seconds.
    public float Cooldown { get; private set; }

    // The amount of force to apply with the explosion.
    public float ExplosionForce { get; private set; }
    // The radius of the explosion.
    public float ExplosionRadius { get; private set; }
    // The upwards modifier to apply with the explosion.
    public float UpwardsModifier { get; private set; }

    // The audio to play when launching a mine.
    public AudioClip LaunchMineAudio;
    // The audio clips for when the mine explodes.
    public AudioClip[] ExplosionAudio;

    private void Start() {
      ExplosionForce = (float)FirebaseRemoteConfig.GetValue(
        StringConstants.RemoteConfigMineTileForce).DoubleValue;
      ExplosionRadius = (float)FirebaseRemoteConfig.GetValue(
        StringConstants.RemoteConfigMineTileRadius).DoubleValue;
      UpwardsModifier = (float)FirebaseRemoteConfig.GetValue(
        StringConstants.RemoteConfigMineTileUpwardsMod).DoubleValue;
    }

    public void FixedUpdate() {
      if (Cooldown > 0.0f) {
        Cooldown -= Time.fixedDeltaTime;
        if (Cooldown <= 0.0f) {
          FinishCooldown();
        }
      }
    }

    public void StartCooldown() {
      Cooldown = TotalCooldownTime;
    }

    public void FinishCooldown() {
      Cooldown = 0.0f;
      // Unhide the child mine object.
      if (Mine != null) {
        Mine.SetActive(true);
      }
    }

    public override void Reset() {
      FinishCooldown();
      // Reset the animation back to default.
      foreach (Animator animator in transform.root.GetComponentsInChildren<Animator>()) {
        animator.enabled = true;
        animator.Play(StringConstants.AnimationMineIdleState);
      }
    }

    // When the player rolls off of the mine is when we want to trigger it.
    void OnTriggerExit(Collider collider) {
      if (Cooldown <= 0.0f && collider.GetComponent<PlayerController>() != null) {
        StartCooldown();
        // Trigger the mine animation.
        foreach (Animator animator in transform.root.GetComponentsInChildren<Animator>()) {
          animator.enabled = true;
          animator.SetTrigger(StringConstants.AnimationLaunchMine);
        }

        // Play the audio for launching the mine.
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null) {
          audioSource.clip = LaunchMineAudio;
          audioSource.Play();
        }
      }
    }

    // Called when the animation has reached the spot to explode the mine.
    void ExplodeMine() {
      Transform mineLocation = transform;
      if (Mine != null) {
        mineLocation = Mine.transform;
        // Hide the mine child.
        Mine.SetActive(false);
      }

      PlayerController pc = CommonData.mainGame.PlayerController;
      if (pc != null && DamageAmount != 0) {
        float distance = Vector3.Distance(pc.transform.position, mineLocation.position);
        if (distance < ExplosionRadius)
          pc.Hit(DamageAmount);
      }

      // Spawn the explosion where the mine was. Note the objects are in charge of
      // cleaning themselves up.
      GameObject.Instantiate(ExplosionPrefab, mineLocation.position, mineLocation.rotation);
      // The shrapnel animation is based off of the tile, so spawn there instead.
      GameObject.Instantiate(ShrapnelPrefab, transform.position, transform.rotation);

      if (CommonData.mainGame.player != null) {
        // Apply the explosion force to the player ball.
        Rigidbody playerRB = CommonData.mainGame.player.GetComponent<Rigidbody>();
        playerRB.AddExplosionForce(ExplosionForce, mineLocation.position, ExplosionRadius,
          UpwardsModifier, ForceMode.Impulse);
      }

      // Play the audio for exploding the mine.
      AudioSource audioSource = GetComponent<AudioSource>();
      if (audioSource != null) {
        audioSource.PlayRandom(ExplosionAudio);
      }
    }
  }
}
