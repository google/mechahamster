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
  class BaseLevelSelect : BaseState {
    // Width/Height of the menu, expressed as a portion of the screen width:
    private const float MenuWidth = 0.25f;
    private const float MenuHeight = 0.75f;
    private const float ColumnWidth = 0.33f;

    // GUI state.
    private Vector2 scrollViewPosition;
    private int mapSelection = 0;

    private LevelMap currentLevel;
    private GUIStyle titleStyle;
    private GUIStyle descriptionStyle;
    private string[] levelNames;

    private int currentLoadedMap = -1;

    private const float kUIColumnWidth = 0.33f;

    // Subclasses need to set these:
    protected List<MapListEntry> mapSourceList;
    protected string mapDBPath;

    public BaseLevelSelect() {
      // Initialize some styles that we'll for the title.
      titleStyle = new GUIStyle();
      titleStyle.alignment = TextAnchor.UpperCenter;
      titleStyle.fontSize = 50;
      titleStyle.normal.textColor = Color.white;

      descriptionStyle = new GUIStyle();
      descriptionStyle.alignment = TextAnchor.UpperCenter;
      descriptionStyle.fontSize = 20;
      descriptionStyle.normal.textColor = Color.white;
    }

    // Update function, which gets called once per frame.
    public override void Update() {
      // If they've got a different map selected than the one we have loaded,
      // load the new one!
      if (currentLoadedMap != mapSelection) {
        currentLoadedMap = mapSelection;
        manager.PushState(new WaitingForDBLoad<LevelMap>(
          mapDBPath + mapSourceList[mapSelection].mapId));
      }
    }

    public override void Resume(StateExitValue results) {
      if (results != null) {
        if (results.sourceState == typeof(WaitingForDBLoad<LevelMap>)) {
          var resultData = results.data as WaitingForDBLoad<LevelMap>.Results;
          if (resultData.wasSuccessful) {
            currentLevel = resultData.results;
            CommonData.gameWorld.DisposeWorld();
            CommonData.gameWorld.SpawnWorld(currentLevel);
            Debug.Log("Map load complete!");
          } else {
            Debug.LogWarning("Map load complete, but not successful...");
          }
        }
      }
    }

    // Initialization method.  Called after the state is added to the stack.
    public override void Initialize() {
      levelNames = new string[mapSourceList.Count];

      for (int i = 0; i < mapSourceList.Count; i++) {
        levelNames[i] = mapSourceList[i].name;
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
          GUILayout.MaxWidth(Screen.width * ColumnWidth));
      mapSelection = GUILayout.SelectionGrid(
          mapSelection, levelNames, 1);
      GUILayout.EndScrollView();

      GUILayout.BeginVertical(GUILayout.MaxWidth(Screen.width * kUIColumnWidth));
      GUILayout.Label(mapSourceList[mapSelection].name, titleStyle);
      GUILayout.EndVertical();

      GUILayout.BeginVertical(GUILayout.MaxWidth(Screen.width * kUIColumnWidth));
      if (GUILayout.Button(StringConstants.ButtonPlay)) {
        Firebase.Analytics.FirebaseAnalytics.LogEvent(
            StringConstants.AnalyticsEventMapStart,
            StringConstants.AnalyticsParamMapId, CommonData.gameWorld.worldMap.mapId);

        manager.PushState(new Gameplay());
      }
      if (GUILayout.Button(StringConstants.ButtonCancel)) {
        manager.SwapState(new MainMenu());
      }
      GUILayout.EndVertical();

      GUILayout.EndHorizontal();
    }
  }
}
