using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
namespace Hamster.States
{
    /*
     * The server needs time to load the level before it can accept players. This has to do with how levels are created on the fly from prefab tile parts.
     */
    public class ServerLoadingLevel : BaseState
    {
        enum originalMHStates
        {
            UnknownState,
            BaseState,
            StartState,
            LoadLevelRequested,
            WaitingForDBLoad,
            ChooseSignInMenu,
            MainMenu,
            GentleLoadLevel,
            ForceLoadLevel,
            AgonesReady,
            Gameplay,   //  finished loading or choosing through the menu.
        };

        public int levelIdx = -1;
        bool bLoadedLevel = false;
        bool immediateLevelLoad = false;
        originalMHStates internalState;

        originalMHStates GetState(string statename)
        {
            switch (statename)
            {
                case "Hamster.States.BaseState":
                    return originalMHStates.BaseState;
                    break;

                case "Hamster.States.Gameplay":
                    return originalMHStates.Gameplay;
                    break;

                case "Hamster.States.MainMenu":
                    return originalMHStates.MainMenu;
                    break;
                case "Hamster.States.ChooseSignInMenu":
                    return originalMHStates.ChooseSignInMenu;
                    break;
                default:
                    return originalMHStates.UnknownState;
                    break;
            }
        }
        //  theoretically preferable to force load level in that it doesn't destroy the stack. However, perhaps the stack needed to be destroyed.
        public bool GentleLoadLevel(int idx)
        {
            Debug.Log("GentleLoadLevel=" + idx.ToString());
            bool bSuccess = false;
            if (Hamster.CommonData.mainGame != null)
            {
                Hamster.States.LevelSelect lvlSel = new Hamster.States.LevelSelect();   //  create new state for FSM that will let us force the starting level.
                bSuccess = lvlSel.RequestLoadLevel(idx); //  this is just the stub that initiates the state. It needs to run its update at least once before it has actually loaded any levels.
                Hamster.CommonData.mainGame.stateManager.PushState(lvlSel);    //  begin the state normally.
                internalState = originalMHStates.GentleLoadLevel;
            }

            // If we're running through Agones, signal ready after the level has loaded
            if (MultiplayerGame.instance.agones != null)
            {
                //MultiplayerGame.instance.agones.Ready();
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
                //MultiplayerGame.instance.agones.Ready();
                internalState = originalMHStates.AgonesReady;
            }
            return bSuccess;
        }

        override public void Initialize()
        {
            string msg = "ServerLoadingLevel.Initialize: level=" + levelIdx.ToString();
            CustomNetworkManagerHUD hud = MultiplayerGame.instance.manager.GetComponent<CustomNetworkManagerHUD>();
            if (hud != null)
                hud.showClientDebugInfoMessage(msg);


            bLoadedLevel = false;
            internalState = originalMHStates.StartState;
            if (levelIdx >= 0)
            {
                immediateLevelLoad = true;
            }
        }
        // Start is called before the first frame update
        //    void Start()    //  note this doesn't do anything because it was never implemented in the original MechaHamster.
        //{
        //    Debug.LogWarning("ServerLoadingLevel.Start: level=" + levelIdx.ToString() + "\n");
        //}

        // Update is called once per frame
        public override void Update()
        {
            //Debug.LogWarning("ServerLoadingLevel.Update: bLoadedLevel=" + bLoadedLevel.ToString() + "\n");
            if (immediateLevelLoad) //  otherwise, we'll go through the menu.
            {
                BaseState curState = Hamster.CommonData.mainGame.stateManager.CurrentState();
                string curStateName = curState.ToString();
                //Debug.Log("ServerLoadingLevel.Update: level=" + levelIdx.ToString() + "\n");
                //Debug.Log("ServerLoadingLevel:curState=" + curState + "(" + GetState(curStateName).ToString() + ")\n");
                //  we must wait until the original single player MechaHamster state has reached "MainMenu" before we can load the level
                if (GetState(curStateName) == originalMHStates.BaseState) {
                    bLoadedLevel = GentleLoadLevel(levelIdx);    //  force the MainGame state to load the level that was requested. 
                }
                if (GetState(curStateName) == originalMHStates.MainMenu)
                {
                    bLoadedLevel = GentleLoadLevel(levelIdx);    //  force the MainGame state to load the level that was requested. 
                }
                if (GetState(curStateName) == originalMHStates.ChooseSignInMenu)    //  skip the sign in menu since we're the server.
                {
                    bLoadedLevel = GentleLoadLevel(levelIdx);    //  force the MainGame state to load the level that was requested. 
                }
            }
            //  weird... we can load the level without this variable being set somehow. Okay, just check to see if we're in the game play state and then change our server state!
            //if (bLoadedLevel)   //  if the level is loaded, we can see if we're in a state to start the Pre-Openmatch gameplay.
            {
                BaseState curState = Hamster.CommonData.mainGame.stateManager.CurrentState();
                string curStateName = curState.ToString();
                string curStateEnum = GetState(curStateName).ToString();
                string msg = "ServerLoadingLevel.Update: level=" + levelIdx.ToString() + "\nstate=" + curStateName;
                CustomNetworkManagerHUD hud = MultiplayerGame.instance.manager.GetComponent<CustomNetworkManagerHUD>();
                if (hud != null)
                    hud.showClientDebugInfoMessage(msg);

                originalMHStates curMHState = GetState(curStateName);
                if (curMHState == originalMHStates.Gameplay)
                {
                    //  the server has finished loading the map and is ready to let players drop in. So go to the next state.
                    //  Allow players to drop in to the game now.
                    if (MultiplayerGame.instance.agones != null)
                    {
                        MultiplayerGame.instance.ServerEnterMultiPlayerState<Hamster.States.ServerListenForClients>();
                    }
                    else
                    {
                        MultiplayerGame.instance.ServerEnterMultiPlayerState<Hamster.States.ServerPreOpenMatchGamePlay>();
                    }
                }
            }
        }
    }
}