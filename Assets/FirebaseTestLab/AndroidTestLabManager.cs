#if UNITY_ANDROID

using System;
using UnityEngine;

namespace FirebaseTestLab {
  internal class AndroidTestLabManager : TestLabManager {
    public AndroidTestLabManager() {
      // get the UnityPlayerClass
      AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");

      // get the current activity
      _activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

      // get the intent from the activity
      _intent = _activity.Call<AndroidJavaObject>("getIntent");

      CheckManifestForScenario();
    }

    public override void NotifyHarnessTestIsComplete() {
      _activity.Call("finish");
    }

    private readonly AndroidJavaObject _activity;
    private readonly AndroidJavaObject _intent;

    private int _logFileDescriptor = -1;

    private void CheckManifestForScenario() {
      string action = _intent.Call<string>("getAction");
      if (action != "com.google.intent.action.TEST_LOOP") return;

      InitializeLogging();

      ScenarioNumber = _intent.Call<int>("getIntExtra", "scenario", NoScenarioPresent);
    }

    private void InitializeLogging() {
      // get the data from the intent, which is a Uri
      AndroidJavaObject logFileUri = _intent.Call<AndroidJavaObject>("getData");
      if (logFileUri == null) return;

      string encodedPath = logFileUri.Call<string>("getEncodedPath");
      Debug.Log("[FTL] Log file is located at: " + encodedPath);

      try {
        int fd = _activity
          .Call<AndroidJavaObject>("getContentResolver")
          .Call<AndroidJavaObject>("openAssetFileDescriptor", logFileUri, "w")
          .Call<AndroidJavaObject>("getParcelFileDescriptor")
          .Call<int>("getFd");
        Debug.Log("[FTL] extracted file descriptor from intent: " + fd);
        _logFileDescriptor = LibcWrapper.dup(fd);
        Debug.Log("[FTL] duplicated file descriptor: " + _logFileDescriptor);
        Debug.Log("[FTL] trying to write to fd: " + _logFileDescriptor);

        int bytesWritten = LibcWrapper.WriteToFileDescriptor(
          _logFileDescriptor,
          "This was a success result!"
        );

        Debug.Log("[FTL] wrote " + bytesWritten + " bytes to " + _logFileDescriptor);
      }
      catch (Exception e) {
        Debug.LogError("[FTL] - exception fetching log file descriptor: "
                       + e
                       + "\n"
                       + e.StackTrace);
      }
    }
  }
}

#endif // UNITY_ANDROID