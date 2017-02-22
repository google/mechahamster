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
      dialogComponent.gameObject.SetActive(true);

      SignInResult result = results.data as SignInResult;

      if (result != null && !result.Canceled) {
#if UNITY_EDITOR
        manager.PopState();
#else
        if (auth.CurrentUser != null) {
          manager.PopState();
        } else {
          manager.PushState(new BasicDialog("Error signing in."));
        }
#endif
      }
    }

    public override void Suspend() {
      dialogComponent.gameObject.SetActive(false);
    }

    // Initialization method.  Called after the state
    // is added to the stack.
    public override void Initialize() {
      auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
      dialogComponent = SpawnUI<Menus.ChooseSignInGUI>(StringConstants.PrefabsChooseSigninMenu);
    }

    public override void HandleUIEvent(GameObject source, object eventData) {
      if (source == dialogComponent.CreateAccountButton.gameObject) {
        manager.PushState(new CreateAccount());
      } else if (source == dialogComponent.SignInButton.gameObject) {
        manager.PushState(new SignInWithEmail());
      } else if (source == dialogComponent.SignInLaterButton.gameObject) {
        manager.PushState(
            new WaitForTask(auth.SignInAnonymouslyAsync(),
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
