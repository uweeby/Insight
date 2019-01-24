using Insight;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InsightServer : InsightCommon
{
    protected int serverHostId = -1;

    ITransport _transport;
    public virtual ITransport transport
    {
        get
        {
            _transport = _transport ?? GetComponent<ITransport>();
            if (_transport == null)
                Debug.LogWarning("InsightServer has no Transport component. Networking won't work without a Transport");
            return _transport;
        }
    }

    protected Dictionary<int, InsightNetworkConnection> connections;

    protected List<SendToAllFinishedCallbackData> sendToAllFinishedCallbacks = new List<SendToAllFinishedCallbackData>();

    public virtual void Start()
    {
        DontDestroyOnLoad(this);
        Application.runInBackground = true;

        // use Debug.Log functions for Telepathy so we can see it in the console
        Telepathy.Logger.LogMethod = Debug.Log;
        Telepathy.Logger.LogWarningMethod = Debug.LogWarning;
        Telepathy.Logger.LogErrorMethod = Debug.LogError;

        // create and start the server
        //server = new Server();

        connections = new Dictionary<int, InsightNetworkConnection>();

        messageHandlers = new Dictionary<short, InsightNetworkMessageDelegate>();

        if (AutoStart)
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

    // grab all new messages. do this in your Update loop.
    public void HandleNewMessages()
    {
        if (serverHostId == -1)
            return;

        //Message msg;
        //while (server.GetNextMessage(out msg))
        //{
        //    switch (msg.eventType)
        //    {
        //        case Telepathy.EventType.Connected:
        //            HandleConnect(msg);
        //            break;
        //        case Telepathy.EventType.Data:
        //            HandleData(msg.connectionId, msg.data, 0);
        //            break;
        //        case Telepathy.EventType.Disconnected:
        //            HandleDisconnect(msg);
        //            break;
        //    }
        //}

        int connectionId;
        TransportEvent transportEvent;
        byte[] data;
        while (transport.ServerGetNextMessage(out connectionId, out transportEvent, out data))
        {
            switch (transportEvent)
            {
                case TransportEvent.Connected:
                    //Debug.Log("NetworkServer loop: Connected");
                    HandleConnect(connectionId, 0);
                    break;
                case TransportEvent.Data:
                    //Debug.Log("NetworkServer loop: clientId: " + message.connectionId + " Data: " + BitConverter.ToString(message.data));
                    HandleData(connectionId, data, 0);
                    break;
                case TransportEvent.Disconnected:
                    //Debug.Log("NetworkServer loop: Disconnected");
                    HandleDisconnect(connectionId, 0);
                    break;
            }
        }
    }

    void HandleConnect(int connectionId, byte error)
    {
        if (logNetworkMessages) { Debug.Log("[InsightServer] - connectionID: " + connectionId, this); }

        // get ip address from connection
        string address = GetConnectionInfo(connectionId);

        // add player info
        InsightNetworkConnection conn = new InsightNetworkConnection();
        conn.Initialize(this, address, serverHostId, connectionId);
        AddConnection(conn);

        OnConnected(conn);
    }

    void HandleDisconnect(int connectionId, byte error)
    {
        InsightNetworkConnection conn;
        if (connections.TryGetValue(connectionId, out conn))
        {
            conn.Disconnect();
            RemoveConnection(connectionId);

            OnDisconnected(conn);
        }
    }

    void HandleData(int connectionId, byte[] data, byte error)
    {
        //InsightNetworkConnection conn;

        NetworkReader reader = new NetworkReader(data);
        var msgType = reader.ReadInt16();
        var callbackId = reader.ReadInt32();
        InsightNetworkConnection insightNetworkConnection;
        if (!connections.TryGetValue(connectionId, out insightNetworkConnection))
        {
            Debug.LogError("HandleData: Unknown connectionId: " + connectionId, this);
            return;
        }

        if (callbacks.ContainsKey(callbackId))
        {
            var msg = new InsightNetworkMessage(insightNetworkConnection, callbackId) { msgType = msgType, reader = reader };
            callbacks[callbackId].callback.Invoke(CallbackStatus.Ok, msg);
            callbacks.Remove(callbackId);

            CheckForFinishedCallback(callbackId);
        }
        else
        {
            insightNetworkConnection.TransportReceive(data);
        }
    }

    public string GetConnectionInfo(int connectionId)
    {
        string address;
        transport.GetConnectionInfo(connectionId, out address);
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
        if (transport.ServerActive())
        {
            NetworkWriter writer = new NetworkWriter();
            writer.Write(msgType);

            int callbackId = 0;
            if (callback != null)
            {
                callbackId = ++callbackIdIndex; // pre-increment to ensure that id 0 is never used.
                callbacks.Add(callbackId, new CallbackData() { callback = callback, timeout = Time.realtimeSinceStartup + callbackTimeout });
            }

            writer.Write(callbackId);

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
            return transport.ServerSend(connectionId, 0, data);
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

    private void OnApplicationQuit()
    {
        if (logNetworkMessages) { Debug.Log("[InsightServer] Stopping Server"); }
        transport.ServerStop();
    }

    private void CheckForFinishedCallback(int callbackId)
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
