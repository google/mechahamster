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
using System.Collections;

namespace Hamster.InputControllers {

  // Class for handling the player's input while playing back a
  // replay.  Plays back whatever the player entered, and occasionally
  // sets the ball position directly, to compensate for physics drift.
  public class ReplayController : BasePlayerController {

    States.Gameplay gameplayState;
    Vector2 playerInputVector = Vector2.zero;

    ReplayData replayData;

    int inputDataIndex = 0;
    int positionDataIndex = 0;

    public ReplayController(string replayFileName, States.Gameplay gameplayState) {
      this.gameplayState = gameplayState;

      // Load binary replay file as TextAsset.  Unity supports .bytes extension.
      TextAsset asset = Resources.Load(replayFileName) as TextAsset;
      replayData = ReplayData.CreateFromStream(new System.IO.MemoryStream(asset.bytes));

      if (replayData.mapHash != CommonData.gameWorld.mapHash) {
        Debug.LogWarning("Warning:  Map does not match playback data!\n" +
          "Replay: " + replayData.mapHash + "\n" +
          "Map:" + CommonData.gameWorld.mapHash);
        }
      }

    public override Vector2 GetInputVector() {
      int timestamp = gameplayState.fixedUpdateTimestamp;

      if (replayData != null && timestamp >= 0) {
        while (inputDataIndex < replayData.inputData.Length &&
            replayData.inputData[inputDataIndex].timestamp < timestamp) {
          inputDataIndex++;
        }
        while (positionDataIndex < replayData.positionData.Length &&
            replayData.positionData[positionDataIndex].timestamp < timestamp) {
          positionDataIndex++;
        }

        if (positionDataIndex < replayData.positionData.Length &&
            replayData.positionData[positionDataIndex].timestamp == timestamp) {
          PositionDataEntry entry = replayData.positionData[positionDataIndex];
          gameplayState.SetPlayerPosition(
            entry.position, entry.rotation, entry.velocity, entry.angularVelocity);
        }

        if (inputDataIndex < replayData.inputData.Length &&
            replayData.inputData[inputDataIndex].timestamp == timestamp) {
          playerInputVector = replayData.inputData[inputDataIndex].playerInputVector;
        }

        // If we run out of replay data, count it as a finishing the level.
        if (inputDataIndex >= replayData.inputData.Length) {
          CommonData.mainGame.PlayerController.HandleGoalCollision();
        }

      }
      return playerInputVector;
    }
  }
}
