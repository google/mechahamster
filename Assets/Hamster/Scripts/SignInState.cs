using System;
using UnityEngine;

namespace Hamster
{
  /// <summary>
  /// Manages the signed in state, keeping it in the User Preferences.
  /// </summary>
  public class SignInState {
    public enum State : int
    {
      Unknown = 0,
      SignedOut = 1,
      Email = 2,
      Anonymous = 3,
      GooglePlayServices = 4,
      GameCenter = 5
    }

    /// <summary>
    /// Loads the signed-in state from user preferences.
    /// </summary>
    /// <returns>Signed in state or Unknown if it was never set.</returns>
    public static SignInState.State GetState()
    {
      if (!PlayerPrefs.HasKey(StringConstants.SignInState))
      {
        return SignInState.State.Unknown;
      }

      return (SignInState.State)PlayerPrefs.GetInt(StringConstants.SignInState);
    }

    /// <summary>
    /// Stores the signed-in state in user preferences.
    /// </summary>
    public static void SetState(SignInState.State state) {
      PlayerPrefs.SetInt(StringConstants.SignInState, (int)state);
    }
  }
}
