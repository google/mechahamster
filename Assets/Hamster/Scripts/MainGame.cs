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

using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Unity.Editor;
using UnityEngine.SocialPlatforms;

namespace Hamster {

  public class MainGame : MonoBehaviour {

    [HideInInspector]
    public States.StateManager stateManager = new States.StateManager();
    private float currentFrameTime, lastFrameTime;

    private const string PlayerPrefabID = "Player";

    // The active player object in the game.
    [HideInInspector]
    public GameObject player;

    // The player responsible for game music.
    private AudioSource musicPlayer;

    // The PlayerController component on the active player object.
    public PlayerController PlayerController {
      get {
        return player != null ? player.GetComponent<PlayerController>() : null;
      }
    }

    public UnityEvent PlayerSpawnedEvent = new UnityEvent();

    // Volume is treated as an int for the UI display.
    public const int MaxVolumeValue = 6;
    private int musicVolume = 0;
    public int MusicVolume {
      get {
        return musicVolume;
      }
      set {
        musicVolume = value;
        PlayerPrefs.SetInt(StringConstants.MusicVolume, musicVolume);
        // Music volume is controlled on the music source, which is set to
        // ignore the volume settings of the listener.
        CommonData.mainCamera.GetComponentInChildren<AudioSource>().volume =
          (float)musicVolume / MaxVolumeValue;
      }
    }
    private int soundFxVolume = 0;
    public int SoundFxVolume {
      get {
        return soundFxVolume;
      }
      set {
        soundFxVolume = value;
        PlayerPrefs.SetInt(StringConstants.SoundFxVolume, soundFxVolume);
        // Sound effect volumes are controlled by setting the listeners volume,
        // instead of each effect individually.
        AudioListener.volume = (float)soundFxVolume / MaxVolumeValue;
      }
    }

    private bool firebaseInitialized;

    IEnumerator Start() {
      Screen.SetResolution(Screen.width / 2, Screen.height / 2, true);
      GooglePlayServicesSignIn.InitializeGooglePlayGames();
      InitializeFirebaseAndStart();
      while (!firebaseInitialized) {
        yield return null;
      }
      StartGame();
    }

    void Update() {
      lastFrameTime = currentFrameTime;
      currentFrameTime = Time.realtimeSinceStartup;
      stateManager.Update();
    }

    void FixedUpdate() {
      stateManager.FixedUpdate();
    }

    // Play an audio clip as music.  If that clip is already playing,
    // we ignore it, and keep playing without restarting.
    public void PlayMusic(AudioClip music, bool shouldLoop) {
      if (musicPlayer.clip != music || !musicPlayer.isPlaying) {
        musicPlayer.Stop();
        musicPlayer.clip = music;
        musicPlayer.Play();
        musicPlayer.loop = shouldLoop;
      }
    }

    // Utility function for picking a random track to play from a selection.
    public void SelectAndPlayMusic(AudioClip[] musicArray, bool shouldLoop) {
      PlayMusic(musicArray[Random.Range(0, musicArray.Length)], shouldLoop);
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
      States.BaseState state = stateManager.CurrentState();
      return (state is States.Gameplay ||
        // While with LevelFinished the game is not technically running, we want
        // to mimic the traditional behavior in the background.
        state is States.LevelFinished);
    }

    // Utility function for spawning the player.
    public GameObject SpawnPlayer() {
      if (player == null) {
        player = (GameObject)Instantiate(CommonData.prefabs.lookup[PlayerPrefabID].prefab);
        PlayerSpawnedEvent.Invoke();
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

    // Sets the default values for remote config.  These are the values that will
    // be used if we haven't fetched yet.
    System.Threading.Tasks.Task InitializeRemoteConfig() {
      Dictionary<string, object> defaults = new Dictionary<string, object>();

      // VR Viewing height:
      defaults.Add(StringConstants.RemoteConfigVRHeightScale, 0.65f);
      // Physics defaults:
      defaults.Add(StringConstants.RemoteConfigPhysicsGravity, -20.0f);
      // Invites defaults:
      defaults.Add(StringConstants.RemoteConfigInviteTitleText,
          StringConstants.DefaultInviteTitleText);
      defaults.Add(StringConstants.RemoteConfigInviteMessageText,
          StringConstants.DefaultInviteMessageText);
      defaults.Add(StringConstants.RemoteConfigInviteCallToActionText,
          StringConstants.DefaultInviteCallToActionText);
      defaults.Add(StringConstants.RemoteConfigEmailContentHtml,
          StringConstants.DefaultEmailContentHtml);
      defaults.Add(StringConstants.RemoteConfigEmailSubjectText,
          StringConstants.DefaultEmailSubjectText);

      // Defaults for Map Objects:
      // Acceleration Tile
      defaults.Add(StringConstants.RemoteConfigAccelerationTileForce, 24.0f);
      // Drag Tile
      defaults.Add(StringConstants.RemoteConfigSandTileDrag, 5.0f);
      // Jump Tile
      defaults.Add(StringConstants.RemoteConfigJumpTileVelocity, 8.0f);
      // Mine Tile
      defaults.Add(StringConstants.RemoteConfigMineTileForce, 10.0f);
      defaults.Add(StringConstants.RemoteConfigMineTileRadius, 2.0f);
      defaults.Add(StringConstants.RemoteConfigMineTileUpwardsMod, 0.2f);
      // Spikes Tile
      defaults.Add(StringConstants.RemoteConfigSpikesTileForce, 10.0f);
      defaults.Add(StringConstants.RemoteConfigSpikesTileRadius, 1.0f);
      defaults.Add(StringConstants.RemoteConfigSpikesTileUpwardsMod, -0.5f);
      // Feature Flags
      defaults.Add(StringConstants.RemoteConfigGameplayRecordingEnabled, false);

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
      System.Threading.Tasks.Task.WhenAll(
          InitializeRemoteConfig()
        ).ContinueWith(task => { firebaseInitialized = true; });
    }

    // Actually start the game, once we've verified that everything
    // is working and we have the firebase prerequisites ready to go.
    void StartGame() {
      // FirebaseApp is responsible for starting up Crashlytics, when the core app is started.
      // To ensure that the core of FirebaseApp has started, grab the default instance which
      // is lazily initialized.
      FirebaseApp app = FirebaseApp.DefaultInstance;

      // Remote Config data has been fetched, so this applies it for this play session:
      Firebase.RemoteConfig.FirebaseRemoteConfig.ActivateFetched();

      CommonData.prefabs = FindObjectOfType<PrefabList>();
      CommonData.mainCamera = FindObjectOfType<CameraController>();
      CommonData.mainGame = this;
      Firebase.AppOptions ops = new Firebase.AppOptions();
      CommonData.app = Firebase.FirebaseApp.Create(ops);

      // Setup database url when running in the editor
#if UNITY_EDITOR
      if (CommonData.app.Options.DatabaseUrl == null) {
        CommonData.app.SetEditorDatabaseUrl("https://YOUR-PROJECT-ID.firebaseio.com");
      }
#endif

      Screen.orientation = ScreenOrientation.Landscape;

      musicPlayer = CommonData.mainCamera.GetComponentInChildren<AudioSource>();

      CommonData.gameWorld = FindObjectOfType<GameWorld>();

      // Set up volume settings.
      MusicVolume = PlayerPrefs.GetInt(StringConstants.MusicVolume, MaxVolumeValue);
      // Set the music to ignore the listeners volume, which is used for sound effects.
      CommonData.mainCamera.GetComponentInChildren<AudioSource>().ignoreListenerVolume = true;
      SoundFxVolume = PlayerPrefs.GetInt(StringConstants.SoundFxVolume, MaxVolumeValue);

      stateManager.PushState(new States.Startup());
    }
  }
}
