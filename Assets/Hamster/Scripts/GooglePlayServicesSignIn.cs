#if GPGS

using System;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine;
using System.Threading.Tasks;
using Firebase.Auth;

namespace Hamster
{
  /// <summary>
  /// Manages interactions with Google Play Games.
  /// </summary>
  public class GooglePlayServicesSignIn
  {
    /// <summary>
    /// Whether or not the game is allowed to auto sign in.
    /// </summary>
    /// <returns><c>true</c>, if auto sign in was allowed, <c>false</c> otherwise.</returns>
    public static bool CanAutoSignIn() {
      SignInState.State state = SignInState.GetState();
      return state == SignInState.State.GooglePlayServices || state == SignInState.State.Unknown;
    }

    /// <summary>
    /// Whether or not Google Play Games are enabled.
    /// </summary>
    public static bool GooglePlayServicesEnabled() {
      return true;
    }

    /// <summary>
    /// Private helper that signs the into Google Play Games and performs
    /// a Firebase account operation.
    /// </summary>
    private static Task<FirebaseUser> SignIntoGooglePlayServices(
      Action<Firebase.Auth.Credential,
      TaskCompletionSource<FirebaseUser>> operation)
    {
      TaskCompletionSource<FirebaseUser> taskCompletionSource =
        new TaskCompletionSource<FirebaseUser>();

      UnityEngine.Social.localUser.Authenticate((bool success) => {
        if (success) {
          String authCode = PlayGamesPlatform.Instance.GetServerAuthCode();
          if (String.IsNullOrEmpty(authCode)) {
            Debug.LogError(@"Signed into Play Games Services but failed to get the server auth code,
            will not be able to sign into Firebase");
            taskCompletionSource.SetException(new AggregateException(
                new[] { new ApplicationException() }));
            return;
          }

          Credential credential = PlayGamesAuthProvider.GetCredential(authCode);
          operation(credential, taskCompletionSource);
        } else {
          Debug.LogError("Failed to sign into Play Games Services");
          taskCompletionSource.SetException(new AggregateException(
              new[] { new ApplicationException() }));
        }
      });

      return taskCompletionSource.Task;
    }

    /// <summary>
    /// Links the Google Play Games account to the current Firebase one (anonymous or email).
    /// </summary>
    public static Task<FirebaseUser> LinkAccount() {
      return SignIntoGooglePlayServices(
          (Credential credential, TaskCompletionSource<FirebaseUser> taskCompletionSource) => {
            FirebaseAuth.DefaultInstance.CurrentUser.LinkWithCredentialAsync(
              credential).ContinueWith(t => {
                if (!t.IsCompleted || t.IsCanceled) {
                  taskCompletionSource.SetCanceled();
                } else if (t.IsFaulted) {
                  taskCompletionSource.SetException(t.Exception);
                } else {
                  SignInState.SetState(SignInState.State.GooglePlayServices);
                  taskCompletionSource.SetResult(t.Result);
                }
            });
      });
    }

    /// <summary>
    /// Signs into Google Play Games account and Firebase (creating an account automatically
    /// if needed).
    /// </summary>
    /// <returns>The in.</returns>
    public static Task<FirebaseUser> SignIn()
    {
      return SignIntoGooglePlayServices(
        (Credential credential, TaskCompletionSource<FirebaseUser> taskCompletionSource) => {
          FirebaseAuth.DefaultInstance.SignInWithCredentialAsync(credential).ContinueWith(t => {
            if (!t.IsCompleted || t.IsCanceled) {
              taskCompletionSource.SetCanceled();
            } else if (t.IsFaulted) {
              taskCompletionSource.SetException(t.Exception);
            } else {
              SignInState.SetState(SignInState.State.GooglePlayServices);
              taskCompletionSource.SetResult(t.Result);
            }
          });
      });
    }

    /// <summary>
    /// Signs out of GPGS.
    /// </summary>
    public static void SignOut() {
      PlayGamesPlatform.Instance.SignOut();
    }

    /// <summary>
    /// Initializes the Google Play Games Client.
    /// </summary>
    public static void InitializeGooglePlayGames()
    {
      PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
        .RequestServerAuthCode(false /*forceRefresh*/)
        .Build();

      PlayGamesPlatform.InitializeInstance(config);
      PlayGamesPlatform.DebugLogEnabled = true;
      PlayGamesPlatform.Activate();
    }
  }
}

#else

using System;
using UnityEngine;
using System.Threading.Tasks;
using Firebase.Auth;

namespace Hamster
{
  /// <summary>
  /// Dummy implementation of GooglePlayServicesSignIn if Google Play Games Services is not enabled.
  /// </summary>
  public class GooglePlayServicesSignIn
  {
    private static Task<FirebaseUser> FailTask()
    {
      TaskCompletionSource<FirebaseUser> taskCompletionSource =
        new TaskCompletionSource<FirebaseUser>();

      taskCompletionSource.SetException(new AggregateException(
          new[] { new ApplicationException() }));

      return taskCompletionSource.Task;
    }

    public static bool CanAutoSignIn() {
      return false;
    }

    public static Task<FirebaseUser> LinkAccount() {
      return FailTask();
    }

    public static Task<FirebaseUser> SignIn() {
      return FailTask();
    }

    public static bool GooglePlayServicesEnabled() {
      return false;
    }

    public static void SignOut() {
    }

    public static void InitializeGooglePlayGames()
    {
    }
  }
}

#endif
