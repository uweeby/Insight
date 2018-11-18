using Mirror;
using System;
using System.Collections.Generic;
using Telepathy;
using UnityEngine;

namespace Insight
{
    public class InsightClient : MonoBehaviour
    {
        public int clientID = -1;
        public int connectionID = 0;

        public string networkAddress = "localhost";
        public int networkPort = 5000;
        public bool logNetworkMessages;

        InsightNetworkConnection insightNetworkConnection;

        Client client = new Client();

        Dictionary<short, InsightNetworkMessageDelegate> messageHandlers;

        protected enum ConnectState
        {
            None,
            Connecting,
            Connected,
            Disconnected,
        }
        protected ConnectState connectState = ConnectState.None;

        public bool isConnected { get { return connectState == ConnectState.Connected; } }

        public virtual void Start()
        {
            DontDestroyOnLoad(gameObject);

            // use Debug.Log functions for Telepathy so we can see it in the console
            Telepathy.Logger.LogMethod = Debug.Log;
            Telepathy.Logger.LogWarningMethod = Debug.LogWarning;
            Telepathy.Logger.LogErrorMethod = Debug.LogError;

            messageHandlers = new Dictionary<short, InsightNetworkMessageDelegate>();

            insightNetworkConnection = new InsightNetworkConnection();
            insightNetworkConnection.Initialize(this, networkAddress, clientID, connectionID);
        }
        void Update()
        {
            HandleNewMessages();
        }

        public void StartClient(string Address, int Port)
        {
            networkAddress = Address;
            networkPort = Port;

            Debug.Log("Connecting to Insight Server: " + Address + ":" + Port);
            client.Connect(Address, Port);

            OnClientStart();
        }

        public void StopClient()
        {
            Debug.Log("Disconnecting from Insight Server");
            client.Disconnect();

            OnClientStop();
        }

        public void HandleNewMessages()
        {
            if (!client.Connected)
                return;

            // grab all new messages. do this in your Update loop.
            Message msg;
            while (client.GetNextMessage(out msg))
            {
                switch (msg.eventType)
                {
                    case Telepathy.EventType.Connected:
                        //Debug.Log("Connected to Insight Server");
                        connectState = ConnectState.Connected;
                        OnConnected(msg);
                        break;
                    case Telepathy.EventType.Data:
                        //Debug.Log("Data: " + BitConverter.ToString(msg.data));
                        HandleBytes(msg.data);
                        break;
                    case Telepathy.EventType.Disconnected:
                        //Debug.Log("Disconnected");
                        connectState = ConnectState.Disconnected;
                        OnDisconnected(msg);
                        break;
                }
            }
        }

        public bool Send(byte[] data)
        {
            if (client.Connected)
            {
                return SendBytes(connectionID, data);
            }
            Debug.Log("Client.Send: not connected!");
            return false;
        }

        public bool SendMsg(short msgType, MessageBase msg)
        {
            NetworkWriter writer = new NetworkWriter();
            msg.Serialize(writer);

            // pack message and send
            byte[] message = Protocol.PackMessage((ushort)msgType, writer.ToArray());
            return SendBytes(0, message);
        }

        public bool SendMsg(short msgType, MessageBase msg, bool Callback)
        {
            if(!Callback)
            {
                SendMsg(msgType, msg);
            }

            NetworkWriter writer = new NetworkWriter();
            msg.Serialize(writer);

            // pack message and send
            byte[] message = Protocol.PackMessage((ushort)msgType, writer.ToArray());
            return SendBytes(0, message);
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

            return client.Send(bytes);
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
                    //msg.conn = this;

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

        private void OnApplicationQuit()
        {
            StopClient();
        }

        //------------Virtual Handlers-------------
        public virtual void OnConnected(Message msg)
        {

        }

        public virtual void OnDisconnected(Message msg)
        {

        }

        public virtual void OnClientStart()
        {

        }

        public virtual void OnClientStop()
        {

        }
    }
}