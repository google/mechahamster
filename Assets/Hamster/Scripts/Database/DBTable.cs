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
using System.Threading;

public class DBTable<T> {
  // Abstraction for a table on the database, containing
  // a keyed set of an arbitrary class or structure.
  // Useful for large datasets, where you don't want to have to send
  // the whole table every time a single value changes.  (Changes
  // to a single value will only involve transmitting/receiving that element.
  // Unlike DBStruct, which transmits the entire object whenever any
  // part of it changes.)
  //
  // Note that T needs to be serializable, and should not contain
  // any templated members, or they will fail to render in JSON.
  // See https://docs.unity3d.com/ScriptReference/JsonUtility.ToJson.html
  // for the full limits on what can be serialized.

  public string tableName = "<<UNNAMED TABLE>>";
  public Dictionary<string, DBObj<T>> data { get; private set; }
  public Dictionary<string, T> newData { get; private set; }
  public List<string> deletedEntries { get; private set; }
  public bool areChangesPending { get; private set; }

  private Object clearMutexLock = new Object();
  private Object applyChangeLock = new Object();

  DBTable() {
    DiscardRemoteChanges();
  }

  Firebase.Database.FirebaseDatabase database;
  Firebase.FirebaseApp app;

  public DBTable(string name, Firebase.FirebaseApp app) {
    this.app = app;
    tableName = name;
    database = Firebase.Database.FirebaseDatabase.GetInstance(this.app);
    data = new Dictionary<string, DBObj<T>>();
    newData = new Dictionary<string, T>();
    deletedEntries = new List<string>();

    addListeners();
  }

  private void addListeners() {
    Firebase.Database.DatabaseReference dbRef = database.GetReference(tableName);
    dbRef.ChildAdded += OnChildAdded;
    dbRef.ChildChanged += OnChildChanged;
    dbRef.ChildRemoved += OnChildRemoved;
  }

  private void removeListeners() {
    Firebase.Database.DatabaseReference dbRef = database.GetReference(tableName);
    dbRef.ChildAdded -= OnChildAdded;
    dbRef.ChildChanged -= OnChildChanged;
    dbRef.ChildRemoved -= OnChildRemoved;
  }

  public void Add(string key, T value) {
    data.Add(key, new DBObj<T>(value));
    data[key].isDirty = true;
  }

  public DBObj<T> this[string key] {
    get {
      if (data.ContainsKey(key))
        return data[key];
      else
        return null;
    }
    set {
      data.Add(key, value);
    }
  }

  // Override this to make custom nonsense like tables that have more
  // than one type in them.
  public virtual T GetFromJson(string json) {
    return JsonUtility.FromJson<T>(json);
  }

  void OnChildAdded(object sender, Firebase.Database.ChildChangedEventArgs args) {
    bool lockSuccess = Monitor.TryEnter(clearMutexLock);
    if (!lockSuccess) return; // We don't care about changes if we're already mid-clear.
    if (args.DatabaseError != null) {
      Debug.LogError(args.DatabaseError);
      return;
    }
    T newValue = GetFromJson(args.Snapshot.GetRawJsonValue());
    lock (applyChangeLock) {
      string key = args.Snapshot.Key;
      newData[key] = newValue;
      areChangesPending = true;
    }
    Monitor.Exit(clearMutexLock);
  }

  void OnChildChanged(object sender, Firebase.Database.ChildChangedEventArgs args) {
    if (!Monitor.TryEnter(clearMutexLock)) return;
    if (args.DatabaseError != null) {
      Debug.LogError(args.DatabaseError);
      return;
    }
    T newValue = GetFromJson(args.Snapshot.GetRawJsonValue());
    lock (applyChangeLock) {
      string key = args.Snapshot.Key;
      newData[key] = newValue;
      areChangesPending = true;
    }
    Monitor.Exit(clearMutexLock);
  }

  void OnChildRemoved(object sender, Firebase.Database.ChildChangedEventArgs args) {
    if (!Monitor.TryEnter(clearMutexLock)) return;
    if (args.DatabaseError != null) {
      Debug.LogError(args.DatabaseError);
      return;
    }
    lock (applyChangeLock) {
      string key = args.Snapshot.Key;
      newData.Remove(key);
      data.Remove(key);
      if (!deletedEntries.Contains(key)) {
        deletedEntries.Add(key);
      }
      areChangesPending = true;
    }
    Monitor.Exit(clearMutexLock);
  }

  public void ApplyRemoteChanges() {
    lock (applyChangeLock) {
      if (areChangesPending) {

        foreach (string key in deletedEntries) {
          data.Remove(key);
        }
        foreach (KeyValuePair<string, T> pair in newData) {
          if (!data.ContainsKey(pair.Key)) {
            data[pair.Key] = new DBObj<T>();
          }
          data[pair.Key].data = pair.Value;
        }
        areChangesPending = false;
      }
    }
  }

  public void DiscardRemoteChanges() {
    areChangesPending = false;
  }

  // Returns a guaranteed unique string, usable as a key value.
  public string GetUniqueKey() {
    return database.RootReference.Child(tableName).Push().Key;
  }

  // Clears out the table on the server.
  public void Clear() {
    lock (clearMutexLock) {
      database.RootReference.Child(tableName).SetValueAsync(null).ContinueWith(task => {
        if (task.IsFaulted)
          Debug.LogError("Task faulted!\n" + task.Exception.ToString());
        data.Clear();
        newData.Clear();
        deletedEntries.Clear();
        DiscardRemoteChanges();
      });
    }
  }

  public void PushData() {
    lock (applyChangeLock) {
      UnityEngine.Assertions.Assert.IsNotNull(database, "Database ref is null!");
      foreach (KeyValuePair<string, DBObj<T>> pair in data) {
        if (pair.Value.isDirty) {
          string json = JsonUtility.ToJson(pair.Value.data);
          if (json == "{}")
            Debug.LogError("Warning - DBTable serialized [" + typeof(T).ToString()
             + "] as an empty string.\nMake sure at least one member is public!");
          database.RootReference.Child(tableName).Child(pair.Key).SetRawJsonValueAsync(json);
          pair.Value.isDirty = false;
        }
      }
    }
  }
}

public class DBObj<T> {
  public bool isDirty = true;
  public T data;

  public DBObj() { }

  public DBObj(T data) {
    this.data = data;
  }
}