using UnityEngine;

namespace Hamster.States {
  class MainMenu : BaseState {
    // Width/Height of the menu, expressed as a portion of the screen width:
    const float kMenuWidth = 0.25f;
    const float kMenuHeight = 0.75f;

    private GUIStyle titleStyle;
    private GUIStyle subTitleStyle;

    public MainMenu() {
      // Initialize some styles that we'll for the title.
      titleStyle = new GUIStyle();
      titleStyle.alignment = TextAnchor.UpperCenter;
      titleStyle.fontSize = 50;

      subTitleStyle = new GUIStyle();
      subTitleStyle.alignment = TextAnchor.UpperCenter;
      subTitleStyle.fontSize = 20;
    }

    // Initialization method.  Called after the state
    // is added to the stack.
    public override void Initialize() {
      Time.timeScale = 0.0f;
    }

    // Called once per frame for GUI creation, if the state is active.
    public override void OnGUI() {
      float menuWidth = kMenuWidth * Screen.width;
      float menuHeight = kMenuHeight * Screen.height;
      GUI.skin = CommonData.prefabs.guiSkin;

      UnityEngine.GUIStyle centeredStyle = GUI.skin.GetStyle("Label");

      GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height - menuHeight));
      centeredStyle.alignment = TextAnchor.UpperCenter;

      GUILayout.Label(StringConstants.TitleText, titleStyle);
      GUILayout.Label(StringConstants.SubTitleText, subTitleStyle);

      GUILayout.EndArea();

      GUILayout.BeginArea(
          new Rect((Screen.width - menuWidth) / 2, (Screen.height - menuHeight) / 2,
          menuWidth, menuHeight));

      GUILayout.BeginVertical();
      if (GUILayout.Button(StringConstants.ButtonPlay)) {
        manager.SwapState(new LevelSelect());
      }
      if (GUILayout.Button(StringConstants.ButtonEditor)) {
        manager.SwapState(new States.Editor());
      }
      GUILayout.EndVertical();
      GUILayout.EndArea();
    }
  }
}
