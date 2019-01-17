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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
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

    // Variable for controlling how many frames until the next position data is recorded.
    uint positionRecordInterval = 3;

    // Record maximum 6000 records of position and/or input data
    // (About 5 minutes if record every frame in 20fps)
    const int maxRecords = 6000;

    public GameplayRecorder(string mapId, uint sampleInternal) {
      positionRecordInterval = sampleInternal;

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
      // Only record before maximum frames of position data has been recorded.
      if (positionData.Count < maxRecords && inputData.Count < maxRecords) {
        InputControllers.BasePlayerController playerController = playerBall.inputController;
        if (timestamp % positionRecordInterval == 0) {
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
    }

    // Serializes and outputs the current recording to a file.
    public void OutputToFile(string fileName) {
      ReplayData replayData = CreateReplayData();
      replayData.WriteToFile(fileName);
    }

    // Create ReplayData object from recorded data.
    public ReplayData CreateReplayData() {
      return new ReplayData(mapId,
        CommonData.gameWorld.mapHash, positionData.ToArray(), inputData.ToArray());
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

    // Private constructor for factory functions such as CreateFromStream
    private ReplayData() { }

    // Serialize the current recording to a binary array.
    public void Serialize(Stream stream) {
      BinaryFormatter formatter = new BinaryFormatter();

      try {
        formatter.Serialize(stream, this);
      } catch (System.Exception e) {
        Debug.LogError("Serialization Exception caught.\n" + e.ToString());
      }
    }

    // Write the data to specified file
    public void WriteToFile(string filePath) {
      using (FileStream stream = File.Create(filePath)) {
        Serialize(stream);
      }
    }

    // Deserialize the binary raw data to ReplayData
    public void Deserialize(Stream stream) {
      BinaryFormatter formatter = new BinaryFormatter();
      ReplayData deserializedData = formatter.Deserialize(stream) as ReplayData;

      mapId = deserializedData.mapId;
      mapHash = deserializedData.mapHash;
      positionData = deserializedData.positionData;
      inputData = deserializedData.inputData;
    }

    // Read the data from specified file
    public void ReadFromFile(string filePath) {
      using (FileStream stream = File.OpenRead(filePath)) {
        Deserialize(stream);
      }
    }

    // Static utility function to create ReplayData from stream such as
    // FileStream or MemoryStream
    public static ReplayData CreateFromStream(Stream stream) {
      ReplayData replay = new ReplayData();
      replay.Deserialize(stream);
      return replay;
    }

    // Utility function to download replay record from a given path and deserialize into
    // ReplayData struct
    public static Task<ReplayData> DownloadReplayRecordAsync(string replayPath) {
      TaskCompletionSource<ReplayData> tComplete = new TaskCompletionSource<ReplayData>();

      Firebase.Storage.StorageReference storageRef =
        Firebase.Storage.FirebaseStorage.DefaultInstance.GetReference(replayPath);

      storageRef.GetStreamAsync(stream => {
        tComplete.SetResult(ReplayData.CreateFromStream(stream));
      }).ContinueWith(task => {
        if (task.IsFaulted) {
          tComplete.SetException(task.Exception);
        }
        if (task.IsCanceled) {
          tComplete.SetCanceled();
        }
      });

      return tComplete.Task;
    }
  }

  [System.Serializable]
  public class PositionDataEntry {
    public int timestamp;
    public SerializableVector3 position;
    public SerializableQuaternion rotation;
    public SerializableVector3 velocity;
    public SerializableVector3 angularVelocity;

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
    public SerializableVector2 playerInputVector;

    public InputDataEntry(int timestamp, Vector2 playerInputVector) {
      this.timestamp = timestamp;
      this.playerInputVector = playerInputVector;
    }
  }

  // BinaryFormatter cannot serialize Unity native Vector3, Vector2 and Quaternion,
  // because they are not marked as serializable.  The following structs are work-arounds
  // utilizing implicit conversion operator.
  [System.Serializable]
  public struct SerializableVector3 {
    public float x;
    public float y;
    public float z;

    public SerializableVector3(Vector3 v) {
      x = v.x;
      y = v.y;
      z = v.z;
    }

    public static implicit operator SerializableVector3(Vector3 v) {
      return new SerializableVector3(v);
    }

    public static implicit operator Vector3(SerializableVector3 sv) {
      return new Vector3(sv.x, sv.y, sv.z);
    }
  }

  [System.Serializable]
  public struct SerializableVector2 {
    public float x;
    public float y;

    public SerializableVector2(Vector2 v) {
      x = v.x;
      y = v.y;
    }

    public static implicit operator SerializableVector2(Vector2 v) {
      return new SerializableVector2(v);
    }

    public static implicit operator Vector2(SerializableVector2 sv) {
      return new Vector2(sv.x, sv.y);
    }
  }

  [System.Serializable]
  public struct SerializableQuaternion {
    public float x;
    public float y;
    public float z;
    public float w;

    public SerializableQuaternion(Quaternion q) {
      x = q.x;
      y = q.y;
      z = q.z;
      w = q.w;
    }

    public static implicit operator SerializableQuaternion(Quaternion q) {
      return new SerializableQuaternion(q);
    }

    public static implicit operator Quaternion(SerializableQuaternion sq) {
      return new Quaternion(sq.x, sq.y, sq.z, sq.w);
    }
  }
}
