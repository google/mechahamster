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

namespace Hamster.Menus {

  // Interface class for providing code access to the GUI
  // elements in the settings prefab.
  public class SettingsGUI : BaseMenu {

    // These fields are set in the inspector.
    public GUIButton MainButton;
    public GUIButton SelfDestructButton;
    public GUIButton TermsAndServicesButton;
    public GUIButton PrivacyButton;
    public GameObject MusicButtonHolders;
    public GameObject SoundFxButtonHolders;

    // The prefab of the volume buttons, that are spawned into the holders.
    public GameObject VolumeButtonPrefab;

    // The colors used when the volume button should be on/off.
    public Color VolumeOn;
    public Color VolumeOff;
  }
}
