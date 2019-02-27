using System.Text;
using UnityEngine;
using System;
using System.Threading;
using System.Threading.Tasks;
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
        private Messages.Player player;
        private AsyncServerStreamingCall<Messages.Player> asyncUpdate;
        private string address = "";
        private int port = 0;

        public string Address
        {
            get { return address; }
        }
        public int Port
        {
            get { return port; }
        }
         
        private string GetPlayerUniqueID()
        {
            return UnityEngine.Application.platform.ToString().ToLower() + "-" + SystemInfo.deviceUniqueIdentifier;
        }

        private async Task<string> WaitForMatchResults(AsyncServerStreamingCall<Messages.Player> stream)
        {
            Metadata header = await stream.ResponseHeadersAsync;

            var responseStream = stream.ResponseStream;

            while (await responseStream.MoveNext(CancellationToken.None))
            {
                var reply = responseStream.Current;

                if (reply.Assignment.Length > 0)
                {
                    stream.Dispose();

                    return reply.Assignment;
                }
            }

            return "";
        }

        private IEnumerator RetrieveMatchResults()
        {
            Debug.Log("OpenMatchClient: Waiting for response from server...");

            Task<string> result;

            result = WaitForMatchResults(asyncUpdate);

            while (!result.IsCompleted)
            {
                yield return null;
            }

            string assignment = result.Result;

            // The server returns a string in the form of IP:PORT, and running on GCP
            // this is always ever an IPv4 string.
            string[] tokens = assignment.Split(':');

            if (tokens.Length == 2)
            {
                address = tokens[0];
                port = Convert.ToInt32(tokens[1]);

                Debug.LogFormat("OpenMatchClient: Assigned to game server {0}:{1}", address, port);
            }
            else
            {
                Debug.LogFormat("OpenMatchClient: Error parsing assignment result: {0}", assignment);
            }
        }

        /// <summary>Attempts to register a Player with the Front End API</summary>
        /// <param name="address">The IP address of the Front End API Server</param>
        /// <param name="matchProperties">A JSON-encoded list of properties that matches will select on</param>
        public bool Connect(string address, string matchProperties = "{}")
        {
            channel = new Channel(address, OpenMatchPort, ChannelCredentials.Insecure);
            client = new Frontend.FrontendClient(channel);

            player = new Messages.Player();
            player.Id = GetPlayerUniqueID();
            player.Properties = matchProperties;

            // Attempt to delete the player ID first because if the ID exists already in Open Match it will
            // be ignored until it is removed (by the client or internal clean-up timeouts).
            client.DeletePlayer(player);

            Debug.LogFormat("OpenMatchClient: Attempting to create [{0} : {1}]", player.Id, player.Properties);

            Messages.Result result = client.CreatePlayer(player);

            if (result.Success)
            {
                Debug.LogFormat("OpenMatchClient: Connected to Front End API @ {0} as player {1}", address, player.Id);

                var stop = new CancellationTokenSource();

                var callOptions = new CallOptions()
                    .WithCancellationToken(stop.Token)
                    .WithDeadline(DateTime.UtcNow.AddMinutes(1))
                    .WithHeaders(Metadata.Empty);
                
                asyncUpdate = client.GetUpdates(player, callOptions);

                //
                StartCoroutine(RetrieveMatchResults());

                return true;
            }

            Debug.LogFormat("OpenMatchClient: Failed to connect to server @ {0}", address);

            return false;
        }

        /// <summary>Attempts to delete a registered Player from the Front End API</summary>
        /// <returns>True if the player is successfully deleted</returns>
        public bool Disconnect()
        {
            if (player != null)
            {
                Debug.LogFormat("OpenMatchClient: Deleting player {0}", player.Id);

                asyncUpdate.Dispose();

                Messages.Result result = client.DeletePlayer(player);

                address = "";
                port = 0;

                return result.Success; 
            }

            return true;
        }
    }
}
