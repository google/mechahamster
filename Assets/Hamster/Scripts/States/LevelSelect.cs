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
  class LevelSelect : BaseState {
    Menus.LevelSelectGUI menuComponent;

    private int mapSelection = 0;
    private int currentPage = 0;
    private LevelMap currentLevel;
    private string[] levelNames;
    private LevelDirectory levelDir;

    private int currentLoadedMap = -1;

    // Layout constants.
    private const int ButtonsPerPage = 5;
    private const float ColumnPadding = 50;

    public LevelSelect() {
    }

    const string kLevelDirectoryJson = "LevelList";

    Dictionary<int, GameObject> levelButtons = new Dictionary<int, GameObject>();

    // Update function, which gets called once per frame.
    public override void Update() {
      // If they've got a different map selected than the one we have loaded,
      // load the new one!
      if (currentLoadedMap != mapSelection) {
        currentLoadedMap = mapSelection;
        TextAsset json = Resources.Load(levelDir.levels[currentLoadedMap].filename) as TextAsset;
        currentLevel = JsonUtility.FromJson<LevelMap>(json.ToString());
        currentLevel.DatabasePath = null;
        CommonData.gameWorld.DisposeWorld();
        CommonData.gameWorld.SpawnWorld(currentLevel);
      }
    }

    // Initialization method.  Called after the state is added to the stack.
    public override void Initialize() {
      TextAsset json = Resources.Load(kLevelDirectoryJson) as TextAsset;
      levelDir = JsonUtility.FromJson<LevelDirectory>(json.ToString());

      levelNames = new string[levelDir.levels.Count];

      // Generate a list of level names.
      for (int i = 0; i < levelDir.levels.Count; i++) {
        levelNames[i] = levelDir.levels[i].name;
      }

      menuComponent = SpawnUI<Menus.LevelSelectGUI>(StringConstants.PrefabsLevelSelectMenu);
      menuComponent.SelectionText.text = StringConstants.BuiltinLevelScreenTitle;

      SpawnLevelButtons(currentPage);
      ChangePage(0);
    }

    // Removes all buttons from the screen.
    void ClearCurrentButtons() {
      foreach (KeyValuePair<int, GameObject> pair in levelButtons) {
        GameObject.Destroy(pair.Value);
      }
      levelButtons.Clear();
    }

    // Creates one page worth of level buttons for a given page.  Sets their names
    // and properties, and makes sure they're in the correct part of the window.
    // Also removes any existing level buttons.
    void SpawnLevelButtons(int page) {
      ClearCurrentButtons();
      int maxButtonIndex = (currentPage + 1) * ButtonsPerPage;
      if (maxButtonIndex > levelDir.levels.Count) maxButtonIndex = levelDir.levels.Count;
      for (int i = currentPage * ButtonsPerPage; i < maxButtonIndex; i++) {
        GameObject button = GameObject.Instantiate(
            CommonData.prefabs.menuLookup[StringConstants.PrefabsLevelSelectButton]);
        Menus.LevelSelectButtonGUI component = button.GetComponent<Menus.LevelSelectButtonGUI>();
        if (component != null) {
          component.buttonId = i;
          levelButtons[i] = button;
          button.transform.SetParent(menuComponent.Panel.transform, false);
          component.ButtonText.text = levelDir.levels[i].name;
        } else {
          Debug.LogError("Level select button prefab had no LevelSelectButtionGUI component.");
        }

        gui.transform.SetParent(CommonData.mainCamera.transform, false);
      }
    }

    public override void Resume(StateExitValue results) {
      menuComponent.gameObject.SetActive(true);
    }

    public override void Suspend() {
      menuComponent.gameObject.SetActive(false);
    }

    void ChangePage(int delta) {
      currentPage += delta;
      int pageMax = (int)((levelDir.levels.Count) / ButtonsPerPage);
      if (currentPage <= 0) currentPage = 0;
      if (currentPage >= pageMax) currentPage = pageMax;

      menuComponent.BackButton.gameObject.SetActive(currentPage != 0);
      menuComponent.ForwardButton.gameObject.SetActive(currentPage != pageMax);
      SpawnLevelButtons(currentPage);
    }

    public override void HandleUIEvent(GameObject source, object eventData) {
      Menus.LevelSelectButtonGUI buttonComponent =
          source.GetComponent<Menus.LevelSelectButtonGUI>();
      if (source == menuComponent.MainButton.gameObject) {
        manager.SwapState(new MainMenu());
      } else if (source == menuComponent.PlayButton.gameObject) {
        manager.PushState(new Gameplay());
      } else if (source == menuComponent.BackButton.gameObject) {
        ChangePage(-1);
      } else if (source == menuComponent.ForwardButton.gameObject) {
        ChangePage(1);
      } else if (buttonComponent != null) {
        // They pressed one of the buttons for a level.
        mapSelection = buttonComponent.buttonId;
      }
    }

  // Clean up when we exit the state.
  public override StateExitValue Cleanup() {
      DestroyUI();
      CommonData.gameWorld.DisposeWorld();
      return null;
    }

    [System.Serializable]
    public class LevelDirectory {
      public LevelDirectory() { }

      public LevelDirectory(List<PremadeLevelEntry> levels) {
        this.levels = levels;
      }

      public List<PremadeLevelEntry> levels;
    }


    [System.Serializable]
    public class PremadeLevelEntry {
      public string name;
      public string description;
      public string filename;

      public PremadeLevelEntry() {}

      public PremadeLevelEntry(string name, string description, string filename) {
        this.name = name;
        this.description = description;
        this.filename = filename;
      }
    }
  }
}
