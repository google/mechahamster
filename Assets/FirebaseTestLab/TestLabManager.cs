// Copyright 2017 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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
#if UNITY_ANDROID && !UNITY_EDITOR
      return new AndroidTestLabManager();
#else
      return new DummyTestLabManager();
#endif // UNITY_ANDROID
    }
  }
}