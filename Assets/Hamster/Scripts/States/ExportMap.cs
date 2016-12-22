using UnityEngine;

namespace Hamster.States {
  // Utility state for exporing levels to json text files.
  // Used for creating/updating prepackaged levels.
  class ExportMap : BaseState {
    string jsonExport;
    string fileName = "export.txt";

    public ExportMap(LevelMap currentLevel) {
      jsonExport = JsonUtility.ToJson(currentLevel, true);
    }

    // Initialization method.  Called after the state
    // is added to the stack.
    public override void Initialize() {
      Time.timeScale = 0.0f;
    }

    // Called once per frame for GUI creation, if the state is active.
    public override void OnGUI() {
      GUI.skin = CommonData.prefabs.guiSkin;
      GUILayout.BeginVertical();
      GUILayout.BeginHorizontal();
      GUILayout.Label("Name:");
      fileName = GUILayout.TextField(fileName, GUILayout.Width(Screen.width * 0.5f));
      GUILayout.EndHorizontal();
      if (GUILayout.Button("Save")) {
        System.IO.StreamWriter outfile = System.IO.File.CreateText(fileName);
        outfile.Write(jsonExport);
        outfile.Close();
        manager.PopState();
      }
      if (GUILayout.Button("Cancel")) {
        manager.PopState();
      }

      GUILayout.EndVertical();
    }
  }
}
