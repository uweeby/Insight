using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

public class InsightNetworkClient
{
    public string address;
    public int port;
    public string AuthCode;

    public bool logNetworkMessages;

    Telepathy.Client client;

    Dictionary<short, NetworkMessageDelegate> m_MessageHandlers;

    #region Core
    public InsightNetworkClient()
    {
        Application.runInBackground = true;

        // create and connect the client
        client = new Telepathy.Client();

        // use Debug.Log functions for Telepathy so we can see it in the console
        Telepathy.Logger.LogMethod = Debug.Log;
        Telepathy.Logger.LogWarningMethod = Debug.LogWarning;
        Telepathy.Logger.LogErrorMethod = Debug.LogError;

        m_MessageHandlers = new Dictionary<short, NetworkMessageDelegate>();
    }
    public void StartClient(string Address, int Port)
    {
        address = Address;
        port = Port;

        Debug.Log("Connecting to Insight Server: " + Address + ":" + Port);
        client.Connect(Address, Port);
    }

    public void StopClient()
    {
        Debug.Log("Disconnecting from Insight Server");
        client.Disconnect();
    }

    public void HandleNewMessages()
    {
        if (!client.Connected)
            return;

        // grab all new messages. do this in your Update loop.
        Telepathy.Message msg;
        while (client.GetNextMessage(out msg))
        {
            switch (msg.eventType)
            {
                case Telepathy.EventType.Connected:
                    Debug.Log("Connected to Insight Server");
                    break;
                case Telepathy.EventType.Data:
                    //Debug.Log("Data: " + BitConverter.ToString(msg.data));
                    HandleBytes(msg.data);
                    break;
                case Telepathy.EventType.Disconnected:
                    Debug.Log("Disconnected");
                    break;
            }
        }
    }

    public bool SendMsg(short msgType, MessageBase msg)
    {
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

            NetworkMessageDelegate msgDelegate;
            if (m_MessageHandlers.TryGetValue((short)msgType, out msgDelegate))
            {
                // create message here instead of caching it. so we can add it to queue more easily.
                NetworkMessage msg = new NetworkMessage();
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

    public void RegisterHandler(short msgType, NetworkMessageDelegate handler)
    {
        if (m_MessageHandlers.ContainsKey(msgType))
        {
            //if (LogFilter.Debug) { Debug.Log("NetworkConnection.RegisterHandler replacing " + msgType); }
            Debug.Log("NetworkConnection.RegisterHandler replacing " + msgType);
        }
        m_MessageHandlers[msgType] = handler;
    }
    #endregion
}