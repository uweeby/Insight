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

        public bool InvokeHandler(int msgType, NetworkReader reader, int channelId = 0)
        {
            if (m_MessageHandlers.TryGetValue(msgType, out InsightNetworkMessageDelegate msgDelegate))
            {
                msgDelegate(this, reader, channelId);
                return true;
            }
            if (logger.LogEnabled()) logger.Log("Unknown message ID " + msgType + " " + this + ". May be due to no existing RegisterHandler for this message.");
            return false;
        }

        public virtual bool Send(byte[] bytes)
        {
            return SendBytes(bytes);
        }

        // protected because no one except NetworkConnection should ever send bytes directly to the client, as they
        // would be detected as some kind of message. send messages instead.
        protected virtual bool SendBytes(byte[] bytes)
        {
            //Currently no support for transport channels in Insight.
            if (bytes.Length > GetActiveInsight().transport.GetMaxPacketSize(0))
            {
                logger.LogError("NetworkConnection:SendBytes cannot send packet larger than " + int.MaxValue + " bytes");
                return false;
            }

            if (bytes.Length == 0)
            {
                // zero length packets getting into the packet queues are bad.
                logger.LogError("NetworkConnection:SendBytes cannot send zero bytes");
                return false;
            }

            return TransportSend(bytes);
        }

        public virtual void TransportReceive(ArraySegment<byte> data, int channelId)
        {
            // unpack message
            NetworkReader reader = new NetworkReader(data);

            if (GetActiveInsight().UnpackMessage(reader, out int msgType))
            {
                logger.Log("ConnectionRecv " + this + " msgType:" + msgType + " content:" + BitConverter.ToString(data.Array, data.Offset, data.Count));

                int callbackId = reader.ReadInt32();

                // try to invoke the handler for that message
                if (InvokeHandler(msgType, reader, channelId))
                {
                    lastMessageTime = Time.time;
                }

                else
                {
                    //NOTE: this throws away the rest of the buffer. Need moar error codes
                    logger.LogError("Unknown message ID " + msgType + " connId:" + connectionId);
                }
            }
        }

        protected virtual bool TransportSend(byte[] bytes)
        {
            if (client != null)
            {
                client.Send(bytes);
                return true;
            }
            else if (server != null)
            {
                return server.SendToClient(connectionId, bytes);
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

        public void Reply()
        {
            Reply(new Message());
        }

        public void Reply(Message msg)
        {
            NetworkWriter writer = new NetworkWriter();
            int msgType = GetActiveInsight().GetId(default(Message) != null ? typeof(Message) : msg.GetType());
            writer.WriteUInt16((ushort)msgType);

            writer.WriteInt32(msg.callbackId);
            msg.Serialize(writer);

            Send(writer.ToArray());
        }
    }

    // Handles network messages on client and server
    public delegate void InsightNetworkMessageDelegate(InsightNetworkConnection conn, NetworkReader reader, int channelId);
}
