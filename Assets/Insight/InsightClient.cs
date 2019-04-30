using Mirror;
using System;
using UnityEngine;

namespace Insight
{
    public class InsightClient : InsightCommon
    {
        public bool AutoReconnect = true;
        protected int clientID = -1; //-1 = never connected, 0 = disconnected, 1 = connected
        protected int connectionID = 0;

        InsightNetworkConnection insightNetworkConnection;

        Transport _transport;
        public virtual Transport transport
        {
            get
            {
                _transport = _transport ?? GetComponent<Transport>();
                if (_transport == null)
                    Debug.LogWarning("InsightClient has no Transport component. Networking won't work without a Transport");
                return _transport;
            }
        }

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
                    if (logNetworkMessages) { Debug.Log("[InsightClient] - Trying to reconnect..."); }
                    _reconnectTimer = Time.realtimeSinceStartup + ReconnectDelayInSeconds;
                    StartInsight();
                }
            }
        }

        public void Send(byte[] data)
        {
            transport.ClientSend(0, data);
        }

        public void Send(short msgType, MessageBase msg)
        {
            Send(msgType, msg, null);
        }

        public void Send(short msgType, MessageBase msg, CallbackHandler callback)
        {
            if (!transport.ClientConnected())
            {
                Debug.LogError("[InsightClient] - Client not connected!");
                return;
            }

            NetworkWriter writer = new NetworkWriter();
            writer.Write(msgType);

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

            writer.Write(callbackId);

            msg.Serialize(writer);
            transport.ClientSend(0, writer.ToArray());
        }

        void HandleCallbackHandler(CallbackStatus status, NetworkReader reader)
        {
        }

        void OnConnected()
        {
            if (insightNetworkConnection != null)
            {
                if (logNetworkMessages) { Debug.Log("[InsightClient] - Connected to Insight Server"); }
                connectState = ConnectState.Connected;
            }
            else Debug.LogError("Skipped Connect message handling because m_Connection is null.");
        }

        void OnDisconnected()
        {
            connectState = ConnectState.Disconnected;

            if (insightNetworkConnection != null)
            {
                insightNetworkConnection.InvokeHandlerNoData((short)MsgId.Disconnect);
            }
        }

        protected void HandleBytes(ArraySegment<byte> data)
        {
            InsightNetworkMessageDelegate msgDelegate;
            NetworkReader reader = new NetworkReader(data);
            short msgType = reader.ReadInt16();
            int callbackId = reader.ReadInt32();
            InsightNetworkMessage msg = new InsightNetworkMessage(insightNetworkConnection, callbackId)
            {
                msgType = msgType,
                reader = reader
            };

            if (callbacks.ContainsKey(callbackId))
            {
                callbacks[callbackId].callback.Invoke(CallbackStatus.Ok, msg);
                callbacks.Remove(callbackId);
            }
            else if (messageHandlers.TryGetValue(msgType, out msgDelegate))
            {
                msgDelegate(msg);
            }
            else
            {
                //NOTE: this throws away the rest of the buffer. Need moar error codes
                Debug.LogError("Unknown message ID " + msgType);// + " connId:" + connectionId);
            }
        }

        static void OnError(Exception exception)
        {
            // TODO Let's discuss how we will handle errors
            Debug.LogException(exception);
        }

        void OnApplicationQuit()
        {
            if (logNetworkMessages) { Debug.Log("[InsightClient] Stopping Client"); }
            StopInsight();
        }

        ////------------Virtual Handlers-------------
        public virtual void OnStartInsight()
        {
            if (logNetworkMessages) { Debug.Log("[InsightClient] - Connecting to Insight Server: " + networkAddress); }
        }

        public virtual void OnStopInsight()
        {
            if (logNetworkMessages) { Debug.Log("[InsightClient] - Disconnecting from Insight Server"); }
        }
    }
}
