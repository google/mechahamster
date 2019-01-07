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
using System.Collections.Generic;
using Firebase.Unity.Editor;
using System.Threading.Tasks;


namespace Hamster.States {

  // Utility state for sending firebase invites.  It basically
  // just triggers the invite sending, waits around
  // until it's done, and then exits the state.
  class SendInvite : BaseState {
    bool isComplete = false;

    // Initialization method.  Called after the state
    // is added to the stack.
    public override void Initialize() {
      if (CommonData.gameWorld.worldMap.mapId == StringConstants.DefaultMapId) {
        Debug.LogWarning("Error - Trying to share an unsaved map!");
        manager.PopState();
        return;
      }

      Firebase.Invites.Invite invite = new Firebase.Invites.Invite();

      invite.TitleText = Firebase.RemoteConfig.FirebaseRemoteConfig.GetValue(
        StringConstants.RemoteConfigInviteTitleText).StringValue;
      invite.MessageText = Firebase.RemoteConfig.FirebaseRemoteConfig.GetValue(
        StringConstants.RemoteConfigInviteMessageText).StringValue;
      invite.CallToActionText = Firebase.RemoteConfig.FirebaseRemoteConfig.GetValue(
        StringConstants.RemoteConfigInviteCallToActionText).StringValue;

      invite.EmailContentHtml = Firebase.RemoteConfig.FirebaseRemoteConfig.GetValue(
        StringConstants.RemoteConfigEmailContentHtml).StringValue;
      invite.EmailSubjectText = Firebase.RemoteConfig.FirebaseRemoteConfig.GetValue(
        StringConstants.RemoteConfigEmailSubjectText).StringValue;

      invite.DeepLinkUrl = new System.Uri(
          string.Format(StringConstants.DefaultInviteDeepLinkUrl,
              CommonData.gameWorld.worldMap.mapId));

      Firebase.Invites.FirebaseInvites.SendInviteAsync(invite).ContinueWith(task => {
        isComplete = true;
        if (task.IsFaulted) {
          Debug.LogError("Invite failed!\n" + task.Exception);
        } else {
          Firebase.Analytics.FirebaseAnalytics.LogEvent(StringConstants.AnalyticsEventMapShared,
            StringConstants.AnalyticsParamMapId, CommonData.gameWorld.worldMap.mapId);
          if (Social.localUser.authenticated) {
            Social.ReportProgress(GPGSIds.achievement_friendly_hamster, 100.0f, (bool success) => {
              Debug.Log("Map sharing achievement unlocked. Sucess: " + success);
            });
          }
          SetMapToShared();
        }
      });
    }

    // Called once per frame when the state is active.
    public override void Update() {
      if (isComplete) {
        manager.PopState();
      }
    }

    // Save the current map to the database.  If no mapID is provided,
    // a new id is created.  Otherwise, it saves over the existing ID.
    void SetMapToShared() {
      LevelMap currentLevel = CommonData.gameWorld.worldMap;

      if (currentLevel.isShared == true) {
        // Map is already shared.
        return;
      }

      currentLevel.isShared = true;
      DBStruct<LevelMap> dbLevel = new DBStruct<LevelMap>(
          CommonData.DBMapTablePath + currentLevel.mapId, CommonData.app);
      dbLevel.Initialize(currentLevel);
      dbLevel.PushData();
    }
  }
}
