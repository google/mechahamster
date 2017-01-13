using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Firebase.Unity.Editor;


namespace Hamster.States {

  // State invoked whenever a new message is received.
  // Is responsible for decoding the message, and then
  // triggering whatever actions are necesary.
  class MessageReceived : BaseState {

    Firebase.Messaging.MessageReceivedEventArgs messageArgs;
    Dictionary<string, string> messageValues = new Dictionary<string, string>();
    string messageTitle, messageBody, messageFrom;

    public MessageReceived(Firebase.Messaging.MessageReceivedEventArgs messageArgs) {
      this.messageArgs = messageArgs;
    }

    public override void Initialize() {
      Time.timeScale = 0.0f;

      var notification = messageArgs.Message.Notification;

      if (notification == null) {
        Debug.LogError("Got a message with no notification.\n" + messageArgs.ToString());
        manager.PopState();
      }

      messageTitle = notification.Title;
      messageBody = notification.Body;

      if (messageArgs.Message.From.Length > 0) {
        messageFrom = messageArgs.Message.From;
      }

      if (messageArgs.Message.Data.Count > 0) {
        foreach (System.Collections.Generic.KeyValuePair<string, string> iter in
                 messageArgs.Message.Data) {
          messageValues[iter.Key] = iter.Value;
        }
      }

      string messageType = "";
      messageValues.TryGetValue(StringConstants.MessageKeyType, out messageType);

      switch (messageType) {
        case StringConstants.MessageTypeBonusMap:
          // Got a new map!
          CommonData.currentUser.data.AddBonusMap(
              messageValues[StringConstants.MessageDataMapName],
              messageValues[StringConstants.MessageDataMapId]);
          CommonData.currentUser.PushData();

          // Now tell them about it!
          manager.SwapState(new BasicDialog(string.Format(StringConstants.BonusMapUserMessage,
            messageValues[StringConstants.MessageDataMapName])));
          break;
        default:
          Debug.LogError("Received an unknown message key type: " +
            messageValues[StringConstants.MessageKeyType]);
          manager.PopState();
          break;
      }
    }

    public override void OnGUI() {
      GUI.skin = CommonData.prefabs.guiSkin;
      GUILayout.BeginVertical();
      GUILayout.EndVertical();
    }
  }
}
