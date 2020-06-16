using Mirror;
using System;
using UnityEngine;

namespace Insight
{
    public class InsightClient : InsightCommon
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(InsightClient));

        public bool AutoReconnect = true;
        protected int clientID = -1; //-1 = never connected, 0 = disconnected, 1 = connected
        protected int connectionID = 0;

        InsightNetworkConnection insightNetworkConnection;

        public float ReconnectDelayInSeconds = 5f;
        float _reconnectTimer;

        public virtual void Start()
        {
            if(DontDestroy)
            {
                DontDestroyOnLoad(this);
            }

            Application.runInBackground = true;

            if (AutoStart)
            {
                StartInsight();
            }

            clientID = 0;
            insightNetworkConnection = new InsightNetworkConnection();
            insightNetworkConnection.Initialize(this, networkAddress, clientID, connectionID);
            insightNetworkConnection.SetHandlers(messageHandlers);

            transport.OnClientConnected.AddListener(OnConnected);
            transport.OnClientDataReceived.AddListener(HandleBytes);
            transport.OnClientDisconnected.AddListener(OnDisconnected);
            transport.OnClientError.AddListener(OnError);
        }

        public virtual void Update()
        {
            CheckConnection();

            CheckCallbackTimeouts();
        }

        public void StartInsight(string Address)
        {
            networkAddress = Address;

            StartInsight();
        }

        public override void StartInsight()
        {
            transport.ClientConnect(networkAddress);

            OnStartInsight();

            _reconnectTimer = Time.realtimeSinceStartup + ReconnectDelayInSeconds;
        }

        public override void StopInsight()
        {
            transport.ClientDisconnect();
            OnStopInsight();
        }

        private void CheckConnection()
        {
            if (AutoReconnect)
            {
                if (!isConnected && (_reconnectTimer < Time.time))
                {
                    logger.Log("[InsightClient] - Trying to reconnect...");
                    _reconnectTimer = Time.realtimeSinceStartup + ReconnectDelayInSeconds;
                    StartInsight();
                }
            }
        }

        public void Send(byte[] data)
        {
            transport.ClientSend(0,  new ArraySegment<byte>(data));
        }

        public void Send(MessageBase msg)
        {
            Send(msg, null);
        }

        public void Send(MessageBase msg, CallbackHandler callback)
        {
            if (!transport.ClientConnected())
            {
                logger.LogError("[InsightClient] - Client not connected!");
                return;
            }

            NetworkWriter writer = new NetworkWriter();
            int msgType = GetId(default(MessageBase) != null ? typeof(MessageBase) : msg.GetType());
            writer.WriteUInt16((ushort)msgType);

            int callbackId = 0;
            if (callback != null)
            {
                callbackId = ++callbackIdIndex; // pre-increment to ensure that id 0 is never used.
                callbacks.Add(callbackId, new CallbackData()
                {
                    callback = callback,
                    timeout = Time.realtimeSinceStartup + callbackTimeout
                });
            }

            writer.WriteInt32(callbackId);

            msg.Serialize(writer);
            transport.ClientSend(0, new ArraySegment<byte>(writer.ToArray()));
        }

        void HandleCallbackHandler(CallbackStatus status, NetworkReader reader)
        {
        }

        void OnConnected()
        {
            if (insightNetworkConnection != null)
            {
                logger.Log("[InsightClient] - Connected to Insight Server");
                connectState = ConnectState.Connected;
            }
            else logger.LogError("Skipped Connect message handling because m_Connection is null.");
        }

        void OnDisconnected()
        {
            connectState = ConnectState.Disconnected;

            StopInsight();
        }

        protected void HandleBytes(ArraySegment<byte> data, int channelId)
        {
            InsightNetworkMessageDelegate msgDelegate;
            NetworkReader reader = new NetworkReader(data);
            if(UnpackMessage(reader, out int msgType))
            {
                int callbackId = reader.ReadInt32();

                if (callbacks.ContainsKey(callbackId))
                {
                    callbacks.Remove(callbackId);
                }

                if (messageHandlers.TryGetValue(msgType, out msgDelegate))
                {
                    insightNetworkConnection.InvokeHandler(msgType, reader, channelId);
                }
            }
            else
            {
                //NOTE: this throws away the rest of the buffer. Need moar error codes
                logger.LogError("Unknown message ID " + msgType);// + " connId:" + connectionId);
            }
        }

        void OnError(Exception exception)
        {
            // TODO Let's discuss how we will handle errors
            logger.LogException(exception);
        }

        void OnApplicationQuit()
        {
            logger.Log("[InsightClient] Stopping Client");
            StopInsight();
        }

        ////------------Virtual Handlers-------------
        public virtual void OnStartInsight()
        {
            logger.Log("[InsightClient] - Connecting to Insight Server: " + networkAddress);
        }

        public virtual void OnStopInsight()
        {
            logger.Log("[InsightClient] - Disconnecting from Insight Server");
        }
    }
}
