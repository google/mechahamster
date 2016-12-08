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

    // X-Z plane at height 0
    public static Plane kZeroPlane = new Plane(Vector3.up, new Vector3(0, 0, 0));
  }
}
