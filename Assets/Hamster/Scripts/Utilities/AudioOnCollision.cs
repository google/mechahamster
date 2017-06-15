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

using System;
using UnityEngine;

namespace Hamster.Utilities {
  // Plays one of the attached audio clips when a collision or trigger
  // event happens.
  class AudioOnCollision : MonoBehaviour {
    // The minimum impulse required to trigger a sound.
    public float MinImpulseMagnitude = 1.0f;
    // The impulse value that is mapped to max volume. Note that volume scales from
    // the min impulse to this.
    public float MaxImpulseMagnitude = 12.0f;
    // On collision, a random clip from this list is played.
    public AudioClip[] AudioClips = null;

    // The audio source component to play the clips one.
    AudioSource AudioSource { get; set; }
    // The time to wait for until disabling the audio source, in seconds.
    private float disableAtTime = 0;

    private void Start() {
      // Get the audio source, adding one if necessary.
      AudioSource = GetComponent<AudioSource>();
      if (AudioSource == null) {
        AudioSource = gameObject.AddComponent<AudioSource>();
      }
      // We disable the AudioSource by default, and only enable it when we are playing a clip.
      // We do this since it seems even when no clip is playing the AudioSources are still taking
      // update time. Note, this script will not work with other scripts that try to use the
      // AudioSource.
      AudioSource.enabled = false;
    }

    private void Update() {
      // If the clip has finished, disable the AudioSource to potentially save update time.
      if (Time.realtimeSinceStartup >= disableAtTime) {
        AudioSource.enabled = false;
      }
    }

    // Helper function to play a random clip. Handles enabling/disabling the AudioSource.
    private void PlayRandomClip() {
      AudioSource.enabled = true;
      AudioClip clip = AudioSource.PlayRandom(AudioClips);
      if (clip != null) {
        disableAtTime = Time.realtimeSinceStartup + clip.length;
      } else {
        AudioSource.enabled = false;
      }
    }

    // If the game object uses collision, play the audio based on the collision.
    private void OnCollisionEnter(Collision collision) {
      float magnitude = collision.impulse.magnitude;
      if (magnitude < MinImpulseMagnitude) {
        return;
      }
      // Scale the volume based on the impulse and the given min/max.
      float volume =
        Mathf.InverseLerp(MinImpulseMagnitude, MaxImpulseMagnitude, magnitude);
      AudioSource.volume = volume;
      PlayRandomClip();
    }

    // If the game object uses triggers, play the audio whenever the trigger enter happens.
    private void OnTriggerEnter(Collider other) {
      PlayRandomClip();
    }
  }
}
