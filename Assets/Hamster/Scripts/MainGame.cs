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
      CommonData.prefabs = FindObjectOfType<PrefabList>();
      CommonData.mainCamera = FindObjectOfType<Camera>();
      CommonData.mainGame = this;
      Firebase.AppOptions ops = new Firebase.AppOptions();
      CommonData.app = Firebase.FirebaseApp.Create(ops);
      CommonData.app.SetEditorDatabaseUrl("https://hamster-demo.firebaseio.com/");

      Screen.orientation = ScreenOrientation.Landscape;

      CommonData.gameWorld = FindObjectOfType<GameWorld>();
      currentUser = new DBStruct<UserData>("user", CommonData.app);
      stateManager.PushState(new States.MainMenu());

      // When the game starts up, it needs to either download the user data
      // or create a new profile.
      stateManager.PushState(new States.FetchUserData(kUserID));
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

    void OnGUI() {
      stateManager.OnGUI();
    }
  }
}
