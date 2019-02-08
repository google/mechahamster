
using System.Text;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

namespace Hamster
{
    // A communication module to interact with the Agones sidecar
    public class AgonesClient : MonoBehaviour
    {
        private const string ReadyURL = "http://localhost:59358/ready";
        private const string HealthURL = "http://localhost:59358/health";
        private const string ShutdownURL = "http://localhost:59358/shutdown";
        public float HealthCheckPeriod = 2.5f;

        private WaitForSecondsRealtime m_healthCheckDelay;

        // Posts a JSON string to a given url
        private static void PostJSON(string url, string json)
        {
            UnityWebRequest request = UnityWebRequest.Post(url, json);
            byte[] payload = Encoding.UTF8.GetBytes(json);

            request.uploadHandler = new UploadHandlerRaw(payload);
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 5;

            request.SendWebRequest();
        }

        // Periodically posts a message to the Agones health check endpoint
        private IEnumerator HealthCheck()
        {
            Debug.LogFormat("AgonesClient: Beginning health check with {0}s period", HealthCheckPeriod);

            while (true)
            {
                PostJSON(AgonesClient.HealthURL, "{}");

                yield return m_healthCheckDelay;
            }
        }

        private void Awake()
        {
            // We start the health check in Awake() so that other scripts can signal Ready()
            // in their Start() event.
            m_healthCheckDelay = new WaitForSecondsRealtime(HealthCheckPeriod);
            StartCoroutine(HealthCheck());
        }

        // ----- Public API

        // Signals to Agones that the server is ready to accept player connections
        public void Ready()
        {
            Debug.Log("AgonesClient: Signaling ready");
            AgonesClient.PostJSON(AgonesClient.ReadyURL, "{}");
        }

        // Signals to Agones that the server is shutting down
        public void Shutdown()
        {
            Debug.Log("AgonesClient: Signaling shutdown");
            AgonesClient.PostJSON(AgonesClient.ShutdownURL, "{}");
        }
    }
}
