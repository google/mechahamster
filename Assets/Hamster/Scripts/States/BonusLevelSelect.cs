using UnityEngine;
using System.Collections.Generic;

namespace Hamster.States {
  class BonusLevelSelect : BaseState {
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

    public BonusLevelSelect() {
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
        List<MapListEntry> bonusMaps = CommonData.currentUser.data.bonusMaps;
        manager.PushState(new WaitingForDBLoad<LevelMap>(
          CommonData.DBBonusMapTablePath + bonusMaps[mapSelection].mapId));
      }
    }

    public override void Resume(StateExitValue results) {
      Time.timeScale = 0.0f;
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
      Time.timeScale = 0.0f;
      List<MapListEntry> bonusMaps = CommonData.currentUser.data.bonusMaps;
      levelNames = new string[bonusMaps.Count];

      for (int i = 0; i < bonusMaps.Count; i++) {
        levelNames[i] = bonusMaps[i].name;
      }
    }

    // Clean up when we exit the state.
    public override StateExitValue Cleanup() {
      CommonData.gameWorld.DisposeWorld();
      return null;
    }

    // Called once per frame for GUI creation, if the state is active.
    public override void OnGUI() {
      List<MapListEntry> bonusMaps = CommonData.currentUser.data.bonusMaps;
      GUI.skin = CommonData.prefabs.guiSkin;

      GUILayout.BeginHorizontal();

      scrollViewPosition = GUILayout.BeginScrollView(scrollViewPosition,
          GUILayout.MaxWidth(Screen.width * ColumnWidth));
      mapSelection = GUILayout.SelectionGrid(
          mapSelection, levelNames, 1);
      GUILayout.EndScrollView();

      GUILayout.BeginVertical(GUILayout.MaxWidth(Screen.width * kUIColumnWidth));
      GUILayout.Label(bonusMaps[mapSelection].name, titleStyle);
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
