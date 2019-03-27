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
using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Unity.Editor;


namespace Hamster.States {
  // State for asking how the user wants to sign in -
  // Through an auth provider, creating an account, or
  // doing an anonymous signin for later.
  class ChooseSignInMenu : BaseState {

    Firebase.Auth.FirebaseAuth auth;
    Menus.ChooseSignInGUI dialogComponent;

    public override StateExitValue Cleanup() {
      DestroyUI();
      return new StateExitValue(typeof(ChooseSignInMenu), null);
    }

    public override void Resume(StateExitValue results) {
      ShowUI();

      // SignInResult is used with a email/password, while WaitForTask.Results
      // is used when signing in with an anonymous account.
      SignInResult result = results.data as SignInResult;
      WaitForTask.Results taskResult = results.data as WaitForTask.Results;

      if ((result != null && !result.Canceled) ||
          (taskResult != null && !taskResult.task.IsCanceled)) {
        if (auth.CurrentUser != null) {
          CommonData.isNotSignedIn = false;
          manager.PopState();
        } else {
          manager.PushState(new BasicDialog(
              Utilities.StringHelper.SigninInFailureString(taskResult.task)));
        }
      }
    }

    public override void Suspend() {
      HideUI();
    }

    // Initialization method.  Called after the state
    // is added to the stack.
    public override void Initialize() {
      auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
      dialogComponent = SpawnUI<Menus.ChooseSignInGUI>(StringConstants.PrefabsChooseSigninMenu);

      dialogComponent.GooglePlaySignIn.gameObject.SetActive(
          GooglePlayServicesSignIn.GooglePlayServicesEnabled());

      InitializeGameCenterUI();
    }

    private void InitializeGameCenterUI() {
      // Determine whether we should show the button, and whether GameCenter is enabled.
      // Note that these values may differ (for example, we should show the button in the Editor on
      // MacOS to help with debugging and development, but gameCenter doesn't work on that platform,
      // so we don't want to allow the button to be clicked)
      bool showGameCenter = GameCenterSignIn.ShouldShowGameCenterSignIn();
      bool gameCenterEnabled = GameCenterSignIn.IsGameCenterEnabled();

      dialogComponent.GameCenterSignIn.gameObject.SetActive(showGameCenter);

      SetButtonActive(dialogComponent.GameCenterSignIn, false);
      if(gameCenterEnabled) {
        Action updateButtonBasedOnSignedInState = () => {
          bool gameCenterSignedIn = Firebase.Auth.GameCenterAuthProvider.IsPlayerAuthenticated();

          SetButtonActive(dialogComponent.GameCenterSignIn, gameCenterSignedIn);
        };

        GameCenterSignIn.InitializeGameCenterAuthentication(updateButtonBasedOnSignedInState);
      }
    }

    private void SetButtonActive(Hamster.Menus.GUIButton button, bool newActive) {
        button.enabled = newActive;

        var buttonText = button.GetComponentInChildren<UnityEngine.UI.Text>();
        if(buttonText != null) {
          float newAlpha = (newActive ? 1.0f : 0.3f);
          Color newColor = buttonText.color;
          newColor.a = newAlpha;
          buttonText.color = newColor;
        }
    }

    public override void HandleUIEvent(GameObject source, object eventData) {
      if (source == dialogComponent.CreateAccountButton.gameObject) {
        manager.PushState(new CreateAccount());
      } else if (source == dialogComponent.SignInButton.gameObject) {
        manager.PushState(new SignInWithEmail());
      } else if (source == dialogComponent.AnonymousSignIn.gameObject) {
        manager.PushState(
            new WaitForTask(auth.SignInAnonymouslyAsync().ContinueWith(t => {
                  SignInState.SetState(SignInState.State.Anonymous);}),
              StringConstants.LabelSigningIn, true));
      } else if (source == dialogComponent.DontSignIn.gameObject) {
        CommonData.isNotSignedIn = true;
        manager.PopState();
      } else if (source == dialogComponent.GooglePlaySignIn.gameObject) {
        manager.PushState(
            new WaitForTask(GooglePlayServicesSignIn.SignIn(),
              StringConstants.LabelSigningIn, true));
      } else if (source == dialogComponent.GameCenterSignIn.gameObject) {
        manager.PushState(
            new WaitForTask(GameCenterSignIn.SignIn(),
              StringConstants.LabelSigningIn, true));
      }
    }
  }

  public class SignInResult {
    public bool Canceled = false;
    public SignInResult(bool canceled) {
      this.Canceled = canceled;
    }

  }
}
