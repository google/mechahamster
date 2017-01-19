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
using System.Collections;

namespace Hamster.MapObjects {

  // Clsas that governms behavior of goal tiles - tiles that
  // complete the level when stepped on.
  // Note that this code is largely placeholder - for now it just displays
  // a message when stepped on.  Final flow will be a bit different.
  public class GoalTile : MapObject {
    System.DateTime lastTouch = System.DateTime.MinValue;

    // At the moment, these just display a win message.
    // TODO: End game, bring up different menu, etc.
    protected override void MapObjectActivation(Collider collider) {
      lastTouch = System.DateTime.Now;
    }

    void OnGUI() {
      System.TimeSpan delta = System.DateTime.Now.Subtract(lastTouch);
      if (delta.TotalSeconds <= 5) {
        GUI.skin = FindObjectOfType<PrefabList>().guiSkin;

        UnityEngine.GUIStyle centeredStyle = GUI.skin.GetStyle("Label");
        centeredStyle.alignment = TextAnchor.UpperCenter;
        GUI.Label(new Rect(Screen.width / 2 - 400,
          Screen.height / 2 - 50, 800, 100), "A WINNNER IS YOU", centeredStyle);
      }
    }
  }
}
