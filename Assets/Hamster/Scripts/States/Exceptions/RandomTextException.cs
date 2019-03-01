using System;

namespace Hamster.States {
  public class RandomTextException : CrashlyticsCaughtException {
    public RandomTextException()
    {
    }

    public RandomTextException(string message)
      : base(message)
    {
    }

    public RandomTextException(string message, Exception inner)
      : base(message, inner)
    {
    }
  }
}