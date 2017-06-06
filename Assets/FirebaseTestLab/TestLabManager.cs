// currently this is FirebaseTestLab, but should be Firebase.TestLab.
// the .gitignore is ignoring all Firebase/ paths, so until this is more official,
// it'll have this odd namespace

namespace FirebaseTestLab {
  // This class serves as the entry point for the Firebase Test Lab, described in detail at
  // https://firebase.google.com/docs/test-lab/game-loop
  // It will check to see if the application was launched with the intent to test, and handle
  // the parameters that were passed in.
  // Note this is only supported at the moment on Android platforms.
  public abstract class TestLabManager {
    public const int NoScenarioPresent = -1;
    public int ScenarioNumber = NoScenarioPresent;

    public bool IsTestingScenario {
      get { return ScenarioNumber > NoScenarioPresent; }
    }

    // Notify the harness that the testing scenario is complete.  This will cause the app to quit
    public abstract void NotifyHarnessTestIsComplete();

    public static TestLabManager Instantiate() {
#if UNITY_ANDROID
      return new AndroidTestLabManager();
#else
      return new DummyTestLabManager();
#endif // UNITY_ANDROID
    }
  }
}