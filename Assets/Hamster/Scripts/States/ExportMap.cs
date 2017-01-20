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

namespace Hamster.States {
  // Utility state for exporing levels to json text files.
  // Used for creating/updating prepackaged levels.
  class ExportMap : BaseState {
    string jsonExport;
    string fileName = "export.txt";

    public ExportMap(LevelMap currentLevel) {
      jsonExport = JsonUtility.ToJson(currentLevel, true);
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
