using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Firebase.Unity.Editor;

public class MainGame : MonoBehaviour {

  Camera mainCamera;
  PlayerController player;

  Firebase.FirebaseApp app;
  DBTable<LevelMap> mapTable;
  List<GameObject> activeGameObjects;
  PrefabList prefabs;

  LevelMap worldMap;
  Dictionary<string, GameObject> screenObjects = new Dictionary<string, GameObject>();

  public static Vector3 kGravity = new Vector3(0, -20, 0);

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
  static string[] mapToolList = new string[] {
    "Empty",
    "Wall",
    "Floor",
    "JumpPad",
    "Start",
    "Goal"
  };

  void Start() {
    worldMap = new LevelMap();
    SetGameMode(GameMode.Editor);
    player = FindObjectOfType<PlayerController>();
    mainCamera = FindObjectOfType<Camera>();
    prefabs = FindObjectOfType<PrefabList>();

    Physics.gravity = kGravity;
    Screen.orientation = ScreenOrientation.Landscape;

    Firebase.AppOptions ops = new Firebase.AppOptions();
    app = Firebase.FirebaseApp.Create(ops);
    app.SetEditorDatabaseUrl("https://hamster-demo.firebaseio.com/");

    mapTable = new DBTable<LevelMap>("MapList", app);
  }

  // Iterates through a map and spawns all the objects in it.
  void SpawnWorld(LevelMap map) {
    foreach (MapElement element in map.elements.Values) {
      GameObject obj = SpawnObject(element);
      obj.transform.localScale = element.scale;
    }
  }

  // Spawns a single object in the world, based on a map element.
  GameObject SpawnObject(MapElement element) {
    GameObject obj = null;
    switch (element.type) {
      case MapElement.MapElementType.Empty:
        break;
      case MapElement.MapElementType.Wall:
        obj = (GameObject)(Instantiate(prefabs.wall, element.position, element.rotation));
        break;
      case MapElement.MapElementType.Floor:
        obj = (GameObject)(Instantiate(prefabs.floor, element.position, element.rotation));
        break;
      case MapElement.MapElementType.StartPosition:
        obj = (GameObject)(Instantiate(prefabs.startPos, element.position, element.rotation));
        break;
      case MapElement.MapElementType.JumpPad:
        obj = (GameObject)(Instantiate(prefabs.jumpTile, element.position, element.rotation));
        break;
      case MapElement.MapElementType.Goal:
        obj = (GameObject)(Instantiate(prefabs.goal, element.position, element.rotation));
        break;
      default:
        Debug.LogError("Spawning objects - encountered unknown element type:" + element.type);
        break;
    }
    if (obj != null) {
      screenObjects.Add(GetKeyForElement(element), obj);
    }
    return obj;
  }

  void DisposeWorld() {
    foreach (GameObject obj in screenObjects.Values) {
      Destroy(obj);
    }
    screenObjects.Clear();
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
          worldMap = mapTable.data["test_map"].data;
          DisposeWorld();
          SpawnWorld(worldMap);
          gameMode = GameMode.Editor;
        }
      }
    }
  }

  // Takes a map element, and returns the string key that will represent
  // it in the database.  For most objects, this is a function of their
  // coordinates in world-space.  (Because only one object is allowed
  // to be at a given coordinate.)  For unique objects though, (such as
  // start locations) they have a different name, to enforce that they
  // are once-per-map.
  string GetKeyForElement(MapElement ele) {
    if (ele.type == MapElement.MapElementType.StartPosition)
      return "StartPos";
    else
      return "obj_" + ele.position.ToString();
  }


  static Plane kZeroPlane = new Plane(new Vector3(0, 1, 0), new Vector3(0, 0, 0));

  void LevelEditorUpdate() {
    if (Input.GetMouseButton(0) && GUIUtility.hotControl == 0) {
      MapElement.MapElementType brushElementType = (MapElement.MapElementType)mapToolSelection;

      float rayDist;
      Ray cameraRay = mainCamera.ScreenPointToRay(Input.mousePosition);
      if (kZeroPlane.Raycast(cameraRay, out rayDist)) {
        MapElement element = new MapElement();
        Vector3 pos = cameraRay.GetPoint(rayDist);
        pos.x = Mathf.RoundToInt(pos.x);
        pos.y = Mathf.RoundToInt(pos.y);
        pos.z = Mathf.RoundToInt(pos.z);
        element.position = pos;
        element.type = brushElementType;

        string key = GetKeyForElement(element);

        // TODO(ccornell): Expand this, if there are things beyond the start position that
        // need to be unique per map.
        bool isUnique = (brushElementType == MapElement.MapElementType.StartPosition);

        // If we're placing a unique object, or we're placing an object over an existing object,
        // then remove the old one that is being replaced.
        if (worldMap.elements.ContainsKey(key) &&
            (worldMap.elements[key].type != brushElementType || isUnique)) {
          worldMap.elements.Remove(key);
          Destroy(screenObjects[key]);
          screenObjects.Remove(key);
        }

        // Add the new object.  (Or add nothing, if we're "painting" with empty blocks.)
        if (!worldMap.elements.ContainsKey(key) && brushElementType != MapElement.MapElementType.Empty) {
          SpawnObject(element);
          worldMap.elements.Add(key, element);
        }
      }
    }
  }

  // Sets the current mode the game is in.
  void SetGameMode(GameMode newMode) {
    gameMode = newMode;
    switch (newMode) {
      case GameMode.Editor:
        DisposeWorld();
        SpawnWorld(worldMap);
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
    GUI.skin = prefabs.guiSkin;
    GUILayout.BeginHorizontal();
    switch (gameMode) {
      case GameMode.Editor:
        scrollViewPosition = GUILayout.BeginScrollView(scrollViewPosition);

        mapToolSelection = GUILayout.SelectionGrid(mapToolSelection, mapToolList, 1);

        GUILayout.EndScrollView();

        if (GUILayout.Button(kButtonNameSave)) {
          if (mapTable.data.ContainsKey("test_map")) mapTable.data.Remove("test_map");
          mapTable.Add("test_map", worldMap);
          mapTable.PushData();
        }
        if (GUILayout.Button(kButtonNameLoad)) {
          DisposeWorld();
          worldMap.elements.Clear();
          mapTable = new DBTable<LevelMap>(kDBMapTablePath, app);
          gameMode = GameMode.WaitingForLoad;
        }
        if (GUILayout.Button(kButtonNameClear)) {
          DisposeWorld();
          worldMap.elements.Clear();
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

  [System.Serializable]
  public class LevelMap {
    public string name = "unnamed map";
    public string mapId = "<<mapId>>";
    public string ownerId = "<<ownerId>>";
    public StringMapElementDict elements = new StringMapElementDict();
  }

  // Ok, so this is a bit of a hack.
  // Unity's jsonutility parser hates a lot of things.
  // One thing it hates is Dictionaries.  It can't serialize them at all.
  // Hence, the SerializableDict class, which it CAN serialize.
  // Unfortunately, the OTHER thing it hates, is templated properties.
  // So we have this dorky class here, to specialize SerializeableDict,
  // so the unity jsonutility doesn't get confused.  Because while it CAN
  // serialize SerializableDict<string, MapElement> if it's the top level,
  // it can't serialize SerializableDict<string, MapElement> as a property.
  // But it CAN serialize StringMapElementDict as a property, even though
  // it's the same thing.
  [System.Serializable]
  public class StringMapElementDict : SerializableDict<string, MapElement> {
  }

  [System.Serializable]
  public class SerializableDict<KeyType, ValueType> {
    public List<KeyType> Keys = new List<KeyType>();
    public List<ValueType> Values = new List<ValueType>();

    public void Add(KeyType key, ValueType value) {
      Keys.Add(key);
      Values.Add(value);
    }

    public void Remove(KeyType key) {
      int index = Keys.IndexOf(key);
      if (index < 0) {
        Debug.LogError("Error - could not find key " + key + " in SerializableDict.");
      }
      else {
        Keys.RemoveAt(index);
        Values.RemoveAt(index);
      }
    }

    public int Count {
      get { return Keys.Count; }
    }

    public void Clear() {
      Keys.Clear();
      Values.Clear();
    }

    public bool ContainsKey(KeyType key) {
      return Keys.Contains(key);
    }

    // [] operators
    public ValueType this[KeyType key] {
      get {
        if (Keys.Contains(key)) {
          return Values[Keys.IndexOf(key)];
        }
        else
          return default(ValueType);
      }
      set {
        Keys.Add(key);
        Values.Add(value);
      }
    }
  }

  [System.Serializable]
  public class MapElement {
    public MapElementType type;
    public Vector3 scale = new Vector3(1, 1, 1);
    public Vector3 position = new Vector3(0, 0, 0);
    public Quaternion rotation = Quaternion.identity;

    public enum MapElementType {
      Empty,
      Wall,
      Floor,
      JumpPad,
      StartPosition,
      Goal,
    }
  }
}
