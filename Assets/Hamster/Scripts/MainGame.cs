using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Firebase.Unity.Editor;

namespace Hamster {

  public class MainGame : MonoBehaviour {

    private States.StateManager stateManager = new States.StateManager();
    private float currentFrameTime, lastFrameTime;

    private const string kPlayerLookupID = "Player";

    public GameObject player;

    // More placeholders, will be swapped out for real data once
    // auth is hooked up.
    const string kUserID = "XYZZY";

    public DBStruct<UserData> currentUser;

    void Start() {
      InitializeFirebaseAndStart();
    }

    void Update() {
      lastFrameTime = currentFrameTime;
      currentFrameTime = Time.realtimeSinceStartup;
      stateManager.Update();
    }

    // Utility function to check the time since the last update.
    // Needed, since we can't use Time.deltaTime, as we are adjusting the
    // simulation timestep.  (Setting it to 0 to pause the world.)
    public float TimeSinceLastUpdate {
      get { return currentFrameTime - lastFrameTime; }
    }

    // Utility function to check if the game is currently running.  (i.e.
    // not in edit mode.)
    public bool isGameRunning() {
      return (stateManager.CurrentState() is States.Gameplay);
    }

    // Utility function for spawning the player.
    public GameObject SpawnPlayer() {
      if (player == null) {
        player = (GameObject)Instantiate(CommonData.prefabs.lookup[kPlayerLookupID].prefab);
      }
      return player;
    }

    // Utility function for despawning the player.
    public void DestroyPlayer() {
      if (player != null) {
        Destroy(player);
        player = null;
      }
    }

    // Pass through to allow states to have their own GUI.
    void OnGUI() {
      stateManager.OnGUI();
    }

    void InitializeAnalytics() {
      Firebase.Analytics.FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);

      // Set the user's sign up method.
      Firebase.Analytics.FirebaseAnalytics.SetUserProperty(
        Firebase.Analytics.FirebaseAnalytics.UserPropertySignUpMethod,
        "Google");

      // TODO(ccornell): replace this with a real user token
      // once Auth gets hooked up.
      // Set the user ID.
      Firebase.Analytics.FirebaseAnalytics.SetUserId("desktop_user");
    }

    // Sets the default values for remote config.  These are the values that will
    // be used if we haven't fetched yet.
    System.Threading.Tasks.Task InitializeRemoteConfig() {
      Dictionary<string, object> defaults = new Dictionary<string, object>();

      // Physics defaults:
      defaults.Add(StringConstants.RemoteConfigPhysicsGravity, -20.0f);

      // Invites defaults:
      defaults.Add(StringConstants.RemoteConfigInviteTitleText,
          StringConstants.DefaultInviteTitleText);
      defaults.Add(StringConstants.RemoteConfigInviteMessageText,
          StringConstants.DefaultInviteMessageText);
      defaults.Add(StringConstants.RemoteConfigInviteCallToActionText,
          StringConstants.DefaultInviteCallToActionText);

      Firebase.RemoteConfig.FirebaseRemoteConfig.SetDefaults(defaults);
      return Firebase.RemoteConfig.FirebaseRemoteConfig.FetchAsync(System.TimeSpan.Zero);
    }

    // When the app starts, check to make sure that we have
    // the required dependencies to use Firebase, and if not,
    // add them if possible.
    void InitializeFirebaseAndStart() {
      Firebase.DependencyStatus dependencyStatus = Firebase.FirebaseApp.CheckDependencies();

      if (dependencyStatus != Firebase.DependencyStatus.Available) {
        Firebase.FirebaseApp.FixDependenciesAsync().ContinueWith(task => {
          dependencyStatus = Firebase.FirebaseApp.CheckDependencies();
          if (dependencyStatus == Firebase.DependencyStatus.Available) {
            InitializeFirebaseComponents();
          } else {
            Debug.LogError(
                "Could not resolve all Firebase dependencies: " + dependencyStatus);
            Application.Quit();
          }
        });
      } else {
        InitializeFirebaseComponents();
      }
    }

    void InitializeFirebaseComponents() {
      InitializeAnalytics();

      System.Threading.Tasks.Task.WhenAll(
          InitializeRemoteConfig()
        ).ContinueWith(task => { StartGame(); });
    }

    // Actually start the game, once we've verified that everything
    // is working and we have the firebase prerequisites ready to go.
    void StartGame() {
      // Remote Config data has been fetched, so this applies it for this play session:
      Firebase.RemoteConfig.FirebaseRemoteConfig.ActivateFetched();

      CommonData.prefabs = FindObjectOfType<PrefabList>();
      CommonData.mainCamera = FindObjectOfType<Camera>();
      CommonData.mainGame = this;
      Firebase.AppOptions ops = new Firebase.AppOptions();
      CommonData.app = Firebase.FirebaseApp.Create(ops);
      CommonData.app.SetEditorDatabaseUrl("https://hamster-demo.firebaseio.com/");

      Screen.orientation = ScreenOrientation.Landscape;

      CommonData.gameWorld = FindObjectOfType<GameWorld>();
      currentUser = new DBStruct<UserData>("user", CommonData.app);

      stateManager.PushState(new States.Startup(kUserID));
    }
  }
}
