using UnityEngine;
using System.Collections;

// List of prefabs used by the game.
// A game object uses this component, and can then be used
// to access and instantiate the prefabs.  (Also, this serves to
// force unity to include these prefabs - otherwise Unity automatically
// strips unused resources from release builds:
// https://docs.unity3d.com/Manual/iphone-playerSizeOptimization.html)
public class PrefabList : MonoBehaviour {
public GameObject jumpTile;
  public GameObject startPos;
  public GameObject goal;
  public GameObject wall;
  public GameObject floor;

  public UnityEngine.GUISkin guiSkin;
}
