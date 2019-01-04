using Mirror;
using System;
using System.Collections.Generic;
using Telepathy;
using UnityEngine;
using UnityEngine.Events;

namespace Insight
{
    public class InsightClient : InsightCommon
    {
        [HideInInspector]
        public UnityEvent OnConnectedEvent;

        public bool AutoReconnect = true;
        protected int clientID = -1;
        protected int connectionID = 0;

        InsightNetworkConnection insightNetworkConnection;
        Client client; //Telepathy Client

        public float ReconnectDelayInSeconds = 5f;
        private float _reconnectTimer;
       
        public virtual void Start()
        {
            DontDestroyOnLoad(gameObject);
            Application.runInBackground = true;

            // use Debug.Log functions for Telepathy so we can see it in the console
            Telepathy.Logger.LogMethod = Debug.Log;
            Telepathy.Logger.LogWarningMethod = Debug.LogWarning;
            Telepathy.Logger.LogErrorMethod = Debug.LogError;

            client = new Client();

            messageHandlers = new Dictionary<short, InsightNetworkMessageDelegate>();

            if (AutoStart)
            {
                StartInsight();
            }
        }
        public virtual void Update()
        {
            CheckConnection();

            HandleNewMessages();

            CheckCallbackTimeouts();
        }

        public void StartInsight(string Address, int Port)
        {
            networkAddress = Address;
            networkPort = Port;

            StartInsight();
        }

        public void StartInsight(int Port)
        {
            networkPort = Port;

            StartInsight();
        }

        public override void StartInsight()
        {
            client.Connect(networkAddress, networkPort);
            clientID = 0;
            insightNetworkConnection = new InsightNetworkConnection();
            insightNetworkConnection.Initialize(this, networkAddress, clientID, connectionID);
            OnStartInsight();
            _reconnectTimer = Time.realtimeSinceStartup + ReconnectDelayInSeconds;
        }

        public override void StopInsight()
        {
            client.Disconnect();
            OnStopInsight();
        }

        public bool IsConnecting()
        {
            return client.Connecting;
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

        public void HandleNewMessages()
        {
            if (clientID == -1)
                return;

            // grab all new messages. do this in your Update loop.
            Message msg;
            while (client.GetNextMessage(out msg))
            {
                switch (msg.eventType)
                {
                    case Telepathy.EventType.Connected:
                        connectState = ConnectState.Connected;
                        OnConnected(msg);
                        OnConnectedEvent.Invoke();
                        break;
                    case Telepathy.EventType.Data:
                        HandleBytes(msg.data);
                        break;
                    case Telepathy.EventType.Disconnected:
                        connectState = ConnectState.Disconnected;
                        OnDisconnected(msg);
                        break;
                }
            }
        }

        public void Send(byte[] data)
        {
            client.Send(data);
        }

        public void Send(short msgType, MessageBase msg)
        {
            Send(msgType, msg, null);
        }

        public void Send(short msgType, MessageBase msg, CallbackHandler callback)
        {
            if (!client.Connected)
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
                callbacks.Add(callbackId, new CallbackData() { callback = callback, timeout = Time.realtimeSinceStartup + CALLBACKTIMEOUT });
            }

            writer.Write(callbackId);

            msg.Serialize(writer);
            client.Send(writer.ToArray());
        }

        void HandleCallbackHandler(CallbackStatus status, NetworkReader reader)
        {
        }

        protected void HandleBytes(byte[] buffer)
        {
            // unpack message
            //ushort msgType;

            InsightNetworkMessageDelegate msgDelegate;
            NetworkReader reader = new NetworkReader(buffer);
            var msgType = reader.ReadInt16();
            var callbackId = reader.ReadInt32();
            var msg = new InsightNetworkMessage(insightNetworkConnection, callbackId) { msgType = msgType, reader = reader };

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


        private void OnApplicationQuit()
        {
            if (logNetworkMessages) { Debug.Log("[InsightClient] Stopping Client"); }
            StopInsight();
        }

        //------------Virtual Handlers-------------
        public virtual void OnConnected(Message msg)
        {
            if (logNetworkMessages) { Debug.Log("[InsightClient] - Connected to Insight Server"); }
        }

        public virtual void OnDisconnected(Message msg)
        {
            if (logNetworkMessages) { Debug.Log("[InsightClient] - OnDisconnected()"); }
        }

        public virtual void OnStartInsight()
        {
            if (logNetworkMessages) { Debug.Log("[InsightClient] - Connecting to Insight Server: " + networkAddress + ":" + networkPort); }
        }

        public virtual void OnStopInsight()
        {
            if (logNetworkMessages) { Debug.Log("[InsightClient] - Disconnecting from Insight Server"); }
        }
    }

  
}