using UnityEngine;
using System;
using System.Collections.Generic;

namespace Hamster {
  // Monobehaviour for the actual unity gameobject that represents the map.
  // Largely concerned with spawning the scene representation of a level
  // and tearing it down when done.
  class GameWorld : MonoBehaviour {
    // Dictioary of scene object representations of the map tiles and elements.
    // The key is the same key as they have in the database table.  (A unique
    // string based on their position and type.)
    Dictionary<string, GameObject> sceneObjects = new Dictionary<string, GameObject>();
    public LevelMap worldMap = new LevelMap();

    // Iterates through a map and spawns all the objects in it.
    public void SpawnWorld(LevelMap map) {
      foreach (MapElement element in map.elements.Values) {
        GameObject obj = PlaceTile(element);
        obj.transform.localScale = element.scale;
      }
      worldMap.ownerId = map.ownerId;
      worldMap.name = map.name;
      worldMap.mapId = map.mapId;
    }

    public void DisposeWorld() {
      worldMap.elements.Clear();
      foreach (GameObject obj in sceneObjects.Values) {
        Destroy(obj);
      }
      sceneObjects.Clear();
    }

    // Internal utility function for removing an item from the map, based on its
    // dictionary key.  (The dictionary key is the string returned from
    // MapElement::GetKey())
    void RemoveObject(string key) {
      worldMap.elements.Remove(key);
      Destroy(sceneObjects[key]);
      sceneObjects.Remove(key);
    }

    // Spawns a single object in the world, based on a map element.
    public GameObject PlaceTile(MapElement element) {
      string key = element.GetStringKey();

      // If we're placing a unique object, or we're placing an object over an existing object,
      // then remove the old one that is being replaced.
      if (worldMap.elements.ContainsKey(key)) {
        RemoveObject(key);
      }

      // Add the new object.  (Or add nothing, if we're "adding" an empty blocks)
      GameObject obj = null;
      if (!worldMap.elements.ContainsKey(key) && element.type != MapElement.MapElementType.Empty) {
        obj = SpawnElement(element);
        worldMap.elements.Add(key, element);
      }

      if (obj != null) {
        sceneObjects.Add(element.GetStringKey(), obj);
      }
      return obj;
    }

    GameObject SpawnElement(MapElement element) {
      GameObject obj = null;
      switch (element.type) {
        case MapElement.MapElementType.Empty:
          break;
        case MapElement.MapElementType.Wall:
          obj = (GameObject)(Instantiate(CommonData.prefabs.wall,
              element.position, element.rotation));
          break;
        case MapElement.MapElementType.Floor:
          obj = (GameObject)(Instantiate(CommonData.prefabs.floor,
              element.position, element.rotation));
          break;
        case MapElement.MapElementType.StartPosition:
          obj = (GameObject)(Instantiate(CommonData.prefabs.startPos,
              element.position, element.rotation));
          break;
        case MapElement.MapElementType.JumpPad:
          obj = (GameObject)(Instantiate(CommonData.prefabs.jumpTile,
              element.position, element.rotation));
          break;
        case MapElement.MapElementType.Goal:
          obj = (GameObject)(Instantiate(CommonData.prefabs.goal,
              element.position, element.rotation));
          break;
        default:
          Debug.LogError("Spawning objects - encountered unknown element type:" + element.type);
          break;
      }
      return obj;
    }

  }
}
