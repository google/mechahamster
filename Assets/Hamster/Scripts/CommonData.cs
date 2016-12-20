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
    public const string kDBMapTablePath = "MapList/";
    public const string kDBUserTablePath = "DB_Users/";

    // X-Z plane at height 0
    public static Plane kZeroPlane = new Plane(Vector3.up, new Vector3(0, 0, 0));
  }
}
