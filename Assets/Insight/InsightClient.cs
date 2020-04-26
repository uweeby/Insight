using Mirror;
using System;
using UnityEngine;

namespace Insight
{
    public class InsightClient : NetworkClient
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(InsightClient));

        public bool AutoStart;
        public bool AutoReconnect = true;
        public float ReconnectDelayInSeconds = 5f;
        public string NetworkAddress;

        float _reconnectTimer;

        public virtual void Start()
        {
            DontDestroyOnLoad(this);
            Application.runInBackground = true;

            if (AutoStart)
            {
                Connect();
            }
        }

        public void Connect()
        {
            var builder = new UriBuilder
            {
                Host = NetworkAddress,
                Scheme = "tcp4",
            };

            _ = ConnectAsync(builder.Uri);
        }

        public virtual void Update()
        {
            if (AutoReconnect)
            {
                if (!IsConnected && (_reconnectTimer < UnityEngine.Time.time))
                {
                    logger.Log("[InsightClient] - Trying to reconnect...");
                    _reconnectTimer = UnityEngine.Time.time + ReconnectDelayInSeconds;
                    Disconnect();
                }
            }
        }

        void OnApplicationQuit()
        {
            logger.Log("[InsightClient] Stopping Client");
            Disconnect();
        }
    }
}
