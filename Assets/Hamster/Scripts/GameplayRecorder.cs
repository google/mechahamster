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

using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Hamster {
  // Class for recording gameplay events for playback later.  Also
  // handles serialization of those events to a JSON file.
  public class GameplayRecorder {

    // Lists that we're going to write to a file later.  They represent
    // the recording data.
    List<PositionDataEntry> positionData = new List<PositionDataEntry>();
    List<InputDataEntry> inputData = new List<InputDataEntry>();

    // Input from the player last frame.
    Vector2 previousPlayerInputVector = Vector2.zero;

    // Name of the game level that the play data is associated with.
    string mapId = "";

    // The current gameplay state.  Needed for timestamp synchronization.
    States.Gameplay gameplayState;

    // Constants for controlling how often input and position records are recorded.
    const int positionRecordFrequencyInHz = 30;

    public GameplayRecorder(string mapId) {
      Reset(mapId);
    }

    // Reset everything and prepare to start a new recording.
    public void Reset(string mapId) {
      this.mapId = mapId;
      positionData = new List<PositionDataEntry>();
      inputData = new List<InputDataEntry>();
      previousPlayerInputVector = Vector2.zero;
    }

    // Update function, called once per frame.  Records the current inputs, and
    // occasionally physics + position data, to compensate for floating point drift.
    public void Update(PlayerController playerBall, int timestamp) {
      InputControllers.BasePlayerController playerController = playerBall.inputController;

      if (timestamp % positionRecordFrequencyInHz == 0) {
        Rigidbody rigidbody = playerBall.gameObject.GetComponent<Rigidbody>();
        PositionDataEntry positionDataEntry = new PositionDataEntry(
            timestamp,
            rigidbody.position,
            rigidbody.rotation,
            rigidbody.velocity,
            rigidbody.angularVelocity);

        positionData.Add(positionDataEntry);
      }
      Vector2 playerInputVector = playerBall.inputController.GetInputVector();
      if (previousPlayerInputVector != playerInputVector) {
        InputDataEntry inputDataEntry = new InputDataEntry(timestamp, playerInputVector);

        inputData.Add(inputDataEntry);
        previousPlayerInputVector = playerInputVector;
      }
    }

    // Serializes and outputs the current recording to a file.
    public void OutputToFile(string fileName) {
      System.IO.StreamWriter outfile = System.IO.File.CreateText(fileName);
      outfile.Write(SerializedRecording());
      outfile.Close();
    }

    // Converts the current recording to a json file suitable for serialization.
    string SerializedRecording () {
      ReplayData replayData = new ReplayData(mapId,
          CommonData.gameWorld.mapHash, positionData.ToArray(), inputData.ToArray());
      return JsonUtility.ToJson(replayData, true);
    }

  }

  // Data structures for storing the replay data.
  // Basic structure is a top-level ReplayData object,
  // containing lists of PositionData and InputData entries.
  // Position and Input data has a timestamp, and is guaranteed
  // to be in ascending order, starting from timestamp 0.

  [System.Serializable]
  public class ReplayData {
    public string mapId;
    public int mapHash;
    public PositionDataEntry[] positionData;
    public InputDataEntry[] inputData;

    public ReplayData (string mapId, int mapHash, PositionDataEntry[] positionData,
        InputDataEntry[] inputData) {
      this.mapId = mapId;
      this.mapHash = mapHash;
      this.positionData = positionData;
      this.inputData = inputData;
    }
  }

  [System.Serializable]
  public class PositionDataEntry {
    public int timestamp;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 velocity;
    public Vector3 angularVelocity;

    public PositionDataEntry(int timestamp, Vector3 position, Quaternion rotation,
        Vector3 velocity, Vector3 angularVelocity) {
      this.timestamp = timestamp;
      this.position = position;
      this.rotation = rotation;
      this.velocity = velocity;
      this.angularVelocity = angularVelocity;
    }
  }

  [System.Serializable]
  public class InputDataEntry {
    public int timestamp;
    public Vector2 playerInputVector;

    public InputDataEntry(int timestamp, Vector2 playerInputVector) {
      this.timestamp = timestamp;
      this.playerInputVector = playerInputVector;
    }
  }

}
