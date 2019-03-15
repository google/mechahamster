using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
/*
 *  Unity has two scenes to load when creating a networked game.
 *  1) what they call "Offline scene" - I use NetworkStartScene.unity
 *  2) what they call "Online scene" - I use NetworkMainGameScene.unity.
 *  After #2 is loaded, all of the various prefabs and objects that are in that scene are ready to use.
 *  Before #2 is loaded, we can use the data in the objects in the scene, but they will be destroyed before #2 is loaded! So we must wait for #2 to load before we can do many things
 *  like load the appropriate level on the server.
 */
namespace Hamster.States
{
    public class ServerNetworkManagerOnlineSceneLoaded : BaseState
    {
        int startLevel = -1;    //  default to -1 allows the menu to be shown to select a level.

        override public void Initialize()
        {
            //Debug.LogWarning("ServerNetworkManagerOnlineSceneLoaded.Initialize: " + this.ToString());
            //Debug.LogWarning("ServerNetworkManagerOnlineSceneLoaded.Initialize: " + prevState.ToString());  
        }
        override public void Initialize(BaseState prevState)
        {
            Debug.Log("ServerNetworkManagerOnlineSceneLoaded.Initialize: prevState=" + prevState.ToString());
            if (prevState != null)
            {
                Hamster.States.ServerStartup startupState = prevState as Hamster.States.ServerStartup;
                if (startupState != null)
                {
                    Debug.LogError("startupState.levelIdx=" + startupState.levelIdx.ToString());
                    startLevel = startupState.levelIdx;
                }
            }
        }
        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        public override void Update ()
        {

            

            MultiplayerGame.instance.ServerSwapMultiPlayerState<Hamster.States.ServerLoadingLevel>(startLevel);
            //this.manager.PopState();    //  okay, we're done, so just exit this state?
        }
    }
}