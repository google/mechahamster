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
    // Tracks the map objects that there is a max count limit of.
    Dictionary<string, List<string>> limitedMapObjects =
        new Dictionary<string, List<string>>();

    // Tracks the extra objects, like particles, that need to be destroyed on reset.
    public List<Utilities.DestroyOnReset> destroyOnReset =
        new List<Hamster.Utilities.DestroyOnReset>();

    // Gameobject used to hold all the meshes, after we stitch them together.
    GameObject mergedMeshHolder;

    // Check to see if the meshes have been merged yet.
    public bool AreMeshesMerged {
      get { return mergedMeshHolder != null; }
    }

    // Gets the hash code of the current map.
    public int mapHash {
      get {
        return JsonUtility.ToJson(worldMap).GetHashCode();
      }
    }

    // When spawning in elements, adjust their positions by this amount.
    Vector3 elementAdjust = new Vector3(0.0f, -0.5f, 0.0f);

    public float GameStartTime { get; private set; }
    // Returns the amount of time the current level has been played, in seconds.
    public float ElapsedGameTime {
      get {
        return Time.time - GameStartTime;
      }
    }

    // Returns the amount of time the current level has been played, in milliseconds.
    public long ElapsedGameTimeMs {
      get {
        return (long)(ElapsedGameTime * 1000);
      }
    }

    // Reference to the replay data of previously played level.
    public ReplayData PreviousReplayData;

    // Does the currently loaded map have pending edits not saved to the database.
    public bool HasPendingEdits { get; private set; }

    private void Start() {
      GameStartTime = Time.time;
      PreviousReplayData = null;
      HasPendingEdits = false;
    }

    // Iterates through a map and spawns all the objects in it.
    public void SpawnWorld(LevelMap map) {
      foreach (MapElement element in map.elements.Values) {
        PlaceTile(element);
      }
      worldMap.SetProperties(map.name, map.mapId, map.ownerId, map.DatabasePath);
      HasPendingEdits = false;

      // Set the camera to the start position:
      MapObjects.StartPosition startPos = FindObjectOfType<MapObjects.StartPosition>();
      if (startPos) {
        CommonData.mainCamera.MoveCameraTo(startPos.gameObject.transform.position);
      }
    }

    // Removes the gameobjects that represent the map onscreen.
    // Leaves the data representation (worldMap) intact.
    private void ClearMapGameObjects() {
      foreach (GameObject obj in sceneObjects.Values) {
        Destroy(obj);
      }
      if (mergedMeshHolder != null) {
        Destroy(mergedMeshHolder);
        mergedMeshHolder = null;
      }
      sceneObjects.Clear();
    }

    // Removes the game world, and all gameobjects associated with it.
    public void DisposeWorld() {
      ClearMapGameObjects();
      worldMap.elements.Clear();
      limitedMapObjects.Clear();

      worldMap.ResetProperties();
      worldMap.DatabasePath = null;
      HasPendingEdits = false;
    }

    // Internal utility function for removing an item from the map, based on its
    // dictionary key.  (The dictionary key is the string returned from
    // MapElement::GetKey())
    void RemoveObject(string key) {
      List<string> limitedList;
      if (limitedMapObjects.TryGetValue(worldMap.elements[key].type, out limitedList)) {
        limitedList.Remove(key);
      }
      worldMap.elements.Remove(key);
      Destroy(sceneObjects[key]);
      sceneObjects.Remove(key);
    }

    // Spawns a single object in the world, based on a map element.
    public GameObject PlaceTile(MapElement element) {
      if (string.IsNullOrEmpty(element.type)) {
        return null;
      }

      string key = element.GetStringKey();

      PrefabList.PrefabEntry prefabEntry = CommonData.prefabs.lookup[element.type];

      // If we're placing a unique object, or we're placing an object over an existing object,
      // then remove the old one that is being replaced.
      if (worldMap.elements.ContainsKey(key)) {
        RemoveObject(key);
      }

      // Add the new object.  (Or add nothing, if we're "adding" an empty blocks)

      GameObject obj = SpawnElement(element);
      if (obj != null) {
        worldMap.elements.Add(key, element);
        sceneObjects.Add(element.GetStringKey(), obj);

        if (prefabEntry.maxCount > 0) {
          List<string> limitedList = null;
          if (!limitedMapObjects.TryGetValue(element.type, out limitedList)) {
            limitedList = new List<string>();
            limitedMapObjects[element.type] = limitedList;
          }
          limitedList.Add(key);
          while (limitedList.Count > prefabEntry.maxCount) {
            RemoveObject(limitedList[0]);
          }
        }
      }
      HasPendingEdits = true;
      return obj;
    }

    // Spawns an element in the world as a GameObject, and performs
    // the necessary bookkeeping to track it.
    GameObject SpawnElement(MapElement element) {
      GameObject obj = null;
      PrefabList.PrefabEntry elementDef;
      if (CommonData.prefabs.lookup.TryGetValue(element.type, out elementDef)) {
        if (elementDef.prefab != null) {
          Quaternion orientation = Quaternion.Euler(
              new Vector3(0.0f, element.orientation * 90.0f, 0.0f));
          obj = (GameObject)Instantiate(elementDef.prefab, element.position + elementAdjust,
                                        orientation);
        }
      } else {
        throw new Exception(
            "SpawnElement: type did not match any registered prefabs: [" + element.type + "]");
      }
      return obj;
    }

    // Returns the GameObject that matches with the non-unique MapElement at the given position.
    public GameObject ElementAtPosition(int x, int z) {
      string key = MapElement.GetKeyForPosition(x, z);
      GameObject gameObject;
      if (sceneObjects.TryGetValue(key, out gameObject)) {
        return gameObject;
      } else {
        return null;
      }
    }

    // Called when the current world map is saved to the database.
    public void OnSave() {
      HasPendingEdits = false;
    }

    // Reset the Map back to its original state. This includes the game time since it started,
    // and the MapObjects
    public void ResetMap() {
      GameStartTime = Time.time;
      PreviousReplayData = null;
      foreach (GameObject gameObject in sceneObjects.Values) {
        MapObjects.MapObject mapObject = gameObject.GetComponentInChildren<MapObjects.MapObject>();
        if (mapObject != null) {
          mapObject.Reset();
        }
      }
      foreach (Utilities.DestroyOnReset toDestroy in destroyOnReset) {
        toDestroy.RegisteredInWorld = false;
        Destroy(toDestroy.gameObject);
      }
      destroyOnReset.Clear();
    }

    // Called when a Switch is triggered, forwards that along to switchable map objects.
    public void OnSwitchTriggered() {
      foreach (GameObject gameObject in sceneObjects.Values) {
        MapObjects.ISwitchable switchable =
          gameObject.GetComponentInChildren<MapObjects.ISwitchable>();
        if (switchable != null) {
          switchable.OnSwitchTriggered();
        }
      }
    }

    // Respawns the world.  Useful for splitting things back up into individual
    // meshes, once they've been merged.  (Usually when we are in the editor,
    // transitioning from testing the level, back into edit mode.)
    public void RespawnWorld() {
      ClearMapGameObjects();
      foreach (MapElement element in worldMap.elements.Values) {
        GameObject obj = SpawnElement(element);
        if (obj != null) {
          sceneObjects.Add(element.GetStringKey(), obj);
        }
      }
    }

    // Merge meshes together for faster drawing.
    public void MergeMeshes() {
      if (AreMeshesMerged) {
        return;
      }

      // First make a list of all top-level child meshes that don't
      // have animators attached.
      Dictionary<Material, List<MeshRenderer>> staticMeshes =
          new Dictionary<Material, List<MeshRenderer>>();

      mergedMeshHolder = new GameObject();
      mergedMeshHolder.name = "MergedMeshHolder";

      foreach(GameObject obj in sceneObjects.Values) {
        foreach (Transform child in obj.transform) {
          bool hasAnimator = child.GetComponentsInChildren<Animator>().Length > 0;
          if (!hasAnimator) {
            foreach (MeshRenderer meshRenderer in child.GetComponentsInChildren<MeshRenderer>()) {
              if (meshRenderer.enabled) {
                int materialHash = meshRenderer.sharedMaterial.GetHashCode();
                if (!staticMeshes.ContainsKey(meshRenderer.sharedMaterial)) {
                  staticMeshes.Add(meshRenderer.sharedMaterial, new List<MeshRenderer>());
                }
                staticMeshes[meshRenderer.sharedMaterial].Add(meshRenderer);
                Destroy(meshRenderer);
              }
            }
          }
        }
      }
      foreach (Material mat in staticMeshes.Keys) {
        List<MeshRenderer> meshList = staticMeshes[mat];
        CombineInstance[] combineArray = new CombineInstance[meshList.Count];
        int i = 0;
        foreach (MeshRenderer meshRenderer in meshList) {
          MeshFilter meshFilter = meshRenderer.gameObject.GetComponent<MeshFilter>();
          if (meshFilter != null) {
            combineArray[i].mesh = meshFilter.mesh;
            combineArray[i].transform = meshFilter.transform.localToWorldMatrix;
            i++;
          }
        }
        GameObject meshHolder = new GameObject();
        meshHolder.name = "MeshHolder";

        meshHolder.transform.SetParent(mergedMeshHolder.transform, false);
        MeshRenderer newRenderer = meshHolder.AddComponent<MeshRenderer>();
        MeshFilter newFilter = meshHolder.AddComponent<MeshFilter>();
        newFilter.mesh = new Mesh();
        newFilter.mesh.CombineMeshes(combineArray, true);
        newRenderer.material = mat;
      }
    }
  }
}
