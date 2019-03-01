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

using System.Runtime.CompilerServices;
using Firebase.Crashlytics;
using UnityEngine;

namespace Hamster.States {
  /// <summary>
  /// A Warning menu that lets the user know how many clicks are left until the game will
  /// log an exception to Crashlytics and return to the MainMenu.
  /// </summary>
  public class SelfDestructMenu : BaseState {
    // The GameObject that will control text on the screen
    private Menus.SelfDestructMenuGUI selfDestructMenuComponent;

    // a counter for the number of times that a User can click
    // the SelfDestructButton before it logging and exception
    // and shutting down.
    private const int DESTRUCTION_CLICK_LIMIT = 3;
    private static int destructionClickCountdown = DESTRUCTION_CLICK_LIMIT;

    public static int DestructionClickCount {
      [MethodImpl(MethodImplOptions.Synchronized)]
      get { return destructionClickCountdown; }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public static void DecrementDesctructionClickCount() {
      destructionClickCountdown--;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public static void ResetDestructionClickCount() {
      destructionClickCountdown = DESTRUCTION_CLICK_LIMIT;
    }

    public override void Initialize() {
      InitializeUI();
    }

    public override void Resume(StateExitValue results) {
      InitializeUI();
    }

    private void InitializeUI() {
      if (selfDestructMenuComponent == null) {
        selfDestructMenuComponent = SpawnUI<Menus.SelfDestructMenuGUI>(StringConstants.PrefabSelfDestructMenu);
      }

      selfDestructMenuComponent.ClicksRemainingText.text = destructionClickCountdown.ToString();
      ShowUI();
      DecrementDesctructionClickCount();
    }

    public override void Suspend() {
      HideUI();
    }

    public override StateExitValue Cleanup() {
      DestroyUI();
      return null;
    }

    /// <summary>
    /// If the user clicks the SelfDestructMenuGUI.SettingsButton, return to
    /// the settings page.
    /// </summary>
    /// <param name="source">The GameObject that was clicked</param>
    /// <param name="eventData"></param>
    public override void HandleUIEvent(GameObject source, object eventData) {
      if (source == selfDestructMenuComponent.SettingsButton.gameObject) {
        manager.PopState();
      }
    }
  }
}