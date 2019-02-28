using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
namespace Hamster.States
{
    public class ServerLoadingLevel : BaseState
    {
        public int levelIdx;

        public bool ForceLoadLevel(int idx)
        {
            bool bSuccess = false;
            if (Hamster.CommonData.mainGame != null)
            {
                Hamster.States.LevelSelect lvlSel = new Hamster.States.LevelSelect();   //  create new state for FSM that will let us force the starting level.
                bSuccess = lvlSel.ForceLoadLevel(idx); //  this is just the stub that initiates the state. It needs to run its update at least once before it has actually loaded any levels.
                Hamster.CommonData.mainGame.stateManager.ClearStack(lvlSel);    //  hack: Just slam that state in there disregarding all previous states! OMG!!!
            }

            // If we're running through Agones, signal ready after the level has loaded
            if (MultiplayerGame.instance.agones != null)
            {
                MultiplayerGame.instance.agones.Ready();
            }
            return bSuccess;
        }

        override public void Initialize()
        {
            Debug.LogWarning("ServerLoadingLevel.Initialize: level=" + levelIdx.ToString() + "\n");
        }
            // Start is called before the first frame update
            void Start()
        {
            Debug.LogWarning("ServerLoadingLevel.Start: level=" + levelIdx.ToString() + "\n");
            ForceLoadLevel(levelIdx);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}