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
using FirebaseTestLab;

namespace Hamster {

  // Container class for constants, statics, and other data
  // that needs to be cached or shared between different components
  // or states.
  class CommonData {
    public static PrefabList prefabs;
    public static GameWorld gameWorld;
    public static CameraController mainCamera;
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

    // Whether we're in VR mode or not.  Set at startup by VRSystemSetup.
    public static bool inVrMode = false;
    public static GameObject vrPointer;

    // Data to be used in current replay.  Null, if no replay is playing.
    public static string currentReplayData = null;

    // TestLabManager to track the state of the tests coming in from the intent
    public static TestLabManager testLab = TestLabManager.Instantiate();

    // Whether we're signed in or not.
    public static bool isNotSignedIn = false;

    // Utility function to see if we should be showing menu options that
    // require internet access and a user profile.
    public static bool ShowInternetMenus() {
      return !(isNotSignedIn || currentUser == null);
    }
  }
}
