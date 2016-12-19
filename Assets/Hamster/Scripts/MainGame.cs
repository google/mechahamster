using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Firebase.Unity.Editor;

namespace Hamster {

  public class MainGame : MonoBehaviour {

    private States.StateManager stateManager = new States.StateManager();
    private float currentFrameTime, lastFrameTime;

    private const string kPlayerLookupID = "Player";
    DBTable<UserData> userTable;

    public GameObject player;

    public DBStruct<UserData> currentUser;

    void Start() {
      CommonData.prefabs = FindObjectOfType<PrefabList>();
      CommonData.mainCamera = FindObjectOfType<Camera>();
      CommonData.mainGame = this;
      Firebase.AppOptions ops = new Firebase.AppOptions();
      CommonData.app = Firebase.FirebaseApp.Create(ops);
      CommonData.app.SetEditorDatabaseUrl("https://hamster-demo.firebaseio.com/");

      Screen.orientation = ScreenOrientation.Landscape;

      userTable = new DBTable<UserData>(CommonData.kDBUserTablePath, CommonData.app);
      UserData temp = new UserData();
      //  Temporary login credentials, to be replaced with Auth.
      temp.name = "Ico the Corgi";
      temp.id = "XYZZY";
      string key = "<<TEMP KEY>>";
      userTable.Add(key, temp);
      userTable.PushData();

      CommonData.gameWorld = FindObjectOfType<GameWorld>();
      stateManager.PushState(new States.Editor());

      currentUser = new DBStruct<UserData>("user", CommonData.app);
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
