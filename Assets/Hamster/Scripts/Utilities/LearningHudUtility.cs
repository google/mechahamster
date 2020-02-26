using UnityEngine;
using System.Collections.Generic;

namespace Hamster.Utilities {

	public enum FirebaseFeatureEnum: int {
		CloudFirestore = 0,
		CloudFunctionsForFirebase = 1,
		CloudStorageForFirebase = 2,
		FirebaseAbTesting = 3,
		FirebaseAppDistribution = 4,
		FirebaseAppIndexing = 5,
		FirebaseAuthentication = 6,
		FirebaseCloudMessaging = 7,
		FirebaseCrashReporting = 8,
		FirebaseCrashlytics = 9,
		FirebaseDynamicLinks = 10,
		FirebaseExtensions = 11,
		FirebaseHosting = 12,
		FirebaseInAppMessaging = 13,
		FirebaseInvites = 14,
		FirebasePerformanceMonitoring = 15,
		FirebasePredictions = 16,
		FirebaseRealtimeDatabase = 17,
		FirebaseRemoteConfig = 18,
		FirebaseTestLab = 19,
		GoogleAnalytics = 20,
		MlKitForFirebase = 21
	}

  public class FeatureData {
  	public FirebaseFeatureEnum feature;
		public string desc;
		public string codeUrl;
		public string gettingStartedUrl;
		public Rect iconRect;

		public FeatureData(
        FirebaseFeatureEnum featureEnum, string descArg, string codeUrlArg, string gettingStartedUrlArg, Rect rectArg = new Rect()) {
			feature = featureEnum;
			desc = descArg;
			codeUrl = codeUrlArg;
			gettingStartedUrl = gettingStartedUrlArg;
			iconRect = rectArg;
		}
  }

	public class LearningHudUtility {
	  // Map from enum to icon texture.
	  static Dictionary<FirebaseFeatureEnum, Texture2D> featureTexMap = new Dictionary<FirebaseFeatureEnum, Texture2D>() {
			{ FirebaseFeatureEnum.CloudFirestore, Resources.Load<Texture2D>("FirebaseIcons/cloud_firestore_icon")},
			{ FirebaseFeatureEnum.CloudFunctionsForFirebase, Resources.Load<Texture2D>("FirebaseIcons/cloud_functions_for_firebase_icon")},
			{ FirebaseFeatureEnum.CloudStorageForFirebase, Resources.Load<Texture2D>("FirebaseIcons/cloud_storage_for_firebase_icon")},
			{ FirebaseFeatureEnum.FirebaseAbTesting, Resources.Load<Texture2D>("FirebaseIcons/firebase_ab_testing_icon")},
			{ FirebaseFeatureEnum.FirebaseAppDistribution, Resources.Load<Texture2D>("FirebaseIcons/firebase_app_distribution_icon")},
			{ FirebaseFeatureEnum.FirebaseAppIndexing, Resources.Load<Texture2D>("FirebaseIcons/firebase_app_indexing_icon")},
			{ FirebaseFeatureEnum.FirebaseAuthentication, Resources.Load<Texture2D>("FirebaseIcons/firebase_authentication_icon")},
			{ FirebaseFeatureEnum.FirebaseCloudMessaging, Resources.Load<Texture2D>("FirebaseIcons/firebase_cloud_messaging_icon")},
			{ FirebaseFeatureEnum.FirebaseCrashReporting, Resources.Load<Texture2D>("FirebaseIcons/firebase_crash_reporting_icon")},
			{ FirebaseFeatureEnum.FirebaseCrashlytics, Resources.Load<Texture2D>("FirebaseIcons/firebase_crashlytics_icon")},
			{ FirebaseFeatureEnum.FirebaseDynamicLinks, Resources.Load<Texture2D>("FirebaseIcons/firebase_dynamic_links_icon")},
			{ FirebaseFeatureEnum.FirebaseExtensions, Resources.Load<Texture2D>("FirebaseIcons/firebase_extensions_icon")},
			{ FirebaseFeatureEnum.FirebaseHosting, Resources.Load<Texture2D>("FirebaseIcons/firebase_hosting_icon")},
			{ FirebaseFeatureEnum.FirebaseInAppMessaging, Resources.Load<Texture2D>("FirebaseIcons/firebase_in-app_messaging_icon")},
			{ FirebaseFeatureEnum.FirebaseInvites, Resources.Load<Texture2D>("FirebaseIcons/firebase_invites_icon")},
			{ FirebaseFeatureEnum.FirebasePerformanceMonitoring, Resources.Load<Texture2D>("FirebaseIcons/firebase_performance_monitoring_icon")},
			{ FirebaseFeatureEnum.FirebasePredictions, Resources.Load<Texture2D>("FirebaseIcons/firebase_predictions_icon")},
			{ FirebaseFeatureEnum.FirebaseRealtimeDatabase, Resources.Load<Texture2D>("FirebaseIcons/firebase_realtime_database_icon")},
			{ FirebaseFeatureEnum.FirebaseRemoteConfig, Resources.Load<Texture2D>("FirebaseIcons/firebase_remote_config_icon")},
			{ FirebaseFeatureEnum.FirebaseTestLab, Resources.Load<Texture2D>("FirebaseIcons/firebase_test_lab_icon")},
			{ FirebaseFeatureEnum.GoogleAnalytics, Resources.Load<Texture2D>("FirebaseIcons/google_analytics_icon")},
			{ FirebaseFeatureEnum.MlKitForFirebase, Resources.Load<Texture2D>("FirebaseIcons/ml_kit_for_firebase_icon")}
	  };
    // TODO: Add a static Dictionary to map from enums to "Getting started" urls. Then the gettingStartedUrl argument
    // in AddDialogButton can be removed.

		static bool enabled = false;

    // Enable/Disable Learning mode
    public static void EnableLearningMode(bool enable) {
      LearningHudUtility.enabled = enable;
    }
    
    // Show/Hide Learning mode HUD
    // TODO: Use gestures to temporarily show or hide learning mode.
    public static void ShowHud(bool show) {}

		// A lookup table from game objects (e.g. level buttons) to the firebase feature data attached to them.
		private Dictionary<GameObject, List<FeatureData>> objectFeatureDataMap = new Dictionary<GameObject, List<FeatureData>>();
    // A dummy game object for icons that are given coordinates, not attached to any actual game object.
    private GameObject dummyObject = new GameObject();
    // Whether to show an info window.
		private bool shouldShowWindow = false;
    // The data that is clicked and displayed in the info window.
		private FeatureData clickedFeatureData;
    // The width and height of an icon GUI button.
		private const float iconSize = 40;

    // Set current debug info
    public void SetDebugInfo(string info) {
      Debug.Log(info);
    }

    // Add one Firebase feature related to a game object in the current state.
    public void AddDialogButton(
		    GameObject targetObject, FirebaseFeatureEnum featureEnum, string desc, string codeUrl, string gettingStartedUrl = "") {
			if (!objectFeatureDataMap.ContainsKey(targetObject)) {
				List<FeatureData> dataList = new List<FeatureData>();
				objectFeatureDataMap[targetObject] = dataList;
			}

      FeatureData newData = new FeatureData(featureEnum, desc, codeUrl, gettingStartedUrl);
			objectFeatureDataMap[targetObject].Add(newData);
    }

    // Add one Firebase feature in the current state at a given position.
    public void AddDialogButton(
		    Vector2 pos, FirebaseFeatureEnum featureEnum, string desc, string codeUrl, string gettingStartedUrl = "") {
      // The feature is not attached to a specific game object so use the dummy.
			if (!objectFeatureDataMap.ContainsKey(dummyObject)) {
				List<FeatureData> dataList = new List<FeatureData>();
				objectFeatureDataMap[dummyObject] = dataList;
			}
      FeatureData newData = new FeatureData(
        featureEnum, desc, codeUrl, gettingStartedUrl, new Rect(pos.x, pos.y, iconSize, iconSize));
			objectFeatureDataMap[dummyObject].Add(newData);
    }

    // Draw all GUI during OnGUI()
    public void RenderGUI() {
      if (!LearningHudUtility.enabled)
        return;

      // Render all feature icon buttons.
			RenderFeatureButtons();

      // Render a window if necessary.
			if (shouldShowWindow) {
				Rect hudWindowPos = OpenHudWindow();
				Event e = Event.current;
        // Close the window when a mouse clicks outside.
				if (e.type == EventType.MouseDown && !hudWindowPos.Contains(e.mousePosition)) {
					shouldShowWindow = false;
				}
			}
    }

    // Render all feature icons.
		private void RenderFeatureButtons() {
			foreach (GameObject gameButton in objectFeatureDataMap.Keys) {
        List<FeatureData> dataList = objectFeatureDataMap[gameButton];
  			for (int i = 0; i < dataList.Count; i++) {
          FeatureData data = objectFeatureDataMap[gameButton][i];

  				// Cache the icon rect if it has not been calculated.
          if (data.iconRect.width == 0) {
            ComputeFeatureButtonPosition(gameButton, i);
          }
  				if (GUI.Button(
                data.iconRect,
                LearningHudUtility.featureTexMap[data.feature],
                GUIStyle.none)) {
            shouldShowWindow = true;
  					clickedFeatureData = data;
  				}
  			}
      }
		}

    // Compute the position of the i-th feature icon attached to a game object.
		private void ComputeFeatureButtonPosition(GameObject gameButton, int i) {
      // Get the corners coordinates of the game object in world space.
      RectTransform objectTransform = gameButton.GetComponent<RectTransform>();
      Vector3[] worldPts = new Vector3[4];
      objectTransform.GetWorldCorners(worldPts);

      // Get the corners coordinates on screen.
      Camera cam = CommonData.mainCamera.GetComponentInChildren<Camera>();
      Vector3 scrPt2 = cam.WorldToScreenPoint(worldPts[2]);
      Vector3 scrPt3 = cam.WorldToScreenPoint(worldPts[3]);

      // Position the icon to the right the game object, offset by any previous feature icons.
      float iconLeft = Mathf.Round(scrPt2[0]) + iconSize * i;
      // Vertically align the icon with the game object.
      float scrCenterY = Mathf.Round((scrPt2[1] + scrPt3[1]) / 2f);
      float iconTop = Screen.height - (scrCenterY + iconSize / 2f);

      objectFeatureDataMap[gameButton][i].iconRect.Set(iconLeft, iconTop, iconSize, iconSize);
		}

    // Open an info window to show the clicked data.
    // TODO: Prevent mouse events from propagating through the GUI window and reaching game objects underneath.
		private Rect OpenHudWindow() {
			return GUI.Window(0, new Rect(0, 0, Screen.width / 2f, Screen.height), DrawHudWindow, clickedFeatureData.desc);
	  }

    // Draw the content of the info window displaying the clicked data.
    // TODO: Polish the window.
		private void DrawHudWindow(int windowID) {
			if (GUI.Button(new Rect(10, 20, 180, 20), "Code"))	{
				Application.OpenURL(clickedFeatureData.codeUrl);
			}
			if (clickedFeatureData.gettingStartedUrl.Length != 0) {
				if (GUI.Button(new Rect(10, 50, 180, 20), "Getting Started")) {
					Application.OpenURL(clickedFeatureData.gettingStartedUrl);
				}
			}
		}
  }
}