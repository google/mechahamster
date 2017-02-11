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

  // Tile that adjusts the ball's drag while it is inside.
  public class DragTile : MapObject {
    public float Drag { get; private set; }

    private void Start() {
      Drag = (float)FirebaseRemoteConfig.GetValue(
        StringConstants.RemoteConfigSandTileDrag).DoubleValue;
    }

    void OnTriggerEnter(Collider collider) {
      Ball.AdjustableDrag adjustableDrag = collider.GetComponent<Ball.AdjustableDrag>();
      if (adjustableDrag != null) {
        adjustableDrag.ApplyDrag(Drag);
      }
    }

    void OnTriggerExit(Collider collider) {
      Ball.AdjustableDrag adjustableDrag = collider.GetComponent<Ball.AdjustableDrag>();
      if (adjustableDrag != null) {
        adjustableDrag.RemoveDrag(Drag);
      }
    }
  }
}
