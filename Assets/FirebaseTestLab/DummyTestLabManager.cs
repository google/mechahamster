namespace FirebaseTestLab {
  // Dummy class to handle non-Android platform.  This class is instantiated instead to avoid having
  // to wrap code in #if blocks.
  internal class DummyTestLabManager : TestLabManager {
    public override void NotifyHarnessTestIsComplete() {
      // do nothing!
    }
  }
}