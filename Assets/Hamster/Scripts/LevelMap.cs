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
  // The LevelMap class representation of a single level in the game.
  // It is fully serializable via JsonUtility, and contains a list of
  // all objects (and their positions) in the level.
  [System.Serializable]
  public class LevelMap {
    public string name = StringConstants.DefaultMapName;
    public string mapId = StringConstants.DefaultMapId;
    public string ownerId = "<<ownerId>>";
    public bool isShared = false;
    public StringMapElementDict elements = new StringMapElementDict();

    // The database path that the map is saved under.
    // Null/empty indicates it is an offline available map.
    public string DatabasePath { get; set; }

    public void ResetProperties() {
      name = StringConstants.DefaultMapName;
      mapId = StringConstants.DefaultMapId;
      ownerId = "<<ownerId>>";
      DatabasePath = null;
    }

    public void SetProperties(string name, string mapId, string ownerId,
      string databasePath) {
      this.name = name;
      this.mapId = mapId;
      this.ownerId = ownerId;
      DatabasePath = databasePath;
    }
  }

  // Ok, so this is a bit of a hack.
  // Unity's jsonutility parser hates a lot of things.
  // One thing it hates is Dictionaries.  It can't serialize them at all.
  // Hence, the SerializableDict class, which it CAN serialize.
  // Unfortunately, the OTHER thing it hates, is templated properties.
  // So we have this dorky class here, to specialize SerializeableDict,
  // so the unity jsonutility doesn't get confused.  Because while it CAN
  // serialize SerializableDict<string, MapElement> if it's the top level,
  // it can't serialize SerializableDict<string, MapElement> as a property.
  // But it CAN serialize StringMapElementDict as a property, even though
  // it's the same thing.
  [System.Serializable]
  public class StringMapElementDict : SerializableDict<string, MapElement> {
  }

  [System.Serializable]
  public class SerializableDict<KeyType, ValueType> {
    public List<KeyType> Keys = new List<KeyType>();
    public List<ValueType> Values = new List<ValueType>();

    public void Add(KeyType key, ValueType value) {
      Keys.Add(key);
      Values.Add(value);
    }

    public void Remove(KeyType key) {
      int index = Keys.IndexOf(key);
      if (index < 0) {
        Debug.LogError("Error - could not find key " + key + " in SerializableDict.");
      }
      else {
        Keys.RemoveAt(index);
        Values.RemoveAt(index);
      }
    }

    public int Count {
      get { return Keys.Count; }
    }

    public void Clear() {
      Keys.Clear();
      Values.Clear();
    }

    public bool ContainsKey(KeyType key) {
      return Keys.Contains(key);
    }

    // [] operators
    public ValueType this[KeyType key] {
      get {
        if (Keys.Contains(key)) {
          return Values[Keys.IndexOf(key)];
        }
        else
          return default(ValueType);
      }
      set {
        Keys.Add(key);
        Values.Add(value);
      }
    }
  }

  [System.Serializable]
  public class MapElement {
    public string type;
    public Vector3 scale = new Vector3(1, 1, 1);
    public Vector3 position = new Vector3(0, 0, 0);
    // Orientation of the tile, in 90 degree increments.  (So an
    // orientation of 1 means 90 degrees, 2 means 180, etc.)
    public int orientation = 0;

    // Takes a map element, and returns the string key that will represent
    // it in the database.  For most objects, this is a function of their
    // coordinates in world-space.  (Because only one object is allowed
    // to be at a given coordinate.)  For unique objects though, (such as
    // start locations) they have a different name, to enforce that they
    // are once-per-map.
    public string GetStringKey() {
      return GetKeyForPosition(position);
    }

    // Returns the string key used for non-unique elements as the given position.
    public static string GetKeyForPosition(Vector3 position) {
      return "obj_" + position.ToString();
    }

    // Returns the string key used for non-unique elements as the given position.
    public static string GetKeyForPosition(int x, int z) {
      return GetKeyForPosition(new Vector3(x, 0, z));
    }
  }
}
