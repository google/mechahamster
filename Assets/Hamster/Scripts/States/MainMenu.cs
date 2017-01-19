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
      SetFirebaseInvitesListeners();
    }

    public override void Resume(StateExitValue results) {
      SetFirebaseMessagingListeners();
      SetFirebaseInvitesListeners();
    }

    public override void Suspend() {
      RemoveFirebaseMessagingListeners();
      RemoveFirebaseInvitesListeners();
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
      if (CommonData.currentUser.data.sharedMaps.Count > 0) {
        if (GUILayout.Button(StringConstants.ButtonPlayShared)) {
          manager.PushState(new SharedLevelSelect());
        }
      }
      if (CommonData.currentUser.data.bonusMaps.Count > 0) {
        if (GUILayout.Button(StringConstants.ButtonPlayBonus)) {
          manager.PushState(new BonusLevelSelect());
        }
      }

      if (GUILayout.Button(StringConstants.ButtonAccount)) {
        manager.PushState(new ManageAccount());
      }

      GUILayout.EndVertical();
      GUILayout.EndArea();
    }

    // Update function.  If any states are waiting to be shown, swap to them.
    public override void Update() {
      if (statesToShow.Count != 0) {
        lock (stateStackLock) {
          manager.PushState(statesToShow.Pop());
        }
      }
    }

    // Helper function for adding states that need to be shown.
    // Made a helper function, because it needs a lock, in case
    // randomly firing listeners cause race conditions.
    private void QueueState(BaseState newState) {
      lock (stateStackLock) {
        statesToShow.Push(newState);
      }
    }

    private void SetFirebaseMessagingListeners() {
      Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;
    }

    private void RemoveFirebaseMessagingListeners() {
      Firebase.Messaging.FirebaseMessaging.MessageReceived -= OnMessageReceived;
    }

    private void SetFirebaseInvitesListeners() {
      Firebase.Invites.FirebaseInvites.InviteReceived += OnInviteReceived;
      Firebase.Invites.FirebaseInvites.InviteNotReceived += OnInviteNotReceived;
      Firebase.Invites.FirebaseInvites.ErrorReceived += OnErrorReceived;
    }

    private void RemoveFirebaseInvitesListeners() {
      Firebase.Invites.FirebaseInvites.InviteReceived -= OnInviteReceived;
      Firebase.Invites.FirebaseInvites.InviteNotReceived -= OnInviteNotReceived;
      Firebase.Invites.FirebaseInvites.ErrorReceived -= OnErrorReceived;
    }

    public void OnInviteReceived(object sender,
                                 Firebase.Invites.InviteReceivedEventArgs e) {
      QueueState(new InviteReceived(e));
    }

    public void OnInviteNotReceived(object sender, System.EventArgs e) {
      Debug.Log("No Invite or Deep Link received on start up");
    }

    public void OnErrorReceived(object sender,
                                Firebase.Invites.InviteErrorReceivedEventArgs e) {
      Debug.LogError("Error occurred received the invite: " +
          e.ErrorMessage);
    }

    void HandleConversionResult(System.Threading.Tasks.Task convertTask) {
      if (convertTask.IsCanceled) {
        Debug.Log("Conversion canceled.");
      } else if (convertTask.IsFaulted) {
        Debug.Log("Conversion encountered an error:");
        Debug.Log(convertTask.Exception.ToString());
      } else if (convertTask.IsCompleted) {
        Debug.Log("Conversion completed successfully!");
        Debug.Log("ConvertInvitation: Successfully converted invitation");
      }
    }

    public void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e) {
      QueueState(new MessageReceived(e));
    }
  }
}
