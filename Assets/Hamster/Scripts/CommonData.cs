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

namespace Hamster {

  // Container class for constants, statics, and other data
  // that needs to be cached or shared between different components
  // or states.
  class CommonData {
    public static PrefabList prefabs;
    public static GameWorld gameWorld;
    public static Camera mainCamera;
    public static Firebase.FirebaseApp app;
    public static MainGame mainGame;
    public static DBStruct<UserData> currentUser;

    // Paths to various database tables:
    // Trailing slashes required, because in some cases
    // we append further paths onto these.
    public const string DBMapTablePath = "MapList/";
    public const string DBBonusMapTablePath = "BonusMaps/";
    public const string DBUserTablePath = "DB_Users/";

    // X-Z plane at height 0
    public static Plane kZeroPlane = new Plane(Vector3.up, new Vector3(0, 0, 0));
  }
}
