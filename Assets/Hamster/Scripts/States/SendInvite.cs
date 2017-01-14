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
      Time.timeScale = 0.0f;

      if (CommonData.gameWorld.worldMap.mapId == StringConstants.DefaultMapId) {
        Debug.LogWarning("Error - Trying to share an unsaved map!");
        manager.PopState();
        return;
      }

      Firebase.Invites.Invite invite = new Firebase.Invites.Invite();

      invite.TitleText = Firebase.RemoteConfig.FirebaseRemoteConfig.GetValue(
        StringConstants.RemoteConfigInviteTitleText).StringValue;
      invite.MessageText = Firebase.RemoteConfig.FirebaseRemoteConfig.GetValue(
        StringConstants.RemoteConfigInviteTitleText).StringValue;
      invite.CallToActionText = Firebase.RemoteConfig.FirebaseRemoteConfig.GetValue(
        StringConstants.RemoteConfigInviteCallToActionText).StringValue;

      invite.DeepLinkUrl = new System.Uri(
          string.Format(StringConstants.DefaultInviteDeepLinkUrl,
              CommonData.gameWorld.worldMap.mapId));

      Firebase.Invites.FirebaseInvites.SendInviteAsync(invite).ContinueWith(task => {
        isComplete = true;
        if (task.IsFaulted) {
          Debug.LogError("Invite failed!\n" + task.Exception);
        } else {
          SetMapToShared();
        }
      });
    }

    // Resume the state.  Called when the state becomes active
    // when the state above is removed.  That state may send an
    // optional object containing any results/data.  Results
    // can also just be null, if no data is sent.
    public override void Resume(StateExitValue results) {
      Time.timeScale = 0.0f;
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
