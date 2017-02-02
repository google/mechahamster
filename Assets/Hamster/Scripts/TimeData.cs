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
    public Task<List<TimeData>> UploadTime(LevelMap map) {
      DatabaseReference reference =
        FirebaseDatabase.DefaultInstance.GetReference(GetPath(map));
      return reference.RunTransaction(
        mutableData => UploadScoreTransaction(mutableData, this))
          .ContinueWith(task => {
            return GetTimeList(task.Result);
          });
    }

    // Gets the path for the times on the database, given the level's database path
    // and map id.
    private static string GetPath(LevelMap map) {
        if (!string.IsNullOrEmpty(map.DatabasePath)) {
          return map.DatabasePath + "/Times";
        } else {
          return "OfflineMaps/" + map.mapId + "/Times";
        }
      }

    // Returns the top times, given the level's database path and map id.
    public static Task<List<TimeData>> GetTopTimes(LevelMap map) {
      DatabaseReference reference =
        FirebaseDatabase.DefaultInstance.GetReference(GetPath(map));
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
  }

}
