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

  // Script for buttons in the GUI.  Simply listens for clicks, and
  // reports them to the state manager.  (Which reports them to the
  // currently active game state.)
  public class GUIButton : MonoBehaviour {

    UnityEngine.UI.Button buttonComponent;

    // Use this for initialization
    void Start() {
      buttonComponent = GetComponent<UnityEngine.UI.Button>();
      if (buttonComponent != null) {
        buttonComponent.onClick.AddListener(OnClick);
      }
    }

    void OnClick() {
      CommonData.mainGame.stateManager.HandleUIEvent(gameObject, null);
    }

    void OnDestroy() {
      if (buttonComponent != null)
        buttonComponent.onClick.RemoveListener(OnClick);
    }
  }

}