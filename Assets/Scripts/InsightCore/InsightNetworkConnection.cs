using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

/*
* wire protocol is a list of :   size   |  msgType     | payload
*                               (short)  (variable)   (buffer)
*/
public class InsightNetworkConnection : IDisposable
{
    //NetworkIdentity m_PlayerController;
    //HashSet<NetworkIdentity> m_VisList = new HashSet<NetworkIdentity>();
    //public HashSet<NetworkIdentity> visList { get { return m_VisList; } }

    Dictionary<short, InsightNetworkMessageDelegate> m_MessageHandlers;

    //HashSet<uint> m_ClientOwnedObjects;

    public int hostId = -1;
    public int connectionId = -1;
    public bool isReady;
    public string address;
    public float lastMessageTime;
    //public NetworkIdentity playerController { get { return m_PlayerController; } }
    //public HashSet<uint> clientOwnedObjects { get { return m_ClientOwnedObjects; } }
    public bool logNetworkMessages;
    public bool isConnected { get { return hostId != -1; } }

    InsightNetworkClient client;
    InsightNetworkServer server;

    public virtual void Initialize(InsightNetworkClient clientTransport, string networkAddress, int networkHostId, int networkConnectionId)
    {
        address = networkAddress;
        hostId = networkHostId;
        connectionId = networkConnectionId;
        client = clientTransport;
    }

    public virtual void Initialize(InsightNetworkServer serverTransport, string networkAddress, int networkHostId, int networkConnectionId)
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
        //if (m_ClientOwnedObjects != null)
        //{
        //    foreach (var netId in m_ClientOwnedObjects)
        //    {
        //        var obj = NetworkServer.FindLocalObject(netId);
        //        if (obj != null)
        //        {
        //            obj.GetComponent<NetworkIdentity>().ClearClientOwner();
        //        }
        //    }
        //}
        //m_ClientOwnedObjects = null;
    }

    public void Disconnect()
    {
        // don't clear address so we can still access it in NetworkManager.OnServerDisconnect
        // => it's reset in Initialize anyway and there is no address empty check anywhere either
        //address = "";

        // set not ready and handle clientscene disconnect in any case
        // (might be client or host mode here)
        isReady = false;

        // client? then stop transport
        //if (Transport.layer.ClientConnected())
        //{
        //    Transport.layer.ClientDisconnect();
        //}
        //// server? then disconnect that client
        //else if (Transport.layer.ServerActive())
        //{
        //    Transport.layer.ServerDisconnect(connectionId);
        //}

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

    //public bool InvokeHandlerNoData(short msgType)
    //{
    //    return InvokeHandler(msgType, null);
    //}

    //public bool InvokeHandler(short msgType, NetworkReader reader)
    //{
    //    InsightNetworkMessageDelegate msgDelegate;
    //    if (m_MessageHandlers.TryGetValue(msgType, out msgDelegate))
    //    {
    //        InsightNetworkMessage message = new InsightNetworkMessage();
    //        message.msgType = msgType;
    //        message.conn = this;
    //        message.reader = reader;

    //        msgDelegate(message);
    //        return true;
    //    }
    //    Debug.LogError("NetworkConnection InvokeHandler no handler for " + msgType);
    //    return false;
    //}

    //public bool InvokeHandler(InsightNetworkMessage netMsg)
    //{
    //    InsightNetworkMessageDelegate msgDelegate;
    //    if (m_MessageHandlers.TryGetValue(netMsg.msgType, out msgDelegate))
    //    {
    //        msgDelegate(netMsg);
    //        return true;
    //    }
    //    return false;
    //}

    public void RegisterHandler(short msgType, InsightNetworkMessageDelegate handler)
    {
        if (m_MessageHandlers.ContainsKey(msgType))
        {
            if (LogFilter.logDebug) { Debug.Log("NetworkConnection.RegisterHandler replacing " + msgType); }
        }
        m_MessageHandlers[msgType] = handler;
    }

    public void UnregisterHandler(short msgType)
    {
        m_MessageHandlers.Remove(msgType);
    }

    public virtual bool Send(short msgType, MessageBase msg, int channelId = Channels.DefaultReliable)
    {
        if(client != null)
        {
            client.SendMsg(msgType, msg);
            return true;
        }

        if(server != null)
        {
            server.SendMsg(connectionId, msgType, msg);
            return true;
        }
        return false;

        //NetworkWriter writer = new NetworkWriter();
        //msg.Serialize(writer);

        //// pack message and send
        //byte[] message = Protocol.PackMessage((ushort)msgType, writer.ToArray());
        //return SendBytes(message, channelId);
    }

    //// protected because no one except NetworkConnection should ever send bytes directly to the client, as they
    //// would be detected as some kind of message. send messages instead.
    //protected virtual bool SendBytes(byte[] bytes, int channelId = Channels.DefaultReliable)
    //{
    //    if (logNetworkMessages) { Debug.Log("ConnectionSend con:" + connectionId + " bytes:" + BitConverter.ToString(bytes)); }

    //    if (bytes.Length > Transport.MaxPacketSize)
    //    {
    //        Debug.LogError("NetworkConnection:SendBytes cannot send packet larger than " + Transport.MaxPacketSize + " bytes");
    //        return false;
    //    }

    //    if (bytes.Length == 0)
    //    {
    //        // zero length packets getting into the packet queues are bad.
    //        Debug.LogError("NetworkConnection:SendBytes cannot send zero bytes");
    //        return false;
    //    }

    //    byte error;
    //    return TransportSend(channelId, bytes, out error);
    //}

    //// handle this message
    //// note: original HLAPI HandleBytes function handled >1 message in a while loop, but this wasn't necessary
    ////       anymore because NetworkServer/NetworkClient.Update both use while loops to handle >1 data events per
    ////       frame already.
    ////       -> in other words, we always receive 1 message per Receive call, never two.
    ////       -> can be tested easily with a 1000ms send delay and then logging amount received in while loops here
    ////          and in NetworkServer/Client Update. HandleBytes already takes exactly one.
    //protected void HandleBytes(byte[] buffer)
    //{
    //    // unpack message
    //    ushort msgType;
    //    byte[] content;
    //    if (Protocol.UnpackMessage(buffer, out msgType, out content))
    //    {
    //        if (logNetworkMessages) { Debug.Log("ConnectionRecv con:" + connectionId + " msgType:" + msgType + " content:" + BitConverter.ToString(content)); }

    //        InsightNetworkMessageDelegate msgDelegate;
    //        if (m_MessageHandlers.TryGetValue((short)msgType, out msgDelegate))
    //        {
    //            // create message here instead of caching it. so we can add it to queue more easily.
    //            InsightNetworkMessage msg = new InsightNetworkMessage();
    //            msg.msgType = (short)msgType;
    //            msg.reader = new NetworkReader(content);
    //            msg.conn = this;

    //            msgDelegate(msg);
    //            lastMessageTime = Time.time;
    //        }
    //        else
    //        {
    //            //NOTE: this throws away the rest of the buffer. Need moar error codes
    //            Debug.LogError("Unknown message ID " + msgType + " connId:" + connectionId);
    //        }
    //    }
    //    else
    //    {
    //        Debug.LogError("HandleBytes UnpackMessage failed for: " + BitConverter.ToString(buffer));
    //    }
    //}

    //public override string ToString()
    //{
    //    return string.Format("hostId: {0} connectionId: {1} isReady: {2}", hostId, connectionId, isReady);
    //}

    //internal void AddToVisList(NetworkIdentity uv)
    //{
    //    m_VisList.Add(uv);

    //    // spawn uv for this conn
    //    NetworkServer.ShowForConnection(uv, this);
    //}

    //internal void RemoveFromVisList(NetworkIdentity uv, bool isDestroyed)
    //{
    //    m_VisList.Remove(uv);

    //    if (!isDestroyed)
    //    {
    //        // hide uv for this conn
    //        NetworkServer.HideForConnection(uv, this);
    //    }
    //}

    //internal void RemoveObservers()
    //{
    //    foreach (var uv in m_VisList)
    //    {
    //        uv.RemoveObserverInternal(this);
    //    }
    //    m_VisList.Clear();
    //}

    //public virtual void TransportReceive(byte[] bytes)
    //{
    //    HandleBytes(bytes);
    //}

    //public virtual bool TransportSend(int channelId, byte[] bytes, out byte error)
    //{
    //    error = 0;
    //    if (Transport.layer.ClientConnected())
    //    {
    //        client.SendMsg(channelId, bytes);
    //        return true;
    //    }
    //    else if (Transport.layer.ServerActive())
    //    {
    //        Transport.layer.ServerSend(connectionId, channelId, bytes);
    //        return true;
    //    }
    //    return false;
    //}

    //internal void AddOwnedObject(NetworkIdentity obj)
    //{
    //    if (m_ClientOwnedObjects == null)
    //    {
    //        m_ClientOwnedObjects = new HashSet<uint>();
    //    }
    //    m_ClientOwnedObjects.Add(obj.netId);
    //}

    //internal void RemoveOwnedObject(NetworkIdentity obj)
    //{
    //    if (m_ClientOwnedObjects == null)
    //    {
    //        return;
    //    }
    //    m_ClientOwnedObjects.Remove(obj.netId);
    //}
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

public class Channels
{
    public const int DefaultReliable = 0;
    public const int DefaultUnreliable = 1;
}