using System.Text;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using Grpc.Core;
using Api;

namespace Hamster
{
    public class OpenMatchClient : MonoBehaviour
    {
        private const int OpenMatchPort = 50504;
        private Channel channel;
        private Frontend.FrontendClient client;
        private global::Messages.Player player;
         
        private string GetPlayerUniqueID()
        {
            return SystemInfo.deviceUniqueIdentifier;
        }

        /// <summary>Attempts to register a Player with the Front End API</summary>
        /// <param name="address">The IP address of the Front End API Server</param>
        /// <param name="matchProperties">A JSON-encoded list of properties that matches will select on</param>
        public bool Connect(string address, string matchProperties = null)
        {
            channel = new Channel(address, OpenMatchPort, ChannelCredentials.Insecure);
            client = new Frontend.FrontendClient(channel);

            player = new global::Messages.Player();
            player.Id = GetPlayerUniqueID();
            player.Properties = matchProperties ?? "{}";

            // TODO: Change this to async and return the async object instead. This
            // puts the onus of printing useful log messages on the caller, so provide
            // some interfaces to grab the player ID or whatever.

            global::Messages.Result result = client.CreatePlayer(player);

            if (result.Success)
            {
                Debug.LogFormat("OpenMatchClient: Connected to Front End API @ {0} as player {1}", address, player.Id);

                return true;
            }

            Debug.LogFormat("OpenMatchClient: Failed to connect to server @ {0}", address);

            return false;
        }

        /// <summary>Attempts to delete a registered Player from the Front End API</summary>
        /// <returns>The call object if the player was registered, else null</returns>
        public AsyncUnaryCall<global::Messages.Result> Disconnect()
        {
            if (player != null)
            {
                Debug.LogFormat("OpenMatchClient: Deleting player {0}", player.Id);

                return client.DeletePlayerAsync(player);
            }

            return null;
        }
    }
}
