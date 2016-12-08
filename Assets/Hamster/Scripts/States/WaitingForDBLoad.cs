using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Firebase.Unity.Editor;


namespace Hamster.States {
  class WaitingForDBLoad : BaseState {

    DBTable<LevelMap> mapTable;

    public WaitingForDBLoad(DBTable<LevelMap> mapTable) {
      this.mapTable = mapTable;
    }

    // Initialization method.  Called after the state
    // is added to the stack.
    public override void Initialize() {
      Time.timeScale = 0.0f;
    }

    // Resume the state.  Called when the state becomes active
    // when the state above is removed.  That state may send an
    // optional object containing any results/data.  Results
    // can also just be null, if no data is sent.
    public override void Resume(StateExitValue results) {
      Time.timeScale = 0.0f;
    }

    // Called once per frame when the state is active.
    public override void Update() {
      if (mapTable.areChangesPending) {
        mapTable.ApplyRemoteChanges();
        if (mapTable.data.ContainsKey("test_map")) {
          LevelMap worldMap = mapTable.data["test_map"].data;
          CommonData.gameWorld.DisposeWorld();
          CommonData.gameWorld.SpawnWorld(worldMap);
          manager.PopState();
        }
      }
    }

    // Called once per frame for GUI creation, if the state is active.
    public override void OnGUI() {
      GUI.skin = CommonData.prefabs.guiSkin;
      UnityEngine.GUIStyle centeredStyle = GUI.skin.GetStyle("Label");
      centeredStyle.alignment = TextAnchor.UpperCenter;
      GUI.Label(new Rect(Screen.width / 2 - 400,
        Screen.height / 2 - 50, 800, 100), "Loading...", centeredStyle);
    }
  }
}
