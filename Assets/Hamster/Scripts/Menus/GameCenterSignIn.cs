using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Firebase.Auth;

namespace Hamster {
  /// <summary>
  /// Manages interactions with GameCenter
  /// </summary>
  public class GameCenterSignIn {
    private class FetchCredentialFailedException : Exception {
      public FetchCredentialFailedException(string message) : base(message) {}
    }

    private class SignInFailedException : Exception {
      public SignInFailedException(string message) : base(message) {}
    }

    public static bool IsGameCenterEnabled() {
      // Platforms where we should enable GameCenter authentication
      HashSet<RuntimePlatform> enableGameCenterPlatforms = new HashSet<RuntimePlatform> { RuntimePlatform.IPhonePlayer };

      bool enableGameCenter = enableGameCenterPlatforms.Contains(Application.platform);
      return enableGameCenter;
    }

    public static bool ShouldShowGameCenterSignIn() {
      // Platforms where GameCenter authentication isn't enabled, but we
      // should still show the relevant UI (i.e. in the Editor for testing)
      HashSet<RuntimePlatform> showGameCenterPlatforms =
                                        new HashSet<RuntimePlatform> { RuntimePlatform.OSXEditor };

      bool showGameCenter = IsGameCenterEnabled() ||
                                showGameCenterPlatforms.Contains(Application.platform);
      return showGameCenter;
    }

    /// <summary>
    /// Signs into Game Center account and Firebase (creating an account
    /// automatically if needed).
    /// </summary>
    /// <returns>The task that will be completed when SignIn is completed.</returns>
    public static Task<FirebaseUser> SignIn() {
      if (Firebase.Auth.GameCenterAuthProvider.IsPlayerAuthenticated()) {
        var credentialFuture = Firebase.Auth.GameCenterAuthProvider.GetCredentialAsync();
        var retUserFuture = credentialFuture.ContinueWith(credentialTask => {
          if(credentialTask.IsFaulted)
            throw credentialTask.Exception;
          if(!credentialTask.IsCompleted)
            throw new FetchCredentialFailedException(
                    "Game Center SignIn() failed to fetch credential.");

          var credential = credentialTask.Result;
          var userFuture = FirebaseAuth.DefaultInstance.SignInWithCredentialAsync(credential);
          return userFuture;
        }).Unwrap().ContinueWith(userTask => {
          if(userTask.IsFaulted)
            throw userTask.Exception;
          if(!userTask.IsCompleted)
            throw new SignInFailedException(
                    "Game Center SignIn() failed to Sign In with Credential.");

          SignInState.SetState(SignInState.State.GameCenter);
          return userTask.Result;
        });

        return retUserFuture;
      } else {
        TaskCompletionSource<FirebaseUser> taskCompletionSource =
          new TaskCompletionSource<FirebaseUser>();

        taskCompletionSource.SetException(
          new SignInFailedException(
            "Game Center SignIn() failed - User not authenticated to Game Center."));
          return taskCompletionSource.Task;
      }
    }

    /// <summary>
    /// Attempt to authenticate the user on the device. Required before Firebase
    /// Game Center authentication can be used.
    /// </summary>
    public static void InitializeGameCenterAuthentication(Action onAuthenticationComplete = null) {
#if UNITY_IOS
      Social.localUser.Authenticate(success => {
        Debug.Log("Game Center Initialization Complete - Result: " + success);
        if (onAuthenticationComplete != null)
          onAuthenticationComplete();
      });
#else
      Debug.Log("InitializeGameCenterAuthentication failed - Game Center is not supported " +
                  "on this platform.");
#endif
    }
  }
}
