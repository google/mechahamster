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
using Firebase.Storage;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Hamster {

  [System.Serializable]
  public class TimeData {
    // The name associated with the time.
    public string name;
    // The time taken to complete a level, in seconds.
    public long time;

    public TimeData() {}

    public TimeData(string name, long time) {
      this.name = name;
      this.time = time;
    }

    // Uploads the time data to the database, and returns the current top time list.
    public Task<List<TimeData>> UploadTime(LevelMap map, ReplayData replay) {
      DatabaseReference reference =
        FirebaseDatabase.DefaultInstance.GetReference(GetDBTimePath(map));
      return reference.RunTransaction(mutableData => UploadScoreTransaction(mutableData, this))
          .ContinueWith(task => UploadReplayData(GetTimeList(task.Result), map, replay, this))
          .Unwrap();
    }

    // Gets the path, given the level's database path and map id.
    private static string GetPath(LevelMap map) {
      if (!string.IsNullOrEmpty(map.DatabasePath)) {
        return map.DatabasePath;
      } else {
        return "OfflineMaps/" + map.mapId;
      }
    }

    // Gets the path for the times on the database, given the level's database path
    // and map id.
    private static string GetDBTimePath(LevelMap map) {
      return GetPath(map) + "/Times";
    }

    // Gets the path for replay data with highest score on the storage, given the
    // level's database path and map id.
    private static string GetBestReplayStoragePath(LevelMap map) {
      return "/Replay/" + GetPath(map) + "/Highest_score.bytes";
    }

    // Returns the top times, given the level's database path and map id.
    public static Task<List<TimeData>> GetTopTimes(LevelMap map) {
      DatabaseReference reference =
        FirebaseDatabase.DefaultInstance.GetReference(GetDBTimePath(map));
      return reference.GetValueAsync().ContinueWith(task => {
        return GetTimeList(task.Result);
      });
    }

    // Uploads the time data to the top time list for a level, as a Firebase Database transaction.
    private static TransactionResult UploadScoreTransaction(
      MutableData mutableData, TimeData timeData) {
      List<object> leaders = mutableData.Value as List<object>;
      if (leaders == null) {
        leaders = new List<object>();
      }

      // Only save a certain number of the best times.
      if (mutableData.ChildrenCount >= 5) {
        long maxTime = long.MinValue;
        object maxVal = null;
        foreach (object child in leaders) {
          if (!(child is Dictionary<string, object>))
            continue;
          string childName = (string)((Dictionary<string, object>)child)["name"];
          long childTime = (long)((Dictionary<string, object>)child)["time"];
          if (childTime > maxTime) {
            maxTime = childTime;
            maxVal = child;
          }
        }

        if (maxTime < timeData.time) {
          // Don't make any changes to the mutable data, but return it so we can use
          // the snapshot.
          return TransactionResult.Success(mutableData);
        }
        leaders.Remove(maxVal);
      }

      Dictionary<string, object> newTimeEntry = new Dictionary<string, object>();
      newTimeEntry["name"] = timeData.name;
      newTimeEntry["time"] = timeData.time;
      leaders.Add(newTimeEntry);

      mutableData.Value = leaders;
      return TransactionResult.Success(mutableData);
    }

    // Upload the replay data to storage if necessary.  And return time list from previous task
    // Use TaskCompletionSource to handle the following scenarios
    // 1. If there is no need to upload replay data, complete the task immediately
    //    (Ex. replay is disabled or the score is not the highest)
    // 2. If the replay data is available and is best record, complete the task once uploading
    //    is done (success or fail)
    // Either way, the returned task should contain the list of top time records from previous task
    private static Task<List<TimeData>> UploadReplayData(
      List<TimeData> timeDataResult, LevelMap map, ReplayData replay, TimeData timeData) {
      TaskCompletionSource<List<TimeData>> tComplete = new TaskCompletionSource<List<TimeData>>();

      if (replay != null && IsHighestScore(timeData, timeDataResult)) {
        string fileLocation = GetBestReplayStoragePath(map);
        StorageReference storageRef =
          FirebaseStorage.DefaultInstance.GetReferenceFromUrl(CommonData.storageBucketUrl +
            fileLocation);

        // Serializing replay data to byte array
        System.IO.MemoryStream stream = new System.IO.MemoryStream();
        replay.Serialize(stream);
        stream.Position = 0;
        byte[] serializedData = stream.ToArray();

        // Add database path and time to file metadata for future usage
        MetadataChange newMetadata = new MetadataChange {
          CustomMetadata = new Dictionary<string, string> {
            {"DatabasePath", GetDBTimePath(map)},
            {"Time", timeData.time.ToString()}
          }
        };

        storageRef.PutBytesAsync(serializedData, newMetadata).ContinueWith(uploadResult => {
          tComplete.SetResult(timeDataResult);

          if (uploadResult.IsFaulted) {
            if (uploadResult.Exception != null) {
              tComplete.SetException(uploadResult.Exception);
            }
          }
        });
      } else {
        tComplete.SetResult(timeDataResult);
      }
      return tComplete.Task;
    }

    // Check if timeData has the highest score to every data in dataList.  Return true for a tie
    private static bool IsHighestScore(TimeData timeData, List<TimeData> dataList) {
      foreach (TimeData data in dataList) {
        if(data.time < timeData.time) {
          return false;
        }
      }
      return true;
    }

    // Gets the current list of top times from a Database snapshot.
    private static List<TimeData> GetTimeList(DataSnapshot snapshot) {
      List<TimeData> timeList = new List<TimeData>();
      List<object> databaseList = snapshot.Value as List<object>;
      if (databaseList == null) {
        return timeList;
      }

      foreach (object child in databaseList) {
        var childDict = child as Dictionary<string, object>;
        if (childDict == null)
          continue;
        string childName = (string)childDict["name"];
        long childTime = (long)childDict["time"];

        timeList.Add(new TimeData(childName, childTime));
      }

      return timeList;
    }

    // Utility function to download the metadata of the best replay record for the given map
    public static Task<StorageMetadata> GetBestRecordMetadataAsync(LevelMap map) {
      string fileLocation = GetBestReplayStoragePath(map);
      StorageReference storageRef =
        FirebaseStorage.DefaultInstance.GetReferenceFromUrl(CommonData.storageBucketUrl +
          fileLocation);

      return storageRef.GetMetadataAsync();
    }

    // Utility function to download best replay record for the given map and deserialize into
    // ReplayData struct
    public static Task<ReplayData> DownloadBestRecordAsync(LevelMap map) {
      TaskCompletionSource<ReplayData> tComplete = new TaskCompletionSource<ReplayData>();

      string fileLocation = GetBestReplayStoragePath(map);
      StorageReference storageRef =
        FirebaseStorage.DefaultInstance.GetReferenceFromUrl(CommonData.storageBucketUrl +
          fileLocation);

      storageRef.GetStreamAsync( stream => {
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
}
