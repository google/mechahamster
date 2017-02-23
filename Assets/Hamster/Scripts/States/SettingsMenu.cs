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

namespace Hamster.States {
  // State for changing the game settings, such as music volume.
  class SettingsMenu : BaseState {
    private Menus.SettingsGUI menuComponent;

    // The list of buttons controlling the volumes.
    List<GameObject> MusicVolumeButtons = new List<GameObject>();
    List<GameObject> SoundFxVolumeButtons = new List<GameObject>();

    public override void Initialize() {
      InitializeUI();
    }

    public override void Resume(StateExitValue results) {
      InitializeUI();
    }

    private void InitializeUI() {
      menuComponent = SpawnUI<Menus.SettingsGUI>(StringConstants.PrefabsSettingsMenu);
      menuComponent.gameObject.SetActive(true);

      // Set up the buttons that control the music volume.
      if (MusicVolumeButtons.Count != MainGame.MaxVolumeValue + 1) {
        MusicVolumeButtons.Clear();
        foreach (Transform child in menuComponent.MusicButtonHolders.transform) {
          GameObject.Destroy(child.gameObject);
        }
        for (int i = 0; i <= MainGame.MaxVolumeValue; ++i) {
          MusicVolumeButtons.Add(GameObject.Instantiate(
            menuComponent.VolumeButtonPrefab,
            menuComponent.MusicButtonHolders.transform, false));
        }
      }
      // Set up the buttons that control the sound effect volume.
      if (SoundFxVolumeButtons.Count != MainGame.MaxVolumeValue + 1) {
        SoundFxVolumeButtons.Clear();
        foreach (Transform child in menuComponent.SoundFxButtonHolders.transform) {
          GameObject.Destroy(child.gameObject);
        }
        for (int i = 0; i <= MainGame.MaxVolumeValue; ++i) {
          SoundFxVolumeButtons.Add(GameObject.Instantiate(
            menuComponent.VolumeButtonPrefab,
            menuComponent.SoundFxButtonHolders.transform, false));
        }
      }

      UpdateVolumeColors();
    }

    // Updates the volume button's color based on the volume settings.
    private void UpdateVolumeColors() {
      for (int i = 0; i <= MainGame.MaxVolumeValue; ++i) {
        MusicVolumeButtons[i].GetComponent<UnityEngine.UI.Image>().color =
          (i <= CommonData.mainGame.MusicVolume)
            ? menuComponent.VolumeOn
            : menuComponent.VolumeOff;
        SoundFxVolumeButtons[i].GetComponent<UnityEngine.UI.Image>().color =
          (i <= CommonData.mainGame.SoundFxVolume)
            ? menuComponent.VolumeOn
            : menuComponent.VolumeOff;
      }
    }

    public override void Suspend() {
      menuComponent.gameObject.SetActive(false);
    }

    public override StateExitValue Cleanup() {
      MusicVolumeButtons.Clear();
      DestroyUI();
      return null;
    }

    public override void HandleUIEvent(GameObject source, object eventData) {
      if (source == menuComponent.MainButton.gameObject) {
        manager.PopState();
      } else {
        // Check the volume buttons, and update the appropriate volume if necessary.
        bool volumeChanged = false;
        for (int i = 0; i <= MainGame.MaxVolumeValue; ++i) {
          if (MusicVolumeButtons[i] == source) {
            volumeChanged = true;
            CommonData.mainGame.MusicVolume = i;
            break;
          }
        }
        if (!volumeChanged) {
          for (int i = 0; i <= MainGame.MaxVolumeValue; ++i) {
            if (SoundFxVolumeButtons[i] == source) {
              volumeChanged = true;
              CommonData.mainGame.SoundFxVolume = i;
              break;
            }
          }
        }
        if (volumeChanged) {
          UpdateVolumeColors();
        }
      }
    }
  }
}
