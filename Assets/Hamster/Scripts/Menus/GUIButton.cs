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
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace Hamster.Menus {

  // Script for buttons in the GUI.  Simply listens for clicks, and
  // reports them to the state manager.  (Which reports them to the
  // currently active game state.)
  // Also handles mouseover animations.
  public class GUIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
      IPointerDownHandler, IPointerUpHandler, IPointerClickHandler {

    // How much the scale oscilates in either direction while the button hovers.
    const float ButtonScaleRange = 0.15f;
    // The frequency of the oscilations, in oscilations-per-2Pi seconds.
    const float ButtonScaleFrequency = 6.0f;
    // How the scale increase when the button is being pressed.
    const float ButtonScalePressed = 0.4f;
    // How fast the scale transitions when changing states, in %-per-frame.
    const float transitionSpeed = 0.08f;

    bool hover = false;
    bool press = false;
    float currentScale = 1.0f;

    UnityEngine.UI.Button buttonComponent;

    Vector3 startingScale;

    private void Awake() {
      startingScale = transform.localScale;
    }

    private void Update() {
      float targetScale = 1.0f;
      if (press) {
        targetScale = 1.0f + ButtonScalePressed * 3.0f;
      } else if (hover) {
        targetScale = 1.0f + ButtonScaleRange +
            Mathf.Sin(Time.realtimeSinceStartup * ButtonScaleFrequency) * ButtonScaleRange;
      }
      currentScale = currentScale * (1.0f - transitionSpeed) + targetScale * transitionSpeed;
      transform.localScale = startingScale * currentScale;
    }

    public void OnPointerClick(PointerEventData eventData) {
      CommonData.mainGame.stateManager.HandleUIEvent(gameObject, null);
    }

    public void OnPointerDown(PointerEventData eventData) {
      press = true;
    }

    public void OnPointerUp(PointerEventData eventData) {
      press = false;
    }

    public void OnPointerEnter(PointerEventData eventData) {
      hover = true;
    }

    public void OnPointerExit(PointerEventData eventData) {
      hover = false;
    }
  }

}