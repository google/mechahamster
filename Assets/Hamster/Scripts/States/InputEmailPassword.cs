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
  // Generic class for handling screens that request the user's
  // email and password, and then start a task based on it.
  // Doesn't do much by itself, but is designed to be inherited from.
  public class InputEmailPassword : BaseState {

    protected string email = "";
    protected string password = "";
    protected string message = "";

    protected string labelText = "";
    protected string buttonText = "";
    protected string failText = "";

    protected virtual void OnButton() { }

    protected Firebase.Auth.FirebaseAuth auth;

    public override void Resume(StateExitValue results) {
      if (results != null) {
        if (results.sourceState == typeof(WaitForTask)) {
          WaitForTask.Results taskResults = results.data as WaitForTask.Results;
          if (taskResults.task.IsFaulted) {
            message = failText + taskResults.task.Exception.ToString();
          } else {
            manager.PopState();
          }
        }
      }
    }

    // Initialization method.  Called after the state
    // is added to the stack.
    public override void Initialize() {
      auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
      Time.timeScale = 0.0f;
    }

    // Called once per frame for GUI creation, if the state is active.
    // TODO(ccornell): This needs some layout attention, if it's going
    // to see much use.  Needs to be centered at a minimum.
    public override void OnGUI() {
      GUI.skin = CommonData.prefabs.guiSkin;
      GUILayout.BeginVertical();
      GUILayout.Label(labelText);
      GUILayout.BeginHorizontal();
      GUILayout.Label(labelText);
      email = GUILayout.TextField(email, GUILayout.Width(Screen.width / 2));
      GUILayout.EndHorizontal();
      GUILayout.BeginHorizontal();
      GUILayout.Label(StringConstants.LabelPassword);
      password = GUILayout.PasswordField(password, '*', GUILayout.Width(Screen.width / 2));
      GUILayout.EndHorizontal();

      if (GUILayout.Button(buttonText)) {
        OnButton();
      }

      if (GUILayout.Button(StringConstants.ButtonCancel)) {
        manager.PopState();
      }
      GUILayout.Label(message);
      GUILayout.EndVertical();
    }
  }
  
  // Screen for asking the user to create a new account
  public class CreateAccount : InputEmailPassword {

    public CreateAccount() {
      labelText = StringConstants.LabelCreateAccount;
      buttonText = StringConstants.ButtonCreateAccount;
      failText = "Could not create account: ";
    }

    protected override void OnButton() {
      manager.PushState(
        new WaitForTask(auth.CreateUserWithEmailAndPasswordAsync(email, password)));
    }

    public override StateExitValue Cleanup() {
      return new StateExitValue(typeof(CreateAccount));
    }
  }


  // Screen for asking the user to sign in to an existing account.
  public class SignInEmail : InputEmailPassword {

    public SignInEmail() {
      labelText = StringConstants.LabelSignIn;
      buttonText = StringConstants.ButtonSignIn;
      failText = "Could not sign in: ";
    }

    protected override void OnButton() {
      manager.PushState(
        new WaitForTask(auth.SignInWithEmailAndPasswordAsync(email, password)));
    }

    public override StateExitValue Cleanup() {
      return new StateExitValue(typeof(SignInEmail));
    }
  }

  // Screen for asking the user to add an email/password to
  // an anonymous account.
  public class AddEmailToAccount : InputEmailPassword {

    public AddEmailToAccount() {
      labelText = StringConstants.LabelAddEmail;
      buttonText = StringConstants.ButtonAddEmailPassword;
      failText = "Could not add email: ";
    }

    protected override void OnButton() {
      Firebase.Auth.Credential credential =
          Firebase.Auth.EmailAuthProvider.GetCredential(email, password);
      manager.PushState(
        new WaitForTask(auth.CurrentUser.LinkWithCredentialAsync(credential), "linking account..."));
    }

    public override StateExitValue Cleanup() {
      return new StateExitValue(typeof(AddEmailToAccount));
    }
  }
}
