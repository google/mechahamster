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

namespace Hamster.MapObjects {

  // When stepped on, these accelerate the ball in a direction.
  public class WallTile : MapObject {
    // Prefab used for single wall pieces.
    public GameObject SinglePrefab;
    // Prefab used for walls that have two near neighbors.
    public GameObject CornerPrefab;
    // Prefab used when a wall is missing a single neighbor.
    public GameObject TTurnPrefab;
    // Prefab used when a wall has all four neighbors.
    public GameObject JunctionPrefab;

    // How much to adjust where the wall is spawned, in relation to this object.
    public Vector3 AdjustSpawn = new Vector3(0.0f, 0.5f, 0.0f);

    // Tracks the spawned Wall object.
    public GameObject SpawnedWall { get; private set; }

    private void Start() {
      UpdateWall();
      UpdateNeighbors();
    }

    private void OnDestroy() {
      // When being removed, tell the neighbors so they can update their walls.
      UpdateNeighbors();
    }

    // Updates the spawned wall being used, with regards to neighboring wall tiles.
    public void UpdateWall() {
      WallTile left = WallTileAtDelta(-1, 0);
      WallTile right = WallTileAtDelta(1, 0);
      WallTile up = WallTileAtDelta(0, 1);
      WallTile down = WallTileAtDelta(0, -1);
      int count =
        (left != null ? 1 : 0) + (right != null ? 1 : 0) +
        (up != null ? 1 : 0) + (down != null ? 1 : 0);

      float yRotation = 0.0f;
      switch (count) {
        case 0:
          // If by yourself, use a single tile.
          SpawnWall(SinglePrefab, 180.0f);
          break;
        case 1:
          // If only one neighbor, rotate so that the edge touches that neighbor.
          if (up != null) {
            yRotation = 180.0f;
          } else if (left != null) {
            yRotation = 90.0f;
          } else if (right != null) {
            yRotation = 270.0f;
          }
          SpawnWall(SinglePrefab, yRotation);
          break;
        case 2:
          GameObject prefab;
          if (up == down || left == right) {
            // If two neighbors, that are directly across from eachother,
            // use a straight piece that connects the two.
            prefab = SinglePrefab;
            if (left != null) {
              yRotation = 90.0f;
            }
          } else {
            // Otherwise, use a corner piece, oriented to touch the two neighbors.
            if (up != null) {
              if (left != null) {
                yRotation = 90.0f;
              } else {
                yRotation = 180.0f;
              }
            } else if (right != null) {
              yRotation = 270.0f;
            }
            prefab = CornerPrefab;
          }
          SpawnWall(prefab, yRotation);
          break;
        case 3:
          // With three neighbors, orient so that the flat edge is towards the missing one.
          if (right == null) {
            yRotation = 90.0f;
          } else if (down == null) {
            yRotation = 180.0f;
          } else if (left == null) {
            yRotation = 270.0f;
          }
          SpawnWall(TTurnPrefab, yRotation);
          break;
        case 4:
          // With four neighbors, connect to all four.
          SpawnWall(JunctionPrefab, yRotation);
          break;
      }
    }

    // Helper function to spawn the prefab with the given rotation about the Y axis,
    // as the SpawnedWall.
    private void SpawnWall(GameObject prefab, float rotation) {
      if (SpawnedWall != null) {
        Destroy(SpawnedWall);
        SpawnedWall = null;
      }
      // We attach the wall as a child, so that when the map is unloaded the spawned
      // wall is removed as well.
      SpawnedWall = Instantiate(prefab, transform.position + AdjustSpawn,
        Quaternion.Euler(0.0f, rotation, 0.0f), transform);
    }

    // Helper function to trigger all neighboring walls to update.
    private void UpdateNeighbors() {
      UpdateNeighborAt(-1, 0);
      UpdateNeighborAt(1, 0);
      UpdateNeighborAt(0, -1);
      UpdateNeighborAt(0, 1);
    }

    // Update the neighbor in a given relative direction.
    private void UpdateNeighborAt(int deltaX, int deltaZ) {
      WallTile wallTile = WallTileAtDelta(deltaX, deltaZ);
      if (wallTile != null) {
        wallTile.UpdateWall();
      }
    }

    // Get the WallTile of a neighbor in a given relative direction.
    private WallTile WallTileAtDelta(int deltaX, int deltaZ) {
      int x = (int)transform.position.x;
      int z = (int)transform.position.z;

      GameObject go = CommonData.gameWorld.ElementAtPosition(x + deltaX, z + deltaZ);
      if (go != null) {
        return go.GetComponentInChildren<WallTile>();
      }
      return null;
    }
  }
}
