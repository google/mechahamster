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

      // TODO(ccornell) write this using transactions.
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