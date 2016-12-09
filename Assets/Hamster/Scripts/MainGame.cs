using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Firebase.Unity.Editor;

namespace Hamster {

  public class MainGame : MonoBehaviour {

    private States.StateManager stateManager = new States.StateManager();
    private float currentFrameTime, lastFrameTime;

    public float TimeSinceLastUpdate {
      get { return currentFrameTime - lastFrameTime; }
    }

    void Start() {
      CommonData.prefabs = FindObjectOfType<PrefabList>();
      CommonData.mainCamera = FindObjectOfType<Camera>();

      Screen.orientation = ScreenOrientation.Landscape;

      Firebase.AppOptions ops = new Firebase.AppOptions();
      CommonData.app = Firebase.FirebaseApp.Create(ops);
      CommonData.app.SetEditorDatabaseUrl("https://hamster-demo.firebaseio.com/");

      CommonData.gameWorld = FindObjectOfType<GameWorld>();
      stateManager.PushState(new States.Editor());
    }

    void Update() {
      lastFrameTime = currentFrameTime;
      currentFrameTime = Time.realtimeSinceStartup;
      stateManager.Update();
    }

    public bool isGameRunning() {
      return (stateManager.CurrentState() is States.Gameplay);
    }

    void OnGUI() {
      stateManager.OnGUI();
    }
  }
}
