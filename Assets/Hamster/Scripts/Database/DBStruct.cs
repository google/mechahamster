// Copyright 2016 Google Inc. All rights reserved.
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

public class DBStruct<T> where T : new() {
  // Holds an arbitrary class or structure.
  // Note that T needs to be serializable, and should not contain
  // any templated members, or they will fail to render in JSON.
  // See https://docs.unity3d.com/ScriptReference/JsonUtility.ToJson.html
  // for the full limits on what can be serialized.
  // The main difference between this and DBTable, is that
  // the table is a dict of T, and this is just one instance.

  // Database path to the struct.
  public string dbPathName = "<<UNNAMED STRUCT>>";
  public bool areChangesPending { get; private set; }

  public T data { get; private set; }
  public T newData { get; private set; }

  Firebase.Database.FirebaseDatabase database;
  Firebase.FirebaseApp app;

  DBStruct() {
    DiscardRemoteChanges();
  }

  public DBStruct(string name, Firebase.FirebaseApp app) {
    this.app = app;
    database = Firebase.Database.FirebaseDatabase.GetInstance(this.app);
    dbPathName = name;
    data = new T();
    newData = new T();
    database.GetReference(dbPathName).ValueChanged += OnDataChanged;
  }

  public void ApplyRemoteChanges() {
    if (areChangesPending) {
      data = newData;
      DiscardRemoteChanges();
    }
  }

  public void DiscardRemoteChanges() {
    areChangesPending = false;
  }

  // Returns a guaranteed unique string, usable as a dictionary key value.
  public string GetUniqueKey() {
    return database.RootReference.Child(dbPathName).Push().Key;
  }

  public void Initialize(T value) {
    data = value;
    newData = value;
    DiscardRemoteChanges();
    PushData();
  }

  void OnDataChanged(object sender, Firebase.Database.ValueChangedEventArgs args) {
    if (args.DatabaseError != null) {
      Debug.LogError("Something went wrong - database error on struct [" + dbPathName + "]!\n"
          + args.DatabaseError.ToString());
      return;
    }
    T newValue = JsonUtility.FromJson<T>(args.Snapshot.GetRawJsonValue());
    newData = newValue;

    areChangesPending = true;
  }

  public void PushData() {
    UnityEngine.Assertions.Assert.IsNotNull(database, "Database ref is null!");
    string json = JsonUtility.ToJson(data);
    database.RootReference.Child(dbPathName).SetRawJsonValueAsync(json);
  }
}
