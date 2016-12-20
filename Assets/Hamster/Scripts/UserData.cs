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

    public UserData() { }

    // Associates an existing map with a user profile.  (Usually used
    // when a new map is created.)
    public void addMap(LevelMap newMap) {
      // Remove the map if we're saving over something that exists:
      List<MapListEntry> toDelete = maps.FindAll(value => { return value.mapId == newMap.mapId; });
      foreach(MapListEntry entry in toDelete) {
        maps.Remove(entry);
      }

      foreach (MapListEntry mapEntry in maps) {
        if (mapEntry.mapId == newMap.mapId)
          throw new System.Exception("map already exists");
      }

      // TODO(ccornell) write this using transactions.
      maps.Add(new MapListEntry(newMap.name, newMap.mapId));
    }

    public void removeMap(string targetMapId) {
      MapListEntry target = null;

      foreach (MapListEntry mapEntry in maps) {
        if (mapEntry.mapId == targetMapId) {
          target = mapEntry;
          break;
        }
      }

      if (target != null) {
        maps.Remove(target);
      } else {
        throw new System.Exception("Could not find map to remove: " + targetMapId);
      }
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
    public string name = "<<MAP NAME>>";
  }
}