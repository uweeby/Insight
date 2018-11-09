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

    #region Core
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
    }

    public void StopServer()
    {
        // stop the server when you don't need it anymore
        server.Stop();
    }

    // grab all new messages. do this in your Update loop.
    public void HandleNewMessages()
    {
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
                    HandleBytes(msg.data);
                    break;
                case Telepathy.EventType.Disconnected:
                    Disconnected(msg);
                    break;
            }
        }
    }

    public bool SendMsg(int connectionId, short msgType, MessageBase msg)
    {
        NetworkWriter writer = new NetworkWriter();
        msg.Serialize(writer);

        // pack message and send
        byte[] message = Protocol.PackMessage((ushort)msgType, writer.ToArray());
        return SendBytes(connectionId, message);
    }

    // protected because no one except NetworkConnection should ever send bytes directly to the client, as they
    // would be detected as some kind of message. send messages instead.
    protected virtual bool SendBytes(int connectionId, byte[] bytes)
    {
        if (logNetworkMessages) { Debug.Log("ConnectionSend con:" + connectionId + " bytes:" + BitConverter.ToString(bytes)); }

        if (bytes.Length > Transport.MaxPacketSize)
        {
            Debug.LogError("NetworkConnection:SendBytes cannot send packet larger than " + Transport.MaxPacketSize + " bytes");
            return false;
        }

        if (bytes.Length == 0)
        {
            // zero length packets getting into the packet queues are bad.
            Debug.LogError("NetworkConnection:SendBytes cannot send zero bytes");
            return false;
        }

        return server.Send(connectionId, bytes);
    }

    // handle this message
    // note: original HLAPI HandleBytes function handled >1 message in a while loop, but this wasn't necessary
    //       anymore because NetworkServer/NetworkClient.Update both use while loops to handle >1 data events per
    //       frame already.
    //       -> in other words, we always receive 1 message per Receive call, never two.
    //       -> can be tested easily with a 1000ms send delay and then logging amount received in while loops here
    //          and in NetworkServer/Client Update. HandleBytes already takes exactly one.
    protected void HandleBytes(byte[] buffer)
    {
        // unpack message
        ushort msgType;
        byte[] content;
        if (Protocol.UnpackMessage(buffer, out msgType, out content))
        {
            //if (logNetworkMessages) { Debug.Log("ConnectionRecv con:" + connectionId + " msgType:" + msgType + " content:" + BitConverter.ToString(content)); }
            if (logNetworkMessages) { Debug.Log(" msgType:" + msgType + " content:" + BitConverter.ToString(content)); }

            InsightNetworkMessageDelegate msgDelegate;
            if (messageHandlers.TryGetValue((short)msgType, out msgDelegate))
            {
                // create message here instead of caching it. so we can add it to queue more easily.
                InsightNetworkMessage msg = new InsightNetworkMessage();
                msg.msgType = (short)msgType;
                msg.reader = new NetworkReader(content);
                msg.conn = this;

                msgDelegate(msg);
                //lastMessageTime = Time.time;
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
    #endregion
}