using System;

namespace Hamster.States {
  public class AnotherForcedException : CrashlyticsCaughtException {
    public AnotherForcedException()
    {
    }

    public AnotherForcedException(string message)
      : base(message)
    {
    }

    public AnotherForcedException(string message, Exception inner)
      : base(message, inner)
    {
    }
  }
}