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
using System.Collections;
namespace Hamster.MapObjects
{

    //  this NetworkStartPosition component is the only thing that we really need. The other stuff is legacy single-player Mecha Hamster code.
    [RequireComponent(typeof(UnityEngine.Networking.NetworkStartPosition))]
    // General base-class for objects on the map.
    public class StartPosition : MapObject
    {
        public bool hasFinishedLoading; //  this is the cue that tells us the level has finished loading.

        static Vector3 kPlayerStartOffset = new Vector3(0, 2, 0);

        // Populated by the inspector:
        // Prefab to use when spawning a new player avatar at level start.
        public GameObject playerPrefab;

        public void Start()
        {
            hasFinishedLoading = true;  //  this lets us know (very roughly) if the level has finished loading. Since the level has many tiles and they all need to go through their Awake/Start() functions, it may take longer for some levels to load depending on the number of tiles.
        }

        public void UpdatePosOri(Transform xform)
        {
            xform.position = transform.position + kPlayerStartOffset;
            xform.rotation = Quaternion.identity;
        }
        //  ugh. this gets spammed because that's just how the original single-player mecha-hamster worked.
        public GameObject SpawnPlayer()
        {
            CommonData.mainGame.player = CommonData.mainGame.SpawnPlayer(); ;
            if (CommonData.mainGame.player != null)
            {
                UpdatePosOri(CommonData.mainGame.player.transform);
            }
            return CommonData.mainGame.player;
        }
        //  this should not be on FixedUpdate
        public void Update()
        {
            if (CommonData.mainGame.isGameRunning())
            {
                if (CommonData.mainGame.player == null)
                {
                    GameObject player = null;
                    //if (CommonData.networkManager != null)
                    //{
                    //    player = MainGame.NetworkSpawnPlayer(CommonData.customNetwork.networkManager.toServerConnection);
                    //}
                    //else
                    {
                        player = SpawnPlayer();
                    }
                }
            }
            else
            {
                if (CommonData.mainGame.player != null)
                {
                    Reset();
                }
            }
        }

        public override void Reset()
        {
            if (CommonData.mainGame.player != null)
            {
                //  StartPosition is not the boss of the player, so it shouldn't have authority to destroy the player. It should serve only to tell the player where to spawn.
                //  so this hack is removed.
                //  CommonData.mainGame.DestroyPlayer();

                //  if this is a networked game, we cannot destroy the player or else we lose the connection to the client. Therefore, we need to move the player to the start position by
                //  respawning the player.
            }
        }
    }
}
