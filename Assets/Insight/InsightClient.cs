using Mirror;
using System;
using System.Collections.Generic;
using Telepathy;
using UnityEngine;

namespace Insight
{
    public class InsightClient : InsightCommon
    {
        public bool AutoReconnect = true;
        protected int clientID = -1;
        protected int connectionID = 0;

        InsightNetworkConnection insightNetworkConnection;
        Client client; //Telepathy Client

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
                if(!isConnected && (_reconnectTimer < Time.time))
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

        public override bool SendMsgToAll(short msgType, MessageBase msg)
        {
            return SendMsg(0, msgType, msg);
        }

        public override bool Send(int connectionId, byte[] data)
        {
            if (client.Connected)
            {
                return SendBytes(connectionID, data);
            }
            Debug.Log("Client.Send: not connected!");
            return false;
        }

        public override bool SendMsg(int connectionId, short msgType, MessageBase msg)
        {
            NetworkWriter writer = new NetworkWriter();
            msg.Serialize(writer);

            // pack message and send
            byte[] message = Protocol.PackMessage((ushort)msgType, writer.ToArray());
            return SendBytes(0, message);
        }

        public bool SendMsg(short msgType, MessageBase msg)
        {
            NetworkWriter writer = new NetworkWriter();
            msg.Serialize(writer);

            // pack message and send
            byte[] message = Protocol.PackMessage((ushort)msgType, writer.ToArray());
            return SendBytes(0, message);
        }

        private bool SendBytes(int connectionId, byte[] bytes)
        {
            if (logNetworkMessages) { Debug.Log("ConnectionSend con:" + connectionId + " bytes:" + BitConverter.ToString(bytes)); }

            if (bytes.Length > int.MaxValue)
            {
                Debug.LogError("NetworkConnection:SendBytes cannot send packet larger than " + int.MaxValue + " bytes");
                return false;
            }

            if (bytes.Length == 0)
            {
                // zero length packets getting into the packet queues are bad.
                Debug.LogError("NetworkConnection:SendBytes cannot send zero bytes");
                return false;
            }

            return client.Send(bytes);
        }

        protected void HandleBytes(byte[] buffer)
        {
            // unpack message
            ushort msgType;
            byte[] content;
            if (Protocol.UnpackMessage(buffer, out msgType, out content))
            {
                if (logNetworkMessages) { Debug.Log(" msgType:" + msgType + " content:" + BitConverter.ToString(content)); }

                InsightNetworkMessageDelegate msgDelegate;
                if (messageHandlers.TryGetValue((short)msgType, out msgDelegate))
                {
                    // create message here instead of caching it. so we can add it to queue more easily.
                    InsightNetworkMessage msg = new InsightNetworkMessage();
                    msg.msgType = (short)msgType;
                    msg.reader = new NetworkReader(content);

                    msgDelegate(msg);
                }
                else
                {
                    //NOTE: this throws away the rest of the buffer. Need moar error codes
                    Debug.LogError("Unknown message ID " + msgType);// + " connId:" + connectionId);
                }
            }
            else
            {
                Debug.LogError("HandleBytes UnpackMessage failed for: " + BitConverter.ToString(buffer));
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