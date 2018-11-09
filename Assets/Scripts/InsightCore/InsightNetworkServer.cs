using Mirror;
using System;
using System.Collections.Generic;
using Telepathy;
using UnityEngine;

public class InsightNetworkServer
{
    public int port;
    public int serverHostId = -1;
    public bool logNetworkMessages;

    Telepathy.Server server;

    Dictionary<int, InsightNetworkConnection> clientConnections;
    Dictionary<short, InsightNetworkMessageDelegate> messageHandlers;

    public Action<Message> Connected { get; internal set; }
    public Action<Message> Disconnected { get; internal set; }

    public delegate void OnConnect(string msg);
    public delegate void OnDisconnect(string msg);

    public InsightNetworkServer()
    {
        Application.runInBackground = true;

        // create and start the server
        server = new Telepathy.Server();

        clientConnections = new Dictionary<int, InsightNetworkConnection>();

        // use Debug.Log functions for Telepathy so we can see it in the console
        Telepathy.Logger.LogMethod = Debug.Log;
        Telepathy.Logger.LogWarningMethod = Debug.LogWarning;
        Telepathy.Logger.LogErrorMethod = Debug.LogError;

        messageHandlers = new Dictionary<short, InsightNetworkMessageDelegate>();
    }

    public void StartServer(int Port)
    {
        port = Port;
        server.Start(Port);
        serverHostId = 0;
    }

    public void StopServer()
    {
        // stop the server when you don't need it anymore
        server.Stop();
        serverHostId = -1;
    }

    // grab all new messages. do this in your Update loop.
    public void HandleNewMessages()
    {
        if (serverHostId == -1)
            return;

        Telepathy.Message msg;
        while (server.GetNextMessage(out msg))
        {
            switch (msg.eventType)
            {
                case Telepathy.EventType.Connected:
                    //OnClientConnect(msg);
                    Connected(msg);
                    break;
                case Telepathy.EventType.Data:
                    HandleData(msg.connectionId, msg.data, 0);
                    break;
                case Telepathy.EventType.Disconnected:
                    Disconnected(msg);
                    break;
            }
        }
    }

    void HandleData(int connectionId, byte[] data, byte error)
    {
        InsightNetworkConnection conn;
        if (clientConnections.TryGetValue(connectionId, out conn))
        {
            OnData(conn, data);
        }
        else
        {
            Debug.LogError("HandleData Unknown connectionId:" + connectionId);
        }
    }

    void OnData(InsightNetworkConnection conn, byte[] data)
    {
        conn.TransportReceive(data);
    }

    public bool Send(int connectionId, byte[] data)
    {
        return server.Send(connectionId, data);
    }

    public void RegisterHandler(short msgType, InsightNetworkMessageDelegate handler)
    {
        if (messageHandlers.ContainsKey(msgType))
        {
            //if (LogFilter.Debug) { Debug.Log("NetworkConnection.RegisterHandler replacing " + msgType); }
            Debug.Log("NetworkConnection.RegisterHandler replacing " + msgType);
        }
        messageHandlers[msgType] = handler;
    }

    public string GetConnectionInfo(int connectionId)
    {
        string address;
        server.GetConnectionInfo(connectionId, out address);
        return address;
    }

    public bool AddConnection(InsightNetworkConnection conn)
    {
        if (!clientConnections.ContainsKey(conn.connectionId))
        {
            // connection cannot be null here or conn.connectionId
            // would throw NRE
            clientConnections[conn.connectionId] = conn;
            conn.SetHandlers(messageHandlers);
            return true;
        }
        // already a connection with this id
        return false;
    }

    public bool RemoveConnection(int connectionId)
    {
        return clientConnections.Remove(connectionId);
    }

    private void OnApplicationQuit()
    {
        server.Stop();
    }
}