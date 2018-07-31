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
using System.Collections.Generic;

namespace Hamster.Menus {

  // Base class for UI menus.  Will normally be inherrited from,
  // by child classes that expose the UI elements.
  public class BaseMenu : MonoBehaviour {

    // Depth to render menus at, in world-units.
    const float MenuRenderDepth = 10.0f;
    static Vector3 MenuOffset = new Vector3(0, 0, MenuRenderDepth);

    private void Awake() {
      Canvas canvas = GetComponent<Canvas>();
      if (canvas == null) {
        // Prefabs that use this class are required to
        // have a canvas component.
        Debug.LogError("UI Menu could not find canvas!");
      } else {
        RectTransform rt = canvas.GetComponent<RectTransform>();
        rt.SetPositionAndRotation(MenuOffset, Quaternion.identity);
        // Set up canvas input.
        if (CommonData.inVrMode) {
          gameObject.AddComponent<GvrPointerGraphicRaycaster>();
        } else {
          gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
      }
    }
  }
}
