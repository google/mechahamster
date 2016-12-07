using UnityEngine;
using System.Collections;

namespace Hamster.MapObjects {

  // Clsas that governms behavior of goal tiles - tiles that
  // complete the level when stepped on.
  // Note that this code is largely placeholder - for now it just displays
  // a message when stepped on.  Final flow will be a bit different.
  public class GoalTile : MapObject {
    System.DateTime lastTouch = System.DateTime.MinValue;

    // At the moment, these just display a win message.
    // TODO: End game, bring up different menu, etc.
    protected override void MapObjectActivation(Collider collider) {
      lastTouch = System.DateTime.Now;
    }

    void OnGUI() {
      System.TimeSpan delta = System.DateTime.Now.Subtract(lastTouch);
      if (delta.TotalSeconds <= 5) {
        GUI.skin = FindObjectOfType<PrefabList>().guiSkin;

        UnityEngine.GUIStyle centeredStyle = GUI.skin.GetStyle("Label");
        centeredStyle.alignment = TextAnchor.UpperCenter;
        GUI.Label(new Rect(Screen.width / 2 - 400,
          Screen.height / 2 - 50, 800, 100), "A WINNNER IS YOU", centeredStyle);
      }
    }
  }
}