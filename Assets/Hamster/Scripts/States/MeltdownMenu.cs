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
using Firebase.Crashlytics;
using UnityEngine;
using UnityEngine.UI;

namespace Hamster.States {
  /// <summary>
  /// The state controller for the MeltdownMenu. This menu appears
  /// after the SelfDestruct button in the Settings menu has been
  /// clicked a certain number of times. Once the MenuPanel
  /// has faded to nothing, it will return the state to the MainMenu.
  /// </summary>
  public class MeltdownMenu : BaseState {
    // The GameObject that will control text on the screen
    private Menus.MeltdownMenuGUI meltdownGUI;

    public override void Initialize() {
      InitializeUI();
    }

    public override void Resume(StateExitValue results) {
      InitializeUI();
    }

    private void InitializeUI() {
      LogCrashlyticsException();
      if (meltdownGUI == null) {
        meltdownGUI = SpawnUI<Menus.MeltdownMenuGUI>(StringConstants.PrefabMeltdownMenu);
      }
      meltdownGUI.MenuPanel.gameObject.SetActive(true);
      ShowUI();
    }

    public override void Suspend() {
      HideUI();
    }

    public override StateExitValue Cleanup() {
      DestroyUI();
      return null;
    }

    public override void Update() {
      if (meltdownGUI.MenuPanel.HasFadedToNothing()) {
        ReturnToMainMenu();
      }
    }

    private void ReturnToMainMenu() {
      // This is currently two menu levels in,
      // this is a bit hard-coded at the moment
      // but being used to show a prototype
      manager.PopState();
      manager.PopState();
    }

    /// <summary>
    /// Log an exception to Crashlytics.
    /// </summary>
    /// <exception cref="CrashlyticsCaughtException"></exception>
    private void LogCrashlyticsException() {
      // This is a bit odd, throwing the exception and then catching it immediately,
      // but this allows crashlytics to be able to capture the stack trace.
      PseudoRandomExceptionChooser.Throw("User hit the \"Self Destruct!\" button too many times.");
    }
  }
}