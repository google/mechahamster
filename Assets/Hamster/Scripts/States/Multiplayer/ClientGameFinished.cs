using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;
using UnityEngine.Networking;
namespace Hamster.States
{
    //  Client has reached its goal, but the game goes on for the others. This is misnamed.
    //  for the real "client game finished" see OnClientGameOver
    public class ClientGameFinished : BaseState
    {
        Menus.SingleLabelGUI menuComponent;
        int connectionId;
        float m_raceTime;

        static public void EnterState(int connId, float raceTime)
        {
            Hamster.States.ClientGameFinished clientReachedGoal = new Hamster.States.ClientGameFinished();   //  create new state for FSM that will let us force the starting level.

            Hamster.CommonData.mainGame.stateManager.PushState(clientReachedGoal);    //  begin the state normally.
        }

        // Start is called before the first frame update
        public override void Initialize()
        {
            //  menuComponent = SpawnUI<Menus.SingleLabelGUI>(StringConstants.PrefabsSingleLabelMenu);  //  this only happens on the server, so no reason to put up a menu
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