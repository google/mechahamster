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

  // List of prefabs used by the game.
  // A game object uses this component, and can then be used
  // to access and instantiate the prefabs.  (Also, this serves to
  // force unity to include these prefabs - otherwise Unity automatically
  // strips unused resources from release builds:
  // https://docs.unity3d.com/Manual/iphone-playerSizeOptimization.html)
  public class PrefabList : MonoBehaviour {
    public UnityEngine.GUISkin guiSkin;
    // List of all the prefabs, and their names.  Note that this is mostly
    // just used as a way to edit the list in the inspector - changing this
    // at runtime won't do anything, because all of the data has already been
    // copied out into lookup and prefabnames.
    public PrefabEntry[] prefabs;

    // Lookup dictionary, for quickly finding the prefab, given a name.
    [HideInInspector]
    public Dictionary<string, PrefabEntry> lookup;

    // Array of prefabs names.  Useful because some things (GUI) need
    // them in a plain array of strings.
    [HideInInspector]
    public string[] prefabNames;

    // On startup, populate the lookup dictionary with the entries
    // in the prefab array.  (The prefab array is populated through the
    // unity editor.  We can't just populate the lookup dictionary directly
    // because the unity editor doesn't know how to provide an interface
    // for dictionaries.)
    void Start() {
      lookup = new Dictionary<string, PrefabEntry>();
      prefabNames = new string[prefabs.Length];
      int index = 0;
      foreach (PrefabEntry entry in prefabs) {
        lookup[entry.name] = entry;
        prefabNames[index++] = entry.name;
      }
    }

    [System.Serializable]
    public struct PrefabEntry {
      public PrefabEntry(string name, GameObject prefab, bool limitOnePerMap) {
        this.name = name;
        this.prefab = prefab;
        this.limitOnePerMap = limitOnePerMap;
      }
      public string name;
      public GameObject prefab;
      public bool limitOnePerMap;
    }
  }
}
