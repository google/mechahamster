using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;
using UnityEngine.Networking;
namespace Hamster.States
{
    //  one of the players has finished the match. Need to tell that client and maybe the other clients what's happened!
    public class ServerGameFinished : BaseState
    {
        Menus.SingleLabelGUI menuComponent;
        int connectionId;
        float m_raceTime;

        static public void EnterState(int connId, float raceTime)
        {
            Hamster.States.ServerGameFinished clientFinishedMatch = new Hamster.States.ServerGameFinished();   //  create new state for FSM that will let us force the starting level.
            clientFinishedMatch.m_raceTime = raceTime;
            clientFinishedMatch.connectionId = connId;

            Hamster.CommonData.mainGame.stateManager.PushState(clientFinishedMatch);    //  begin the state normally.
        }

        // Start is called before the first frame update
        public override void Initialize()
        {
            menuComponent = SpawnUI<Menus.SingleLabelGUI>(StringConstants.PrefabsSingleLabelMenu);
            //menuComponent = SpawnUI<Menus.SingleLabelGUI>(StringConstants.PrefabsLevelFinishedMenu);  //  nope. This spawns a big prefab menu system.

            //customNetwork.CustomNetworkManager custMgr = MultiplayerGame.instance.manager as customNetwork.CustomNetworkManager;
            //if (custMgr != null) {
            //    custMgr.server_SendRaceTime(this.connectionId, this.m_raceTime);
            //}

            Time.timeScale = 1.0f;
            CommonData.mainCamera.mode = CameraController.CameraMode.Gameplay;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            MultiplayerGame.instance.ServerGameOver();

        }

        public override void Resume(StateExitValue results)
        {
            ShowUI();
            CommonData.mainGame.SelectAndPlayMusic(CommonData.prefabs.gameMusic, true);
            if (CommonData.vrPointer != null)
            {
                CommonData.vrPointer.SetActive(false);
            }
            Time.timeScale = 1.0f;
            CommonData.mainCamera.mode = CameraController.CameraMode.Gameplay;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        public override StateExitValue Cleanup()
        {
            DestroyUI();
            if (CommonData.vrPointer != null)
            {
                CommonData.vrPointer.SetActive(true);
            }
            CommonData.mainCamera.mode = CameraController.CameraMode.Menu;
            Utilities.HideDuringGameplay.OnGameplayStateChange(false);
            Time.timeScale = 0.0f;
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
            //DestroyReplayAnimator();

            //if (gameplayRecordingEnabled)
            //{
            //    CommonData.mainGame.PlayerSpawnedEvent.RemoveListener(OnPlayerSpawned);
            //}

            return new StateExitValue(typeof(ServerGameFinished));
        }

        public override void Suspend()
        {
            HideUI();
            if (CommonData.vrPointer != null)
            {
                CommonData.vrPointer.SetActive(true);
            }
            Time.timeScale = 0.0f;
            CommonData.mainCamera.mode = CameraController.CameraMode.Menu;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        public override void Update()
        {
        }
    }
}