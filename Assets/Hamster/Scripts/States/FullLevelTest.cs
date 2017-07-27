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
using Hamster.Utilities;

namespace Hamster.States {
  // Test loop class, for automated testing.
  // Loads up a level + replay data, plays through it,
  // and then exits the app.
  internal class FullLevelTest : BaseState {

    private int nextLevelIndex;
    private LevelSelect.LevelDirectory levelDir;
    private PerformanceProfiler profiler;
    private GameObject profilerObj;
    private List<PerformanceProfiler.PerformanceData> performanceData =
        new List<PerformanceProfiler.PerformanceData>();

    public override void Initialize() {
      nextLevelIndex = -1;

      profilerObj = new GameObject();
      profiler = profilerObj.AddComponent<PerformanceProfiler>();
      profiler.Initialize();

      TextAsset json = Resources.Load(LevelSelect.LevelDirectoryJson) as TextAsset;
      levelDir = JsonUtility.FromJson<LevelSelect.LevelDirectory>(json.ToString());

      if (!StartNextLevel()) {
        manager.SwapState(new BasicDialog("No tests found."));
      }
    }

    // When we get back from the gameplay state, load the next map and continue.
    public override void Resume(StateExitValue results) {
      profiler.Finish();
      PerformanceProfiler.PerformanceData data = profiler.GetDataSnapshot();
      data.levelIndex = nextLevelIndex;
      performanceData.Add(data);
      if (!StartNextLevel()) {
        Debug.Log(GenerateFullReport());
        manager.SwapState(new BasicDialog("Tests completed."));
      } else {
        Debug.Log(GenerateLevelReport(data));
      }
    }

    private string GenerateFullReport() {
      string result = "";
      result += "-----------------------------------";
      result += "\n-----------------------------------";
      result += "\nperformance results:";
      result += "\n";

      for (int i = 0; i < performanceData.Count; i++) {
        result += "\n" + GenerateLevelReport(performanceData[i]);
      }

      result += "\n-----------------------------------";
      result += "\n-----------------------------------";
      return result;
    }

    private string GenerateLevelReport(PerformanceProfiler.PerformanceData data) {
      string result = "\nLevel: " + levelDir.levels[data.levelIndex].name;
      result += "\n-----------------------------------";
      result += "\nfps: " + data.fps;
      result += "\nFrame Time: (" + data.minFrameTime;
      result += " - " + data.maxFrameTime + ")";
      result += "\nMean: " + data.meanFrameTime;
      result += "\nMedian: " + data.medianFrameTime;
      result += "\nStd Dev: " + data.frameTimeStdDev;
      result += "\nTotal Time: " + data.totalTime;
      result += "\nSamples: " + data.totalSamples;
      return result;
    }

    // Loads the next level, if one is available, and starts it running with the
    // profiler.  If there are no levels left, returns false.
    private bool StartNextLevel() {
      CommonData.gameWorld.DisposeWorld();
      nextLevelIndex++;
      while (nextLevelIndex < levelDir.levels.Count &&
          levelDir.levels[nextLevelIndex].replay == null) {
        nextLevelIndex++;
      }
      if (nextLevelIndex >= levelDir.levels.Count) {
        CommonData.currentReplayData = null;
        return false;
      }

      CommonData.currentReplayData = levelDir.levels[nextLevelIndex].replay;

      TextAsset json = Resources.Load(levelDir.levels[nextLevelIndex].filename) as TextAsset;
      LevelMap currentLevel = JsonUtility.FromJson<LevelMap>(json.ToString());
      currentLevel.DatabasePath = null;
      CommonData.gameWorld.SpawnWorld(currentLevel);

      manager.PushState(new Gameplay(Gameplay.GameplayMode.TestLoop));
      profiler.Initialize();
      return true;
    }

  }
}