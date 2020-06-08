using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Insight
{
    public class InsightNetworkConnection : IDisposable
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(InsightNetworkConnection));

        Dictionary<int, InsightNetworkMessageDelegate> m_MessageHandlers;

        public int hostId = -1;
        public int connectionId = -1;
        public bool isReady;
        public string address;
        public float lastMessageTime;
        public bool logNetworkMessages;
        public bool isConnected { get { return hostId != -1; } }

        InsightClient client;
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
        }

        internal void SetHandlers(Dictionary<int, InsightNetworkMessageDelegate> handlers)
        {
            m_MessageHandlers = handlers;
        }

        public bool InvokeHandlerNoData(short msgType)
        {
            return InvokeHandler(msgType, null);
        }

        public bool InvokeHandler(short msgType, NetworkReader reader)
        {
            InsightNetworkMessageDelegate msgDelegate;
            if (m_MessageHandlers.TryGetValue(msgType, out msgDelegate))
            {
                InsightNetworkMessage message = new InsightNetworkMessage();
                message.msgType = msgType;
                message.reader = reader;

                msgDelegate(message);
                return true;
            }
            Debug.LogError("NetworkConnection InvokeHandler no handler for " + msgType);
            return false;
        }

        public bool InvokeHandler(InsightNetworkMessage netMsg)
        {
            InsightNetworkMessageDelegate msgDelegate;
            if (m_MessageHandlers.TryGetValue(netMsg.msgType, out msgDelegate))
            {
                msgDelegate(netMsg);
                return true;
            }
            return false;
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

        public virtual bool Send(byte[] bytes)
        {
            return SendBytes(bytes);
        }

        // protected because no one except NetworkConnection should ever send bytes directly to the client, as they
        // would be detected as some kind of message. send messages instead.
        protected virtual bool SendBytes(byte[] bytes)
        {
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

        public virtual void TransportReceive(ArraySegment<byte> data)
        {
            // unpack message
            NetworkReader reader = new NetworkReader(data);

            if (GetActiveInsight().UnpackMessage(reader, out int msgType))
            {
                // logging TODO: Replce all Insight logging with loggers like Mirror
                //if (logger.LogEnabled()) logger.Log("ConnectionRecv " + this + " msgType:" + msgType + " content:" + BitConverter.ToString(buffer.Array, buffer.Offset, buffer.Count));

                int callbackId = reader.ReadInt32();

                // try to invoke the handler for that message
                InsightNetworkMessageDelegate msgDelegate;
                if (m_MessageHandlers.TryGetValue(msgType, out msgDelegate))
                {
                    // create message here instead of caching it. so we can add it to queue more easily.
                    InsightNetworkMessage msg = new InsightNetworkMessage(this, callbackId);
                    msg.msgType = msgType;
                    msg.reader = reader;

                    msgDelegate(msg);
                    lastMessageTime = Time.time;
                }
            }

            else
            {
                //NOTE: this throws away the rest of the buffer. Need moar error codes
                Debug.LogError("Unknown message ID " + msgType + " connId:" + connectionId);
            }
        }

        protected virtual bool TransportSend(byte[] bytes, out byte error)
        {
            error = 0;
            if (client != null)
            {
                client.Send(bytes);
                return true;
            }
            else if (server != null)
            {
                server.SendToClient(connectionId, bytes);
                return true;
            }
            return false;
        }

        public InsightCommon GetActiveInsight()
        {
            if(client != null)
            {
                return client;
            }
            else
            {
                return server;
            }
        }
    }

    public class InsightNetworkMessage
    {
        public int msgType;
        InsightNetworkConnection conn;
        public NetworkReader reader;
        public int callbackId { get; protected set; }
        public int connectionId { get { return conn.connectionId; } }

        public InsightNetworkMessage()
        {

        }

        public InsightNetworkMessage(InsightNetworkConnection conn)
        {
            this.conn = conn;
        }

        public InsightNetworkMessage(InsightNetworkConnection conn, int callbackId)
        {
            this.callbackId = callbackId;
            this.conn = conn;
        }

        public TMsg ReadMessage<TMsg>() where TMsg : MessageBase, new()
        {
            TMsg msg = new TMsg();
            msg.Deserialize(reader);
            return msg;
        }

        public void ReadMessage<TMsg>(TMsg msg) where TMsg : MessageBase
        {
            msg.Deserialize(reader);
        }

        public void Reply()
        {
            Reply(new EmptyMsg());
        }

        public void Reply(MessageBase msg)
        {
            NetworkWriter writer = new NetworkWriter();
            int msgType = conn.GetActiveInsight().GetId(default(MessageBase) != null ? typeof(MessageBase) : msg.GetType());
            writer.WriteUInt16((ushort)msgType);

            //var writer = new NetworkWriter();
            //writer.WriteInt16(msgId);
            writer.WriteInt32(callbackId);
            msg.Serialize(writer);

            conn.Send(writer.ToArray());
        }
    }

    // Handles network messages on client and server
    public delegate void InsightNetworkMessageDelegate(InsightNetworkMessage netMsg);
}
