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


namespace Hamster.States {

  // State invoked whenever a new message is received.
  // Is responsible for decoding the message, and then
  // triggering whatever actions are necesary.
  class MessageReceived : BaseState {

    Firebase.Messaging.MessageReceivedEventArgs messageArgs;
    Dictionary<string, string> messageValues = new Dictionary<string, string>();

    public MessageReceived(Firebase.Messaging.MessageReceivedEventArgs messageArgs) {
      this.messageArgs = messageArgs;
    }

    public override void Initialize() {
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
        case StringConstants.MessageTypeText:
          manager.SwapState(new BasicDialog(messageValues[StringConstants.MessageDataText]));
          break;
        default:
          Debug.LogError("Received an unknown message key type: " +
            messageValues[StringConstants.MessageKeyType]);
          manager.PopState();
          break;
      }
    }
  }
}
