using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Firebase.Unity.Editor;

namespace Hamster {

  public class MainGame : MonoBehaviour {

    Camera mainCamera;

    Firebase.FirebaseApp app;
    DBTable<LevelMap> mapTable;
    List<GameObject> activeGameObjects;

    GameMode gameMode = GameMode.Gameplay;

    public enum GameMode {
      // On the main menu of the game.
      MainMenu,
      // Playing the game.
      Gameplay,
      // In the level editor.
      Editor,
      // Playing the game through the editor.  (Rather than through the main game.)
      EditorPlaytest,
      // Waiting for a level to load.
      WaitingForLoad
    };

    Vector2 scrollViewPosition;
    int mapToolSelection = 0;

    void Start() {
      mainCamera = FindObjectOfType<Camera>();
      CommonData.prefabs = FindObjectOfType<PrefabList>();

      Screen.orientation = ScreenOrientation.Landscape;

      Firebase.AppOptions ops = new Firebase.AppOptions();
      app = Firebase.FirebaseApp.Create(ops);
      app.SetEditorDatabaseUrl("https://hamster-demo.firebaseio.com/");

      mapTable = new DBTable<LevelMap>("MapList", app);
      CommonData.gameWorld = FindObjectOfType<GameWorld>();
      SetGameMode(GameMode.Editor);
    }


    void Update() {
      if (gameMode == GameMode.Editor) {
        LevelEditorUpdate();
      }
      if (gameMode == GameMode.EditorPlaytest) {
        if (Input.GetKeyDown(KeyCode.Escape)) {
          SetGameMode(GameMode.Editor);
        }
      }
      if (gameMode == GameMode.WaitingForLoad) {
        if (mapTable.areChangesPending) {
          mapTable.ApplyRemoteChanges();
          if (mapTable.data.ContainsKey("test_map")) {
            LevelMap worldMap = mapTable.data["test_map"].data;
            CommonData.gameWorld.DisposeWorld();
            CommonData.gameWorld.SpawnWorld(worldMap);
            gameMode = GameMode.Editor;
          }
        }
      }
    }

    void LevelEditorUpdate() {
      if (Input.GetMouseButton(0) && GUIUtility.hotControl == 0) {
        string brushElementType = CommonData.prefabs.prefabNames[mapToolSelection];
        float rayDist;
        Ray cameraRay = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (CommonData.kZeroPlane.Raycast(cameraRay, out rayDist)) {
          MapElement element = new MapElement();
          Vector3 pos = cameraRay.GetPoint(rayDist);
          pos.x = Mathf.RoundToInt(pos.x);
          pos.y = Mathf.RoundToInt(pos.y);
          pos.z = Mathf.RoundToInt(pos.z);
          element.position = pos;
          element.type = brushElementType;

          CommonData.gameWorld.PlaceTile(element);
        }
      }
    }

    // Sets the current mode the game is in.
    void SetGameMode(GameMode newMode) {
      gameMode = newMode;
      switch (newMode) {
        case GameMode.Editor:
          var player = FindObjectOfType<PlayerController>();
          if (player != null) player.ResetToInitialPosition();
          Time.timeScale = 0.0f;
          break;
        case GameMode.EditorPlaytest:
          Time.timeScale = 1.0f;
          break;
        case GameMode.MainMenu:
          Time.timeScale = 0.0f;
          break;
        case GameMode.Gameplay:
          Time.timeScale = 1.0f;
          break;
      }
    }

    public bool isGameRunning() {
      return gameMode == GameMode.EditorPlaytest || gameMode == GameMode.Gameplay;
    }

    const string kButtonNameSave = "Save";
    const string kButtonNameLoad = "Load";
    const string kButtonNameClear = "Clear";
    const string kButtonNamePlay = "Play";

    const string kDBMapTablePath = "MapList";

    void OnGUI() {
      GUI.skin = CommonData.prefabs.guiSkin;
      GUILayout.BeginHorizontal();
      switch (gameMode) {
        case GameMode.Editor:
          scrollViewPosition = GUILayout.BeginScrollView(scrollViewPosition);

          mapToolSelection = GUILayout.SelectionGrid(
              mapToolSelection, CommonData.prefabs.prefabNames, 1);

          GUILayout.EndScrollView();

          if (GUILayout.Button(kButtonNameSave)) {
            if (mapTable.data.ContainsKey("test_map")) mapTable.data.Remove("test_map");
            mapTable.Add("test_map", CommonData.gameWorld.worldMap);
            mapTable.PushData();
          }
          if (GUILayout.Button(kButtonNameLoad)) {
            CommonData.gameWorld.DisposeWorld();
            mapTable = new DBTable<LevelMap>(kDBMapTablePath, app);
            gameMode = GameMode.WaitingForLoad;
          }
          if (GUILayout.Button(kButtonNameClear)) {
            CommonData.gameWorld.DisposeWorld();
          }
          if (GUILayout.Button(kButtonNamePlay)) {
            SetGameMode(GameMode.EditorPlaytest);
          }
          break;
        case GameMode.EditorPlaytest:
          break;
        case GameMode.Gameplay:
          break;
        case GameMode.MainMenu:
          break;
      }
      GUILayout.EndHorizontal();
    }
  }
}
