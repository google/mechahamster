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
  class SignInMenu : BaseState {

    Firebase.Auth.FirebaseAuth auth;

    bool signedIn = false;

    public override StateExitValue Cleanup() {
      return new StateExitValue(typeof(SignInMenu), null);
    }

    public override void Resume(StateExitValue results) {
#if (UNITY_EDITOR)
      manager.PopState();
#else
      if (auth.CurrentUser != null) {
        manager.PopState();
      } else {
        manager.PushState(new BasicDialog("Error signing in."));
      }
#endif
    }

    // Initialization method.  Called after the state
    // is added to the stack.
    public override void Initialize() {
      auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
    }

    // Called once per frame for GUI creation, if the state is active.
    // TODO(ccornell): This needs some layout attention, if it's going
    // to see much use.  Needs to be centered at a minimum.
    public override void OnGUI() {
      bool inVrMode = false;
      // TODO(ccornell) - DAYDREAM SCAFFOLDING
      VRSystemSetup vrSetup = GameObject.FindObjectOfType<VRSystemSetup>();
      if (vrSetup) {
        inVrMode = vrSetup.StartInVRMode;
      }

      GUI.skin = CommonData.prefabs.guiSkin;
      GUILayout.BeginVertical();
      if (GUILayout.Button(StringConstants.ButtonSignInWithEmail)) {
        manager.PushState(new SignInEmail());
      }
      if (GUILayout.Button(StringConstants.ButtonCreateAccount)) {
        manager.PushState(new CreateAccount());
      }
      if (GUILayout.Button(StringConstants.ButtonSignInAnonymously) || inVrMode) {
        manager.PushState(
            new WaitForTask(auth.SignInAnonymouslyAsync(),
              StringConstants.LabelSigningIn, true));
      }
      GUILayout.EndVertical();
    }
  }
}
