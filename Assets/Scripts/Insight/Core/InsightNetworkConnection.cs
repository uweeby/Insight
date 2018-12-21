using Mirror;
using Insight;
using System;
using System.Collections.Generic;
using UnityEngine;

/*
* wire protocol is a list of :   size   |  msgType     | payload
*                               (short)  (variable)   (buffer)
*/
public class InsightNetworkConnection : IDisposable
{
    Dictionary<short, InsightNetworkMessageDelegate> m_MessageHandlers;

    public int hostId = -1;
    public int connectionId = -1;
    public bool isReady;
    public string address;
    public float lastMessageTime;
    public bool logNetworkMessages;
    public bool isConnected { get { return hostId != -1; } }

    Insight.InsightClient client;
    InsightServer server;

    public virtual void Initialize(InsightClient clientTransport, string networkAddress, int networkHostId, int networkConnectionId)
    {
        address = networkAddress;
        hostId = networkHostId;
        connectionId = networkConnectionId;
        client = clientTransport;
    }

    public virtual void Initialize(InsightServer serverTransport, string networkAddress, int networkHostId, int networkConnectionId)
    {
        address = networkAddress;
        hostId = networkHostId;
        connectionId = networkConnectionId;
        server = serverTransport;
    }

    ~InsightNetworkConnection()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        // Take yourself off the Finalization queue
        // to prevent finalization code for this object
        // from executing a second time.
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {

    }

    public void Disconnect()
    {
        isReady = false;

        // remove observers. original HLAPI has hostId check for that too.
        if (hostId != -1)
        {
            //RemoveObservers();
        }
    }

    internal void SetHandlers(Dictionary<short, InsightNetworkMessageDelegate> handlers)
    {
        m_MessageHandlers = handlers;
    }

    public void RegisterHandler(short msgType, InsightNetworkMessageDelegate handler)
    {
        if (m_MessageHandlers.ContainsKey(msgType))
        {
            Debug.Log("NetworkConnection.RegisterHandler replacing " + msgType);
        }
        m_MessageHandlers[msgType] = handler;
    }

    public void UnregisterHandler(short msgType)
    {
        m_MessageHandlers.Remove(msgType);
    }

    public virtual bool Send(short msgType, MessageBase msg)
    {
        NetworkWriter writer = new NetworkWriter();
        msg.Serialize(writer);

        // pack message and send
        byte[] message = Protocol.PackMessage((ushort)msgType, writer.ToArray());
        return SendBytes(message);
    }

    // protected because no one except NetworkConnection should ever send bytes directly to the client, as they
    // would be detected as some kind of message. send messages instead.
    protected virtual bool SendBytes(byte[] bytes)
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

        byte error;
        return TransportSend(bytes, out error);
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
            if (logNetworkMessages) { Debug.Log("ConnectionRecv con:" + connectionId + " msgType:" + msgType + " content:" + BitConverter.ToString(content)); }

            InsightNetworkMessageDelegate msgDelegate;
            if (m_MessageHandlers.TryGetValue((short)msgType, out msgDelegate))
            {
                // create message here instead of caching it. so we can add it to queue more easily.
                InsightNetworkMessage msg = new InsightNetworkMessage();
                msg.msgType = (short)msgType;
                msg.reader = new NetworkReader(content);
                msg.conn = this;

                msgDelegate(msg);
                lastMessageTime = Time.time;
            }
            else
            {
                //NOTE: this throws away the rest of the buffer. Need moar error codes
                Debug.LogError("Unknown message ID " + msgType + " connId:" + connectionId);
            }
        }
        else
        {
            Debug.LogError("HandleBytes UnpackMessage failed for: " + BitConverter.ToString(buffer));
        }
    }

    public virtual void TransportReceive(byte[] bytes)
    {
        HandleBytes(bytes);
    }

    public virtual bool TransportSend(byte[] bytes, out byte error)
    {
        error = 0;
        if (client != null)
        {
            client.Send(0, bytes);
            return true;
        }
        else if (server != null)
        {
            server.Send(connectionId, bytes);
            return true;
        }
        return false;
    }
}

public class InsightNetworkMessage
{
    public short msgType;
    public InsightNetworkConnection conn;
    public NetworkReader reader;

    public TMsg ReadMessage<TMsg>() where TMsg : MessageBase, new()
    {
        var msg = new TMsg();
        msg.Deserialize(reader);
        return msg;
    }

    public void ReadMessage<TMsg>(TMsg msg) where TMsg : MessageBase
    {
        msg.Deserialize(reader);
    }
}

// Handles network messages on client and server
public delegate void InsightNetworkMessageDelegate(InsightNetworkMessage netMsg);