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

namespace Hamster {

  [System.Serializable]
  public class UserData {
    // Database ID
    public string id = "<<ID>>";
    // Plaintext name
    public string name = "<<USER NAME>>";
    // List of all maps owned by this player.
    public List<MapListEntry> maps = new List<MapListEntry>();
    public List<MapListEntry> bonusMaps = new List<MapListEntry>();
    public List<MapListEntry> sharedMaps = new List<MapListEntry>();

    public UserData() { }

    // Associates an existing map with a user profile.  (Usually used
    // when a new map is created.)
    private void AddMapHelper(string mapName, string mapId, List<MapListEntry> mapList) {
      // Remove the map if we're saving over something that exists:
      List<MapListEntry> toDelete = mapList.FindAll(value => {
        return value.mapId == mapId;
      });

      foreach (MapListEntry entry in toDelete) {
        mapList.Remove(entry);
      }

      foreach (MapListEntry mapEntry in mapList) {
        if (mapEntry.mapId == mapId)
          throw new System.Exception("map already exists");
      }

      mapList.Add(new MapListEntry(mapName, mapId));
    }

    private void RemoveMapHelper(string targetMapId, List<MapListEntry> mapList) {
      MapListEntry target = null;

      foreach (MapListEntry mapEntry in mapList) {
        if (mapEntry.mapId == targetMapId) {
          target = mapEntry;
          break;
        }
      }

      if (target != null) {
        mapList.Remove(target);
      } else {
        throw new System.Exception("Could not find map to remove: " + targetMapId);
      }
    }

    public void AddMap(string mapName, string mapId) {
      AddMapHelper(mapName, mapId, maps);
    }

    public void RemoveMap(string targetMapId) {
      RemoveMapHelper(targetMapId, maps);
    }

    public void AddBonusMap(string mapName, string mapId) {
      AddMapHelper(mapName, mapId, bonusMaps);
    }

    public void RemoveBonusMap(string targetMapId) {
      RemoveMapHelper(targetMapId, bonusMaps);
    }

    public void AddSharedMap(string mapName, string mapId) {
      AddMapHelper(mapName, mapId, sharedMaps);
    }

    public void RemoveSharedMap(string targetMapId) {
      RemoveMapHelper(targetMapId, sharedMaps);
    }

  }

  [System.Serializable]
  public class MapListEntry {

    //  Constructor
    public MapListEntry(string name, string mapId) {
      this.name = name;
      this.mapId = mapId;
    }

    // Unique database identifier.
    public string mapId;
    // Plaintext string name.
    public string name = StringConstants.DefaultMapName;
  }
}