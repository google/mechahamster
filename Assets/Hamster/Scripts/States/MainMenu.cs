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

    private Menus.MainMenuGUI menuComponent;

    public MainMenu() {
      // Initialize some styles that we'll for the title.
      titleStyle = new GUIStyle();
      titleStyle.alignment = TextAnchor.UpperCenter;
      titleStyle.fontSize = 50;
      titleStyle.normal.textColor = Color.white;

      subTitleStyle = new GUIStyle();
      subTitleStyle.alignment = TextAnchor.UpperCenter;
      subTitleStyle.fontSize = 20;
      subTitleStyle.normal.textColor = Color.white;
    }

    // Initialization method.  Called after the state
    // is added to the stack.
    public override void Initialize() {
      SetFirebaseMessagingListeners();
      SetFirebaseInvitesListeners();
      menuComponent = SpawnUI<Menus.MainMenuGUI>(StringConstants.PrefabMainMenu);

      // Editor is disabled in VR mode.
      menuComponent.EditorButton.gameObject.SetActive(!CommonData.inVrMode);
      // Only display the shared/bonus levels if the user has at least one.
      menuComponent.SharedLevelsButton.gameObject.SetActive(
        CommonData.currentUser.data.sharedMaps.Count > 0);
      menuComponent.BonusLevelsButton.gameObject.SetActive(
          CommonData.currentUser.data.bonusMaps.Count > 0);
    }

    public override void Resume(StateExitValue results) {
      SetFirebaseMessagingListeners();
      SetFirebaseInvitesListeners();
      menuComponent = SpawnUI<Menus.MainMenuGUI>(StringConstants.PrefabMainMenu);
      menuComponent.gameObject.SetActive(true);
    }

    public override void Suspend() {
      RemoveFirebaseMessagingListeners();
      RemoveFirebaseInvitesListeners();
      menuComponent.gameObject.SetActive(false);
    }

    public override StateExitValue Cleanup() {
      RemoveFirebaseMessagingListeners();
      DestroyUI();
      return null;
    }

    public override void HandleUIEvent(GameObject source, object eventData) {
      if (source == menuComponent.PlayButton.gameObject) {
        manager.SwapState(new LevelSelect());
      } else if (source == menuComponent.EditorButton.gameObject) {
        manager.SwapState(new Editor());
      } else if (source == menuComponent.SharedLevelsButton.gameObject) {
        manager.SwapState(new SharedLevelSelect());
      } else if (source == menuComponent.BonusLevelsButton.gameObject) {
        manager.SwapState(new BonusLevelSelect());
      } else if (source == menuComponent.AccountButton.gameObject) {
        manager.SwapState(new ManageAccount());
      }
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
