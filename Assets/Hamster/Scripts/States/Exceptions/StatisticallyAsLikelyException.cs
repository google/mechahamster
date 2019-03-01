using System;

namespace Hamster.States {
  public class StatisticallyAsLikelyException : CrashlyticsCaughtException {
    public StatisticallyAsLikelyException()
    {
    }

    public StatisticallyAsLikelyException(string message)
      : base(message)
    {
    }

    public StatisticallyAsLikelyException(string message, Exception inner)
      : base(message, inner)
    {
    }
  }
}