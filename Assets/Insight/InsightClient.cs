using Mirror;
using System;
using System.Collections.Generic;
using Telepathy;
using UnityEngine;

namespace Insight
{
    public enum CallbackStatus
    {
        Ok,
        Error
    }

    public class InsightClient : InsightCommon
    {
        public bool AutoReconnect = true;
        protected int clientID = -1;
        protected int connectionID = 0;

        InsightNetworkConnection insightNetworkConnection;
        Client client; //Telepathy Client

        private float _reconnectTimer;

        protected int callbackIdIndex = 0; // 0 is a _special_ id - it represents _no callback_. 
        protected Dictionary<int, CallbackHandler> callbacks = new Dictionary<int, CallbackHandler>();

        public delegate void CallbackHandler(CallbackStatus status, NetworkReader reader);

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
                    _reconnectTimer = Time.time + 5; //Wait 5 seconds before trying to connect again
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
                callbacks.Add(callbackId, callback);
            }

            writer.Write(callbackId);

            msg.Serialize(writer);
            client.Send(writer.ToArray());
        }

        void HandleCallbackHandler(CallbackStatus status, NetworkReader reader)
        {
        }


        //public override bool SendMsgToAll(short msgType, MessageBase msg)
        //{
        //    return SendMsg(0, msgType, msg);
        //}

        //public override bool Send(int connectionId, byte[] data)
        //{
        //    if (client.Connected)
        //    {
        //        return SendBytes(connectionID, data);
        //    }
        //    Debug.Log("Client.Send: not connected!");
        //    return false;
        //}

        //public override bool SendMsg(int connectionId, short msgType, MessageBase msg)
        //{
        //    NetworkWriter writer = new NetworkWriter();
        //    msg.Serialize(writer);

        //    // pack message and send
        //    byte[] message = Protocol.PackMessage((ushort)msgType, writer.ToArray());
        //    return SendBytes(0, message);
        //}

        //public bool SendMsg(short msgType, MessageBase msg)
        //{
        //    NetworkWriter writer = new NetworkWriter();
        //    msg.Serialize(writer);

        //    // pack message and send
        //    byte[] message = Protocol.PackMessage((ushort)msgType, writer.ToArray());
        //    return SendBytes(0, message);
        //}

        //private bool SendBytes(int connectionId, byte[] bytes)
        //{
        //    if (logNetworkMessages) { Debug.Log("ConnectionSend con:" + connectionId + " bytes:" + BitConverter.ToString(bytes)); }

        //    if (bytes.Length > int.MaxValue)
        //    {
        //        Debug.LogError("NetworkConnection:SendBytes cannot send packet larger than " + int.MaxValue + " bytes");
        //        return false;
        //    }

        //    if (bytes.Length == 0)
        //    {
        //        // zero length packets getting into the packet queues are bad.
        //        Debug.LogError("NetworkConnection:SendBytes cannot send zero bytes");
        //        return false;
        //    }

        //    return client.Send(bytes);
        //}

        protected void HandleBytes(byte[] buffer)
        {
            // unpack message
            //ushort msgType;

            InsightNetworkMessageDelegate msgDelegate;
            NetworkReader reader = new NetworkReader(buffer);
            var msgType = reader.ReadInt16();
            var callbackId = reader.ReadInt32();

            if (logNetworkMessages) Debug.Log(" msgType:" + msgType + " content:" + BitConverter.ToString(buffer));

            if (callbacks.ContainsKey(callbackId))
            {
                callbacks[callbackId].Invoke(CallbackStatus.Ok, reader);
                callbacks.Remove(callbackId);
            }
            else if (messageHandlers.TryGetValue(msgType, out msgDelegate))
            {
                // create message here instead of caching it. so we can add it to queue more easily.
                InsightNetworkMessage msg = new InsightNetworkMessage(insightNetworkConnection, callbackId);
                msg.msgType = msgType;
                msg.reader = reader;
                
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