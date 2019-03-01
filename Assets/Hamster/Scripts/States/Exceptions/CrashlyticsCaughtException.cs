using System;

namespace Hamster.States {
  /// <summary>
  /// An empty Exception class inheritting from Exception so
  /// that we can catch a more focused set of exceptions.
  /// </summary>
  public class CrashlyticsCaughtException : Exception {
    public CrashlyticsCaughtException()
    {
    }

    public CrashlyticsCaughtException(string message)
      : base(message)
    {
    }

    public CrashlyticsCaughtException(string message, Exception inner)
      : base(message, inner)
    {
    }
  }
}