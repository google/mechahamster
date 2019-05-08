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
      CommonData.mainGame.SelectAndPlayMusic(CommonData.prefabs.menuMusic, true);
      SetFirebaseMessagingListeners();
      InitializeUI();
    }

    public override void Resume(StateExitValue results) {
      CommonData.mainGame.SelectAndPlayMusic(CommonData.prefabs.menuMusic, true);
      // Whenever we come back to the main menu, check:
      // If we have authentication data, but haven't fetched user data
      // yet, go try to get user data.
      bool shouldFetchUserData ;
      if (results == null) {
        shouldFetchUserData = true;
      } else if (results.sourceState != typeof(FetchUserData) && results.sourceState != typeof(WaitForTask)) {
        shouldFetchUserData = true;
      } else {
        shouldFetchUserData = false;
      }
      if (shouldFetchUserData) {
        Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        if (auth.CurrentUser != null && CommonData.currentUser == null) {
          manager.PushState(new FetchUserData(auth.CurrentUser.UserId));
          return;
        }
      }
      SetFirebaseMessagingListeners();
      InitializeUI();
    }

    private void InitializeUI() {
      if (menuComponent == null) {
        menuComponent = SpawnUI<Menus.MainMenuGUI>(StringConstants.PrefabMainMenu);
      }
      ShowUI();
      // If you're not signed in, many features are disabled.

      // Only display the shared/bonus levels if the user has at least one.
      menuComponent.SharedLevelsButton.gameObject.SetActive(
          CommonData.ShowInternetMenus() && CommonData.currentUser.data.sharedMaps.Count > 0);
      menuComponent.BonusLevelsButton.gameObject.SetActive(
          CommonData.ShowInternetMenus() && CommonData.currentUser.data.bonusMaps.Count > 0);

      // Display the account button on all devices, including in the Unity Editor
      menuComponent.AccountButton.gameObject.SetActive(CommonData.ShowInternetMenus());

      // Editor is disabled in VR mode.
      menuComponent.EditorButton.gameObject.SetActive(
          !CommonData.inVrMode && CommonData.ShowInternetMenus());

      // If you're NOT signed in, the main menu has a button to sign in:
      menuComponent.SignInButton.gameObject.SetActive(CommonData.isNotSignedIn);
    }

    public override void Suspend() {
      RemoveFirebaseMessagingListeners();
      HideUI();
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
        Firebase.Analytics.FirebaseAnalytics.LogEvent(StringConstants.AnalyticsEventEditorOpened);
        manager.SwapState(new Editor());
      } else if (source == menuComponent.SharedLevelsButton.gameObject) {
        manager.SwapState(new SharedLevelSelect());
      } else if (source == menuComponent.BonusLevelsButton.gameObject) {
        manager.SwapState(new BonusLevelSelect());
      } else if (source == menuComponent.AccountButton.gameObject) {
        manager.PushState(new ManageAccount());
      } else if (source == menuComponent.SettingsButton.gameObject) {
        manager.PushState(new SettingsMenu());
      } else if (source == menuComponent.LicenseButton.gameObject) {
        manager.PushState(new LicenseDialog());
      } else if (source == menuComponent.SignInButton.gameObject) {
        manager.PushState(new ChooseSignInMenu());
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
