using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;
using UnityEngine.Networking;
namespace Hamster.States
{
    //  All of the players have finished the race. Ends the match and tells the clients the race results
    //  and to GTFO my server.
    public class ServerGameFinished : BaseState
    {
        Menus.SingleLabelGUI menuComponent;
        int connectionId;
        float m_raceTime;

        static public void EnterState(int connId, float raceTime)
        {
            Hamster.States.ServerGameFinished serverFinishedMatch = new Hamster.States.ServerGameFinished();   //  create new state for FSM that will let us force the starting level.
            serverFinishedMatch.m_raceTime = raceTime;
            serverFinishedMatch.connectionId = connId;

            Hamster.CommonData.mainGame.stateManager.PushState(serverFinishedMatch);    //  begin the state normally.
        }

        // Start is called before the first frame update
        public override void Initialize()
        {
            //  menuComponent = SpawnUI<Menus.SingleLabelGUI>(StringConstants.PrefabsSingleLabelMenu);  //  this only happens on the server, so no reason to put up a menu

            Time.timeScale = 1.0f;
            CommonData.mainCamera.mode = CameraController.CameraMode.Gameplay;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            MultiplayerGame.instance.ServerGameOver();

        }

        public override void Resume(StateExitValue results)
        {
            ShowUI();
            //  CommonData.mainGame.SelectAndPlayMusic(CommonData.prefabs.gameMusic, true); //  not necessary to do this on the server.
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