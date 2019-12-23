using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Insight
{
    public class InsightServer : InsightCommon
    {
        protected int serverHostId = -1; //-1 = never connected, 0 = disconnected, 1 = connected
        protected Dictionary<int, InsightNetworkConnection> connections = new Dictionary<int, InsightNetworkConnection>();
        protected List<SendToAllFinishedCallbackData> sendToAllFinishedCallbacks = new List<SendToAllFinishedCallbackData>();

        Transport _transport;
        public virtual Transport transport
        {
            get
            {
                _transport = _transport ?? GetComponent<Transport>();
                if (_transport == null)
                    Debug.LogWarning("InsightServer has no Transport component. Networking won't work without a Transport");
                return _transport;
            }
        }

        public virtual void Start()
        {
            if(DontDestroy)
            {
                DontDestroyOnLoad(this);
            }

            Application.runInBackground = true;

            transport.OnServerConnected.AddListener(HandleConnect);
            transport.OnServerDisconnected.AddListener(HandleDisconnect);
            transport.OnServerDataReceived.AddListener(HandleData);
            transport.OnServerError.AddListener(OnError);

            if (AutoStart)
            {
                StartInsight();
            }
        }

        public virtual void Update()
        {
            CheckCallbackTimeouts();
        }

        public override void StartInsight()
        {
            if (logNetworkMessages) { Debug.Log("[InsightServer] - Start"); }
            transport.ServerStart();
            serverHostId = 0;

            connectState = ConnectState.Connected;

            OnStartInsight();
        }

        public override void StopInsight()
        {
            connections.Clear();

            // stop the server when you don't need it anymore
            transport.ServerStop();
            serverHostId = -1;

            connectState = ConnectState.Disconnected;

            OnStopInsight();
        }

        void HandleConnect(int connectionId)
        {
            if (logNetworkMessages) { Debug.Log("[InsightServer] - Client connected connectionID: " + connectionId, this); }

            // get ip address from connection
            string address = GetConnectionInfo(connectionId);

            // add player info
            InsightNetworkConnection conn = new InsightNetworkConnection();
            conn.Initialize(this, address, serverHostId, connectionId);
            AddConnection(conn);
        }

        void HandleDisconnect(int connectionId)
        {
            if (logNetworkMessages) { Debug.Log("[InsightServer] - Client disconnected connectionID: " + connectionId, this); }

            InsightNetworkConnection conn;
            if (connections.TryGetValue(connectionId, out conn))
            {
                conn.Disconnect();
                RemoveConnection(connectionId);
            }
        }

        void HandleData(int connectionId, ArraySegment<byte> data, int i)
        {
            NetworkReader reader = new NetworkReader(data);
            short msgType = reader.ReadInt16();
            int callbackId = reader.ReadInt32();
            InsightNetworkConnection insightNetworkConnection;
            if (!connections.TryGetValue(connectionId, out insightNetworkConnection))
            {
                Debug.LogError("HandleData: Unknown connectionId: " + connectionId, this);
                return;
            }

            if (callbacks.ContainsKey(callbackId))
            {
                InsightNetworkMessage msg = new InsightNetworkMessage(insightNetworkConnection, callbackId) { msgType = msgType, reader = reader };
                callbacks[callbackId].callback.Invoke(CallbackStatus.Ok, msg);
                callbacks.Remove(callbackId);

                CheckForFinishedCallback(callbackId);
            }
            else
            {
                insightNetworkConnection.TransportReceive(data);
            }
        }

        void OnError(int connectionId, Exception exception)
        {
            // TODO Let's discuss how we will handle errors
            Debug.LogException(exception);
        }

        public string GetConnectionInfo(int connectionId)
        {
            return transport.ServerGetClientAddress(connectionId);
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

        public bool SendToClient(int connectionId, short msgType, MessageBase msg, CallbackHandler callback = null)
        {
            if (transport.ServerActive())
            {
                NetworkWriter writer = new NetworkWriter();
                writer.WriteInt16(msgType);

                int callbackId = 0;
                if (callback != null)
                {
                    callbackId = ++callbackIdIndex; // pre-increment to ensure that id 0 is never used.
                    callbacks.Add(callbackId, new CallbackData() { callback = callback, timeout = Time.realtimeSinceStartup + callbackTimeout });
                }

                writer.WriteInt32(callbackId);

                msg.Serialize(writer);

                return connections[connectionId].Send(writer.ToArray());
            }
            Debug.LogError("Server.Send: not connected!", this);
            return false;
        }

        public bool SendToClient(int connectionId, short msgType, MessageBase msg)
        {
            return SendToClient(connectionId, msgType, msg, null);
        }

        public bool SendToClient(int connectionId, byte[] data)
        {
            if (transport.ServerActive())
            {
                return transport.ServerSend(new List<int> {connectionId}, 0, new ArraySegment<byte>(data));
            }
            Debug.LogError("Server.Send: not connected!", this);
            return false;
        }

        public bool SendToAll(short msgType, MessageBase msg, CallbackHandler callback, SendToAllFinishedCallbackHandler finishedCallback)
        {
            if (transport.ServerActive())
            {
                SendToAllFinishedCallbackData finishedCallbackData = new SendToAllFinishedCallbackData() { requiredCallbackIds = new HashSet<int>() };

                foreach (KeyValuePair<int, InsightNetworkConnection> conn in connections)
                {
                    SendToClient(conn.Key, msgType, msg, callback);
                    finishedCallbackData.requiredCallbackIds.Add(callbackIdIndex);
                }

                // you can't have _just_ the finishedCallback, although you _can_ have just
                // "normal" callback. 
                if (finishedCallback != null && callback != null)
                {
                    finishedCallbackData.callback = finishedCallback;
                    finishedCallbackData.timeout = Time.realtimeSinceStartup + callbackTimeout;
                    sendToAllFinishedCallbacks.Add(finishedCallbackData);
                }
                return true;
            }
            Debug.LogError("Server.Send: not connected!", this);
            return false;
        }

        public bool SendToAll(short msgType, MessageBase msg, CallbackHandler callback)
        {
            return SendToAll(msgType, msg, callback, null);
        }

        public bool SendToAll(short msgType, MessageBase msg)
        {
            return SendToAll(msgType, msg, null, null);
        }

        public bool SendToAll(byte[] bytes)
        {
            if (transport.ServerActive())
            {
                foreach (var conn in connections)
                {
                    conn.Value.Send(bytes);
                }
                return true;
            }
            Debug.LogError("Server.Send: not connected!", this);
            return false;
        }

        void OnApplicationQuit()
        {
            if (logNetworkMessages) { Debug.Log("[InsightServer] Stopping Server"); }
            transport.ServerStop();
        }

        void CheckForFinishedCallback(int callbackId)
        {
            foreach (var item in sendToAllFinishedCallbacks)
            {
                if (item.requiredCallbackIds.Contains(callbackId)) item.callbacks++;
                if (item.callbacks >= item.requiredCallbackIds.Count)
                {
                    item.callback.Invoke(CallbackStatus.Ok);
                    sendToAllFinishedCallbacks.Remove(item);
                    return;
                }
            }
        }

        protected override void CheckCallbackTimeouts()
        {
            base.CheckCallbackTimeouts();
            foreach (var item in sendToAllFinishedCallbacks)
            {
                if (item.timeout < Time.realtimeSinceStartup)
                {
                    item.callback.Invoke(CallbackStatus.Timeout);
                    sendToAllFinishedCallbacks.Remove(item);
                    return;
                }
            }
        }

        ////----------virtual handlers--------------//
        public virtual void OnStartInsight()
        {
            if (logNetworkMessages) { Debug.Log("[InsightServer] - Server started listening"); }
        }

        public virtual void OnStopInsight()
        {
            if (logNetworkMessages) { Debug.Log("[InsightServer] - Server stopping"); }
        }
    }
}
