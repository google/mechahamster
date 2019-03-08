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
using UnityEngine.UI;

namespace Hamster.Menus {

  /// <summary>
  /// A menu component with some number of text fields that will fade to
  /// nothing over time.
  /// </summary>
  public class MenuMeltdownAnim : MonoBehaviour {

    private const float TARGET_FADE_COLOR = 0.0f;
    private const float FADE_RATE = 0.5f;

    /// <summary>
    /// All text present on the menu.
    /// </summary>
    public Text[] ScreenText;

    private Image menuPanelImage;

    /// <summary>
    /// Grab the image component for the menu panel on startup to ensure that
    /// the we can fade the Image backing the menu.
    /// </summary>
    public void Start() {
      menuPanelImage = GetComponent<Image>();
    }

    /// <summary>
    /// For the main menu panel and text on the menu, fade the color on every
    /// update.
    /// </summary>
    public void Update() {
      Color currentTextColor;
      Color currentBackgroundColor = menuPanelImage.color;
      float originalAlpha = currentBackgroundColor.a;
      float currentColorDiff = Mathf.Abs(originalAlpha - TARGET_FADE_COLOR);
      float newAlpha = FadeColor(menuPanelImage.color);
      Color newColor = new Color(currentBackgroundColor.r,
        currentBackgroundColor.g, currentBackgroundColor.b, newAlpha);
      menuPanelImage.color = newColor;

      foreach (var textObject in ScreenText) {
        currentTextColor = textObject.color;
        newAlpha = FadeColor(currentTextColor);
        newColor = new Color(currentTextColor.r, currentTextColor.g, currentTextColor.b,
          newAlpha);
        textObject.color = newColor;
      }
    }

    /// <summary>
    /// Fade a single color by the FADE_RATE.
    /// </summary>
    /// <param name="originalColor">The color we are fading</param>
    /// <returns>The new alpha to assign to the color</returns>
    private float FadeColor(Color originalColor) {
      float originalAlpha = originalColor.a;
      float currentColorDiff = Mathf.Abs(originalAlpha - TARGET_FADE_COLOR);
      float newColorAlpha = originalColor.a;
      if (currentColorDiff > 0.0001f) {
        newColorAlpha = Mathf.Lerp(newColorAlpha, TARGET_FADE_COLOR,
          FADE_RATE * Time.fixedDeltaTime);
      }
      return newColorAlpha;
    }

    /// <summary>
    /// Check if the menu has faded away.
    /// </summary>
    /// <returns></returns>
    public bool HasFadedToNothing() {
      if (menuPanelImage == null) {
        return false;
      }

      if (!SingleColorHasFadedToNothing(menuPanelImage.color)) {
        return false;
      }

      foreach (var textObject in ScreenText) {
        if (!SingleColorHasFadedToNothing(textObject.color)) {
          return false;
        }
      }

      return true;
    }

    /// <summary>
    /// Check if a single color has faded to nothing (within 0.01f of nothing)
    /// </summary>
    /// <param name="color">the color we are checking</param>
    /// <returns>
    /// true if the color has faded away sufficiently, false otherwise
    /// </returns>
    private bool SingleColorHasFadedToNothing(Color color) {
      float colorDiff = Mathf.Abs(color.a - TARGET_FADE_COLOR);
      if (colorDiff > 0.01f) {
        return false;
      }

      return true;
    }

  }
}