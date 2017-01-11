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
    public StringMapElementDict elements = new StringMapElementDict();

    public void ResetProperties() {
      name = StringConstants.DefaultMapName;
      mapId = StringConstants.DefaultMapId;
      ownerId = "<<ownerId>>";
    }

    public void SetProperties(string name, string mapId, string ownerId) {
      this.name = name;
      this.mapId = mapId;
      this.ownerId = ownerId;
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
    public Quaternion rotation = Quaternion.identity;

    // Takes a map element, and returns the string key that will represent
    // it in the database.  For most objects, this is a function of their
    // coordinates in world-space.  (Because only one object is allowed
    // to be at a given coordinate.)  For unique objects though, (such as
    // start locations) they have a different name, to enforce that they
    // are once-per-map.
    public string GetStringKey() {
      if (CommonData.prefabs.lookup[type].limitOnePerMap)
        return type;
      else
        return "obj_" + position.ToString();
    }
  }
}
