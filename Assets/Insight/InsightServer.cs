using Mirror;
using System.Collections.Generic;
using Telepathy;
using UnityEngine;

namespace Insight
{
    public class InsightServer : InsightCommon
    {
        protected int serverHostId = -1;

        Server server; //Telepathy Server

		protected Dictionary<int, InsightNetworkConnection> connections;

        public virtual void Start()
        {
            DontDestroyOnLoad(this);
            Application.runInBackground = true;

            // use Debug.Log functions for Telepathy so we can see it in the console
            Telepathy.Logger.LogMethod = Debug.Log;
            Telepathy.Logger.LogWarningMethod = Debug.LogWarning;
            Telepathy.Logger.LogErrorMethod = Debug.LogError;

            // create and start the server
            server = new Server();

            connections = new Dictionary<int, InsightNetworkConnection>();

            messageHandlers = new Dictionary<short, InsightNetworkMessageDelegate>();

            if(AutoStart)
            {
                StartInsight();
            }
        }

        public virtual void Update()
        {
            HandleNewMessages();
            CheckCallbackTimeouts();
        }

        public void StartInsight(int Port)
        {
            networkPort = Port;

            StartInsight();
        }

        public override void StartInsight()
        {
            if (logNetworkMessages) { Debug.Log("[InsightServer] - Start On Port: " + networkPort); }
            server.Start(networkPort);
            serverHostId = 0;

            connectState = ConnectState.Connected;

            OnStartInsight();
        }

        public override void StopInsight()
        {
            connections.Clear();

            // stop the server when you don't need it anymore
            server.Stop();
            serverHostId = -1;

            connectState = ConnectState.Disconnected;

            OnStopInsight();
        }

        // grab all new messages. do this in your Update loop.
        public void HandleNewMessages()
        {
            if (serverHostId == -1)
                return;

            Message msg;
            while (server.GetNextMessage(out msg))
            {
                switch (msg.eventType)
                {
                    case Telepathy.EventType.Connected:
                        HandleConnect(msg);
                        break;
                    case Telepathy.EventType.Data:
                        HandleData(msg.connectionId, msg.data, 0);
                        break;
                    case Telepathy.EventType.Disconnected:
                        HandleDisconnect(msg);
                        break;
                }
            }
        }

        void HandleConnect(Message msg)
        {
            Debug.Log("connectionID: " + msg.connectionId, this);

            // get ip address from connection
            string address = GetConnectionInfo(msg.connectionId);

            // add player info
            InsightNetworkConnection conn = new InsightNetworkConnection();
            conn.Initialize(this, address, serverHostId, msg.connectionId);
            AddConnection(conn);

            OnConnected(conn);
        }

        void HandleDisconnect(Message msg)
        {
            InsightNetworkConnection conn;
            if (connections.TryGetValue(msg.connectionId, out conn))
            {
                conn.Disconnect();
                RemoveConnection(msg.connectionId);

                OnDisconnected(conn);
            }
        }

        void HandleData(int connectionId, byte[] data, byte error)
        {
            InsightNetworkConnection conn;

            NetworkReader reader = new NetworkReader(data);
            var msgType = reader.ReadInt16();
            var callbackId = reader.ReadInt32();

            if (callbacks.ContainsKey(callbackId))
            {
                callbacks[callbackId].callback.Invoke(CallbackStatus.Ok, reader);
                callbacks.Remove(callbackId);
            }
            else if (connections.TryGetValue(connectionId, out conn))
            {
                conn.TransportReceive(data);
                return;
            }
            else
            {
                Debug.LogError("HandleData Unknown connectionId:" + connectionId);
            }
        }

        public string GetConnectionInfo(int connectionId)
        {
            string address;
            server.GetConnectionInfo(connectionId, out address);
            return address;
        }

        public bool AddConnection(InsightNetworkConnection conn)
        {
            if (!connections.ContainsKey(conn.connectionId))
            {
                // connection cannot be null here or conn.connectionId
                // would throw NRE
                connections[conn.connectionId] = conn;
                conn.SetHandlers(messageHandlers);
                return true;
            }
            // already a connection with this id
            return false;
        }

        public bool RemoveConnection(int connectionId)
        {
            return connections.Remove(connectionId);
        }

        public bool SendToClient(int connectionId, short msgType, MessageBase msg, CallbackHandler callback)
        {
            if(server.Active)
            {
                NetworkWriter writer = new NetworkWriter();
                writer.Write(msgType);

                int callbackId = 0;
                if (callback != null)
                {
                    callbackId = ++callbackIdIndex; // pre-increment to ensure that id 0 is never used.
                    callbacks.Add(callbackId, new CallbackData() { callback = callback, timeout = Time.realtimeSinceStartup + TIMEOUTDELAY });
                }

                writer.Write(callbackId);

                msg.Serialize(writer);

                return connections[connectionId].Send(writer.ToArray());
            }
            Debug.Log("Server.Send: not connected!", this);
            return false;
        }

        public bool SendToClient(int connectionId, short msgType, MessageBase msg)
        {
            return SendToClient(connectionId, msgType, msg, null);
        }

        public bool SendToClient(int connectionId, byte[] data)
        {
            if (server.Active)
            {
                return server.Send(connectionId, data);
            }
            Debug.Log("Server.Send: not connected!", this);
            return false;
        }

        public bool SendToAll(short msgType, MessageBase msg)
        {
            if (server.Active)
            {
                foreach(KeyValuePair<int, InsightNetworkConnection> conn in connections)
                {
                    SendToClient(conn.Key, msgType, msg);
                    //conn.Value.Send(msgType, msg);
                }
                return true;
            }
            Debug.Log("Server.Send: not connected!", this);
            return false;
        }

        public bool SendToAll(byte[] bytes)
        {
            if(server.Active)
            {
                foreach (var conn in connections)
                {
                    conn.Value.Send(bytes);
                }
                return true;
            }
            Debug.Log("Server.Send: not connected!", this);
            return false;
        }

        private void OnApplicationQuit()
        {
            if (logNetworkMessages) { Debug.Log("[InsightServer] Stopping Server"); }
            server.Stop();
        }

        //----------virtual handlers--------------//

        public virtual void OnConnected(InsightNetworkConnection conn)
        {
            if (logNetworkMessages) { Debug.Log("[InsightServer] - Client connected from: " + conn.address); }
        }

        public virtual void OnDisconnected(InsightNetworkConnection conn)
        {
            if (logNetworkMessages) { Debug.Log("[InsightServer] - OnDisconnected()"); }
        }

        public virtual void OnStartInsight()
        {
            if (logNetworkMessages) { Debug.Log("[InsightServer] - OnStartInsight()"); }
        }

        public virtual void OnStopInsight()
        {
            if (logNetworkMessages) { Debug.Log("[InsightServer] - OnStopInsight()"); }
        }
    }
}