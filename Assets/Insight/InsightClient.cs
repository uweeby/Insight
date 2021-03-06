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

            transport.OnClientConnected=OnConnected;
            transport.OnClientDataReceived=HandleBytes;
            transport.OnClientDisconnected=OnDisconnected;
            transport.OnClientError=OnError;
        }

        public virtual void Update()
        {
            CheckConnection();

            CheckCallbackTimeouts();
        }

        public void StartInsight(string Address)
        {
            if(string.IsNullOrEmpty(Address))
            {
                Debug.LogError("[InsightClient] - Address provided in StartInsight is Null or Empty. Not Starting.");
                return;
            }

            networkAddress = Address;

            StartInsight();
        }

        public override void StartInsight()
        {
            transport.ClientConnect(networkAddress);

            OnStartInsight();

            _reconnectTimer = Time.realtimeSinceStartup + ReconnectDelayInSeconds;
        }

        public void StartInsight(Uri uri)
        {
            transport.ClientConnect(uri);

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
                    Debug.Log("[InsightClient] - Trying to reconnect...");
                    _reconnectTimer = Time.realtimeSinceStartup + ReconnectDelayInSeconds;
                    StartInsight();
                }
            }
        }

        public void Send(byte[] data)
        {
            transport.ClientSend(0,  new ArraySegment<byte>(data));
        }

        public void Send<T>(T msg) where T : Message
        {
            Send(msg, null);
        }

        public void Send<T>(T msg, CallbackHandler callback) where T : Message
        {
            if (!transport.ClientConnected())
            {
                Debug.LogError("[InsightClient] - Client not connected!");
                return;
            }

            NetworkWriter writer = new NetworkWriter();
            int msgType = GetId(default(T) != null ? typeof(T) : msg.GetType());
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

            Writer<T>.write.Invoke(writer, msg);
            transport.ClientSend(0, new ArraySegment<byte>(writer.ToArray()));
        }

        void HandleCallbackHandler(CallbackStatus status, NetworkReader reader)
        {
        }

        void OnConnected()
        {
            if (insightNetworkConnection != null)
            {
                Debug.Log("[InsightClient] - Connected to Insight Server");
                connectState = ConnectState.Connected;
            }
            else Debug.LogError("Skipped Connect message handling because m_Connection is null.");
        }

        void OnDisconnected()
        {
            connectState = ConnectState.Disconnected;

            StopInsight();
        }

        protected void HandleBytes(ArraySegment<byte> data, int i)
        {
            InsightNetworkMessageDelegate msgDelegate;
            NetworkReader reader = new NetworkReader(data);
            if(UnpackMessage(reader, out int msgType))
            {
                int callbackId = reader.ReadInt32();
                InsightNetworkMessage msg = new InsightNetworkMessage(insightNetworkConnection, callbackId)
                {
                    msgType = msgType,
                    reader = reader
                };

                if (callbacks.ContainsKey(callbackId))
                {
                    callbacks[callbackId].callback.Invoke(msg);
                    callbacks.Remove(callbackId);
                }
                else if (messageHandlers.TryGetValue(msgType, out msgDelegate))
                {
                    msgDelegate(msg);
                }
            }
            else
            {
                //NOTE: this throws away the rest of the buffer. Need moar error codes
                Debug.LogError("Unknown message ID " + msgType);// + " connId:" + connectionId);
            }
        }

        void OnError(Exception exception)
        {
            // TODO Let's discuss how we will handle errors
            Debug.LogException(exception);
        }

        void OnApplicationQuit()
        {
            Debug.Log("[InsightClient] Stopping Client");
            StopInsight();
        }

        ////------------Virtual Handlers-------------
        public virtual void OnStartInsight()
        {
            Debug.Log("[InsightClient] - Connecting to Insight Server: " + networkAddress);
        }

        public virtual void OnStopInsight()
        {
            Debug.Log("[InsightClient] - Disconnecting from Insight Server");
        }
    }
}
