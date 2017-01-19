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
    // Width/Height of the menu, expressed as a portion of the screen width:
    private const float kMenuWidth = 0.25f;
    private const float kMenuHeight = 0.75f;
    private const string kButtonNamePlay = "Play";
    private const string kButtonNameBack = "Back";

    // GUI state.
    private Vector2 scrollViewPosition;
    private int mapSelection = 0;

    private LevelMap currentLevel;
    private GUIStyle titleStyle;
    private GUIStyle descriptionStyle;
    private List<PremadeLevelEntry> levels;
    private string[] levelNames;
    private LevelDirectory levelDir;

    private int currentLoadedMap = -1;

    private const float kUIColumnWidth = 0.33f;

    public LevelSelect() {
      // Initialize some styles that we'll for the title.
      titleStyle = new GUIStyle();
      titleStyle.alignment = TextAnchor.UpperCenter;
      titleStyle.fontSize = 50;

      descriptionStyle = new GUIStyle();
      descriptionStyle.alignment = TextAnchor.UpperCenter;
      descriptionStyle.fontSize = 20;
    }

    const string kLevelDirectoryJson = "LevelList";

    // Update function, which gets called once per frame.
    public override void Update() {
      // If they've got a different map selected than the one we have loaded,
      // load the new one!
      if (currentLoadedMap != mapSelection) {
        currentLoadedMap = mapSelection;
        TextAsset json = Resources.Load(levelDir.levels[currentLoadedMap].filename) as TextAsset;
        currentLevel = JsonUtility.FromJson<LevelMap>(json.ToString());
        CommonData.gameWorld.DisposeWorld();
        CommonData.gameWorld.SpawnWorld(currentLevel);
      }
    }

    // Initialization method.  Called after the state is added to the stack.
    public override void Initialize() {
      Time.timeScale = 0.0f;
      TextAsset json = Resources.Load(kLevelDirectoryJson) as TextAsset;
      levelDir = JsonUtility.FromJson<LevelDirectory>(json.ToString());

      levelNames = new string[levelDir.levels.Count];

      for (int i = 0; i < levelDir.levels.Count; i++) {
        levelNames[i] = levelDir.levels[i].name;
      }
    }

    // Clean up when we exit the state.
    public override StateExitValue Cleanup() {
      CommonData.gameWorld.DisposeWorld();
      return null;
    }

    // Called once per frame for GUI creation, if the state is active.
    public override void OnGUI() {
      GUI.skin = CommonData.prefabs.guiSkin;

      GUILayout.BeginHorizontal();

      scrollViewPosition = GUILayout.BeginScrollView(scrollViewPosition,
          GUILayout.MaxWidth(Screen.width * 0.33f));
      mapSelection = GUILayout.SelectionGrid(
          mapSelection, levelNames, 1);
      GUILayout.EndScrollView();

      GUILayout.BeginVertical(GUILayout.MaxWidth(Screen.width * kUIColumnWidth));
      GUILayout.Label(levelDir.levels[mapSelection].name, titleStyle);
      GUILayout.Label(levelDir.levels[mapSelection].description, descriptionStyle);
      GUILayout.EndVertical();

      GUILayout.BeginVertical(GUILayout.MaxWidth(Screen.width * kUIColumnWidth));
      if (GUILayout.Button(kButtonNamePlay)) {
        Firebase.Analytics.FirebaseAnalytics.LogEvent(
            StringConstants.AnalyticsEventMapStart,
            StringConstants.AnalyticsParamMapId, CommonData.gameWorld.worldMap.mapId);

        manager.PushState(new Gameplay());
      }
      if (GUILayout.Button(kButtonNameBack)) {
        manager.SwapState(new MainMenu());
      }
      GUILayout.EndVertical();

      GUILayout.EndHorizontal();
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
