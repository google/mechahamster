using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
namespace Hamster.States
{
    public class ServerLoadingLevel : BaseState
    {
        enum originalMHStates
        {
            UnknownState,
            StartState,
            LoadLevelRequested,
            WaitingForDBLoad,
            MainMenu,
            GentleLoadLevel,
            ForceLoadLevel,
            AgonesReady,
            GamePlay,
        };

        public int levelIdx;
        bool bLoadedLevel = false;
        originalMHStates internalState;

        originalMHStates GetState(string statename)
        {
            switch (statename)
            {
                default:
                    return originalMHStates.UnknownState;
                    break;

                case "Hamster.States.MainMenu":
                    return originalMHStates.MainMenu;
                    break;
                case "Hamster.States.GamePlay":
                    return originalMHStates.GamePlay;
            }
        }
        //  theoretically preferable to force load level in that it doesn't destroy the stack. However, perhaps the stack needed to be destroyed.
        public bool GentleLoadLevel(int idx)
        {
            bool bSuccess = false;
            if (Hamster.CommonData.mainGame != null)
            {
                Hamster.States.LevelSelect lvlSel = new Hamster.States.LevelSelect();   //  create new state for FSM that will let us force the starting level.
                bSuccess = lvlSel.ForceLoadLevel(idx); //  this is just the stub that initiates the state. It needs to run its update at least once before it has actually loaded any levels.
                Hamster.CommonData.mainGame.stateManager.SwapState(lvlSel);    //  begin the state normally.
                internalState = originalMHStates.GentleLoadLevel;
            }

            // If we're running through Agones, signal ready after the level has loaded
            if (MultiplayerGame.instance.agones != null)
            {
                MultiplayerGame.instance.agones.Ready();
                internalState = originalMHStates.AgonesReady;
            }
            return bSuccess;
        }

        public bool ForceLoadLevel(int idx)
        {
            bool bSuccess = false;
            if (Hamster.CommonData.mainGame != null)
            {
                Hamster.States.LevelSelect lvlSel = new Hamster.States.LevelSelect();   //  create new state for FSM that will let us force the starting level.
                bSuccess = lvlSel.ForceLoadLevel(idx); //  this is just the stub that initiates the state. It needs to run its update at least once before it has actually loaded any levels.
                Hamster.CommonData.mainGame.stateManager.ClearStack(lvlSel);    //  hack: Just slam that state in there disregarding all previous states! OMG!!!
                internalState = originalMHStates.ForceLoadLevel;
            }

            // If we're running through Agones, signal ready after the level has loaded
            if (MultiplayerGame.instance.agones != null)
            {
                MultiplayerGame.instance.agones.Ready();
                internalState = originalMHStates.AgonesReady;
            }
            return bSuccess;
        }

        override public void Initialize()
        {
            Debug.LogWarning("ServerLoadingLevel.Initialize: level=" + levelIdx.ToString() + "\n");
            bLoadedLevel = false;
            internalState = originalMHStates.StartState;
        }
            // Start is called before the first frame update
        //    void Start()    //  note this doesn't do anything because it was never implemented in the original MechaHamster.
        //{
        //    Debug.LogWarning("ServerLoadingLevel.Start: level=" + levelIdx.ToString() + "\n");
        //}

        // Update is called once per frame
        public override void Update()
        {
            Debug.LogWarning("ServerLoadingLevel.Update: level=" + levelIdx.ToString() + "\n");
            //  we must wait until the original single player MechaHamster state has reached "MainMenu"
            if (!bLoadedLevel && Hamster.CommonData.mainGame != null)
            {
                BaseState curState = Hamster.CommonData.mainGame.stateManager.CurrentState();
                if (GetState(curState.ToString()) == originalMHStates.MainMenu)
                {
                    bLoadedLevel = GentleLoadLevel(levelIdx);    //  force the MainGame state to load the level that was requested. 
                }
                else if ((GetState(curState.ToString()) == originalMHStates.GamePlay))
                {
                    //  the server has finished loading the map and is ready to let players drop in.
                }
            }
        }
    }
}