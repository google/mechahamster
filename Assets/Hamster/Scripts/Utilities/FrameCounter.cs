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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hamster.Utilities {

  // Utility script to display framerate.
  public class FrameCounter : MonoBehaviour {

    float lastFrameStart = 0.0f;
    int frameCount = 0;
    int currentlyDisplayedFrames = 0;
    GUIStyle style = new GUIStyle();

    private void Start() {
      style.fontSize = 30;
      style.normal.textColor = Color.white;
    }

    private void Update() {
      float currentTime = Time.realtimeSinceStartup;
      if (lastFrameStart + 1.0f < currentTime) {
        currentlyDisplayedFrames = frameCount;
        frameCount = 0;
        lastFrameStart = currentTime;
      } else {
        frameCount++;
      }
    }

    private void OnGUI() {
      GUILayout.Label(currentlyDisplayedFrames + "fps", style);
    }

  }
}
