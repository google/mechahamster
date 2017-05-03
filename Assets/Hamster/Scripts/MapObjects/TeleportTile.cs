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
using System.Collections.Generic;

namespace Hamster.MapObjects {

  // When activated, these teleport the player to the next tile with the same key.
  public class TeleportTile : MapObject {
    // A list of all the Teleport tiles, to teleport between.
    public static Dictionary<int, List<TeleportTile>> TileLists { get; private set; }

    // The amount of time after the tile is used that it can't be used again, in seconds.
    public static float TotalCooldownTime = 1.0f;

    // The cooldown timer for the tile, in seconds.
    public float Cooldown { get; private set; }

    // The GameObject to disable when the tile is on cooldown.
    public GameObject HideOnCooldown;

    // The key that the teleport tile is registered under, tiles teleport to other
    // tiles with the same key. Used on Awake, but made public to be editable in Editor.
    public int Key = 0;
    // The key the tile is registered with. Since Key can potentially change, we save
    // which key we are saved under in the lists.
    private int usedKey;

    void Awake() {
      // Because all Awake's happen in the main Unity thread, no lock is needed.
      if (TileLists == null) {
        TileLists = new Dictionary<int, List<TeleportTile>>();
      }
      usedKey = Key;
      List<TeleportTile> tileList;
      if (!TileLists.TryGetValue(usedKey, out tileList)) {
        tileList = new List<TeleportTile>();
        TileLists[usedKey] = tileList;
      }
      TileLists[usedKey].Add(this);
      Cooldown = 0.0f;
    }

    void OnDestroy() {
      if (TileLists != null && TileLists[usedKey] != null) {
        TileLists[usedKey].Remove(this);
      }
    }

    public override void Reset() {
      FinishCooldown();
    }

    void FixedUpdate() {
      if (Cooldown > 0.0f) {
        Cooldown -= Time.fixedDeltaTime;
        if (Cooldown <= 0.0f) {
          FinishCooldown();
        }
      }
    }

    public void StartCooldown() {
      Cooldown = TotalCooldownTime;
      if (HideOnCooldown != null) {
        HideOnCooldown.SetActive(false);
      }
    }

    private void FinishCooldown() {
      Cooldown = 0.0f;
      if (HideOnCooldown != null) {
        HideOnCooldown.SetActive(true);
      }
    }

    void OnTriggerEnter(Collider collider) {
      PlayerController pc = collider.GetComponent<PlayerController>();
      if (pc != null && Cooldown <= 0.0f) {
        List<TeleportTile> tileList = TileLists[usedKey];
        // Teleport the PlayerController to the next TeleportTile in the list.
        int myIndex = tileList.IndexOf(this);
        if (myIndex >= 0) {
          // Teleport to the next tile in the list.
          int nextIndex = (myIndex + 1) % tileList.Count;
          if (myIndex != nextIndex) {
            pc.transform.position = tileList[nextIndex].transform.position;
            // Set both this and the receiving tile on cooldown.
            StartCooldown();
            tileList[nextIndex].StartCooldown();
          }
        }
      }
    }
  }
}
