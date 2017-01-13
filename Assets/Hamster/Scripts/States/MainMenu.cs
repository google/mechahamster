using UnityEngine;
using System.Collections.Generic;

namespace Hamster.States {
  class MainMenu : BaseState {
    // Width/Height of the menu, expressed as a portion of the screen width:
    const float MenuWidth = 0.40f;
    const float MenuHeight = 0.75f;

    private GUIStyle titleStyle;
    private GUIStyle subTitleStyle;

    private Stack<BaseState> statesToShow = new Stack<BaseState>();
    private Object stateStackLock = new Object();


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
      SetFirebaseMessagingListeners();
    }

    public override void Resume(StateExitValue results) {
      SetFirebaseMessagingListeners();
    }

    public override void Suspend() {
      RemoveFirebaseMessagingListeners();
    }

    public override StateExitValue Cleanup() {
      RemoveFirebaseMessagingListeners();
      return null;
    }

    // Called once per frame for GUI creation, if the state is active.
    public override void OnGUI() {
      float menuWidth = MenuWidth * Screen.width;
      float menuHeight = MenuHeight * Screen.height;
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
      if (GUILayout.Button(StringConstants.ButtonPlayShared)) {
        manager.PushState(new BasicDialog("Not yet implemented"));
      }
      if (GUILayout.Button(StringConstants.ButtonPlayBonus)) {
        manager.PushState(new BonusLevelSelect());
      }
      GUILayout.EndVertical();
      GUILayout.EndArea();
    }

    // Update function.  If any states are waiting to be shown, swap to them.
    public override void Update() {
      if (statesToShow.Count != 0) {
        manager.PushState(statesToShow.Pop());
      }
    }

    // Helper function for adding states that need to be shown.
    // Made a helper function, because it needs a lock, in case
    // randomly firing listeners cause race conditions.
    private void QueueState(BaseState newState) {
      lock(stateStackLock) {
        statesToShow.Push(newState);
      }
    }

    private void SetFirebaseMessagingListeners() {
      Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;
    }

    private void RemoveFirebaseMessagingListeners() {
      Firebase.Messaging.FirebaseMessaging.MessageReceived -= OnMessageReceived;
    }

    public void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e) {
      QueueState(new MessageReceived(e));
    }
  }
}
