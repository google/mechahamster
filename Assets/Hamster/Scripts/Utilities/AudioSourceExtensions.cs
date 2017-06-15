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

namespace Hamster.Utilities {
  // Class to add extensions to Unity's AudioSource component.
  public static class AudioSourceExtensions {
    // Plays a random clip from the list provided on the audio source.
    public static AudioClip PlayRandom(this AudioSource audioSource, AudioClip[] audioClips) {
      if (audioClips != null && audioClips.Length > 0) {
        int index = Random.Range(0, audioClips.Length);
        audioSource.clip = audioClips[index];
        audioSource.Play();
        return audioClips[index];
      }
      return null;
    }
  }
}
