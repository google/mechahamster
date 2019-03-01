using System;

namespace Hamster.States {
  public class IntentionallyObscureException : CrashlyticsCaughtException {
    public IntentionallyObscureException()
    {
    }

    public IntentionallyObscureException(string message)
      : base(message)
    {
    }

    public IntentionallyObscureException(string message, Exception inner)
      : base(message, inner)
    {
    }
  }
}