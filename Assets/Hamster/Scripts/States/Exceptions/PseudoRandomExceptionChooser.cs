using System;
using System.Security.Cryptography;
using Firebase.Auth;
using Firebase.Crashlytics;
using UnityEngine;
using Random = System.Random;

namespace Hamster.States {
  /// <summary>
  /// This utility is meant to throw a random type of exception
  /// to generate a few different types of issues in Crashlytics.
  /// Return a pseudo random exception type from the choices
  /// available. The caller of this utility is responsible
  /// for instantiating/throwing the exception.
  /// </summary>
  public static class PseudoRandomExceptionChooser {
    private static readonly Random RandomGenerator;
    static PseudoRandomExceptionChooser() {
      RandomGenerator = new Random();
    }

    /// <summary>
    /// Throw a random exception from the choices in this directory. Demonstrate
    /// a different set of functions based on which exception is chosen.
    /// </summary>
    /// <param name="message"></param>
    public static void Throw(String message) {
      if (FirebaseAuth.DefaultInstance.CurrentUser != null) {
        Crashlytics.SetUserId(FirebaseAuth.DefaultInstance.CurrentUser.UserId);
      }

      int exceptionIndex = RandomGenerator.Next(0, 6);

      switch (exceptionIndex) {
        case 0:
          Crashlytics.Log("Menu meltdown is imminent.");
          ThrowMenuMeltdown(message);
          break;
        case 1:
          Crashlytics.Log("User triggered another forced exception.");
          ThrowAnotherForcedException(message);
          break;
        case 2:
          Crashlytics.Log("User triggered an intentionally obscure exception.");
          ThrowIntentionallyObscureException();
          break;
        case 3:
          Crashlytics.Log("User triggered a random text exception.");
          ThrowRandomTextException(message);
          break;
        case 4:
          Crashlytics.Log("User triggered an equally statistically likely exception.");
          ThrowStatisticallyAsLikelyException(message);
          break;
        default:
          Crashlytics.Log(String.Format("Could not find index {0} - using default meltdown exception", exceptionIndex));
          ThrowMenuMeltdown(message);
          break;
      }
    }

    private static void ThrowMenuMeltdown(String message) {
      try {
        throw new MenuMeltdownException(message);
      }
      catch (CrashlyticsCaughtException e) {
        Crashlytics.LogException(e);
      }
    }

    private static void ThrowAnotherForcedException(String message) {
      try {
        throw new AnotherForcedException(message);
      }
      catch (CrashlyticsCaughtException e) {
        Crashlytics.LogException(e);
      }
    }

    private static void ThrowIntentionallyObscureException() {
      try {
        throw new IntentionallyObscureException("An error occurred.");
      }
      catch (CrashlyticsCaughtException e) {
        Crashlytics.LogException(e);
      }
    }

    private static void ThrowRandomTextException(String message) {
      Crashlytics.SetCustomKey("guid", Guid.NewGuid().ToString());
      try {
        throw new RandomTextException(message);
      }
      catch (CrashlyticsCaughtException e) {
        Crashlytics.LogException(e);
      }
    }

    private static void ThrowStatisticallyAsLikelyException(String message) {
      try {
        throw new StatisticallyAsLikelyException(message);
      }
      catch (CrashlyticsCaughtException e) {
        Crashlytics.LogException(e);
      }
    }


  }
}