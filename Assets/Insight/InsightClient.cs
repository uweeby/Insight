using Insight;
using Mirror;
using System.Collections.Generic;
using Telepathy;
using UnityEngine;
using UnityEngine.Events;

public class InsightClient : InsightCommon
{
    [HideInInspector]
    public UnityEvent OnConnectedEvent;

    public bool AutoReconnect = true;
    protected int clientID = -1;
    protected int connectionID = 0;

    InsightNetworkConnection insightNetworkConnection;

    Transport _transport;
    public virtual Transport transport
    {
        get
        {
            _transport = _transport ?? GetComponent<Transport>();
            if (_transport == null)
                Debug.LogWarning("InsightClient has no Transport component. Networking won't work without a Transport");
            return _transport;
        }
    }

    public float ReconnectDelayInSeconds = 5f;
    private float _reconnectTimer;

    public virtual void Start()
    {
        DontDestroyOnLoad(gameObject);
        Application.runInBackground = true;

        messageHandlers = new Dictionary<short, InsightNetworkMessageDelegate>();

        if (AutoStart)
        {
            StartInsight();
        }
    }

    public virtual void Update()
    {
        CheckConnection();

        HandleNewMessages();

        CheckCallbackTimeouts();
    }

    public void StartInsight(string Address)
    {
        networkAddress = Address;

        StartInsight();
    }

    public override void StartInsight()
    {
        transport.ClientConnect(networkAddress);
        clientID = 0;
        insightNetworkConnection = new InsightNetworkConnection();
        insightNetworkConnection.Initialize(this, networkAddress, clientID, connectionID);
        insightNetworkConnection.SetHandlers(messageHandlers);

        insightNetworkConnection.RegisterHandler((short)MsgType.Connect, OnClientConnect);
        insightNetworkConnection.RegisterHandler((short)MsgType.Disconnect, OnClientDisconnect);

        OnStartInsight();
        _reconnectTimer = Time.realtimeSinceStartup + ReconnectDelayInSeconds;
    }

    public override void StopInsight()
    {
        transport.ClientDisconnect();
        OnStopInsight();
    }

    //public bool IsConnecting()
    //{
    //    return transport.ClientConnected();
    //}

    private void CheckConnection()
    {
        if (AutoReconnect)
        {
            if (!isConnected && (_reconnectTimer < Time.time))
            {
                if (logNetworkMessages) { Debug.Log("[InsightClient] - Trying to reconnect..."); }
                _reconnectTimer = Time.realtimeSinceStartup + ReconnectDelayInSeconds;
                StartInsight();
            }
        }
    }

    public void HandleNewMessages()
    {
        if (clientID == -1)
            return;

        TransportEvent transportEvent;
        byte[] data;
        while (transport.ClientGetNextMessage(out transportEvent, out data))
        {
            switch (transportEvent)
            {
                case TransportEvent.Connected:
                    if (insightNetworkConnection != null)
                    {
                        connectState = ConnectState.Connected;
                        insightNetworkConnection.InvokeHandlerNoData((short)MsgType.Connect);
                    }
                    else Debug.LogError("Skipped Connect message handling because m_Connection is null.");

                    break;
                case TransportEvent.Data:
                    HandleBytes(data);

                    //if (insightNetworkConnection != null)
                    //{
                    //    insightNetworkConnection.TransportReceive(data);
                    //}
                    //else Debug.LogError("Skipped Data message handling because m_Connection is null.");

                    break;
                case TransportEvent.Disconnected:
                    connectState = ConnectState.Disconnected;

                    if (insightNetworkConnection != null)
                    {
                        insightNetworkConnection.InvokeHandlerNoData((short)MsgType.Disconnect);
                    }
                    break;
            }
        }
    }

    public void Send(byte[] data)
    {
        transport.ClientSend(0, data);
    }

    public void Send(short msgType, MessageBase msg)
    {
        Send(msgType, msg, null);
    }

    public void Send(short msgType, MessageBase msg, CallbackHandler callback)
    {
        if (!transport.ClientConnected())
        {
            Debug.LogError("[InsightClient] - Client not connected!");
            return;
        }

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
        transport.ClientSend(0, writer.ToArray());
    }

    void HandleCallbackHandler(CallbackStatus status, NetworkReader reader)
    {
    }

    protected void HandleBytes(byte[] buffer)
    {
        InsightNetworkMessageDelegate msgDelegate;
        NetworkReader reader = new NetworkReader(buffer);
        var msgType = reader.ReadInt16();
        var callbackId = reader.ReadInt32();
        var msg = new InsightNetworkMessage(insightNetworkConnection, callbackId) { msgType = msgType, reader = reader };

        if (callbacks.ContainsKey(callbackId))
        {
            callbacks[callbackId].callback.Invoke(CallbackStatus.Ok, msg);
            callbacks.Remove(callbackId);
        }
        else if (messageHandlers.TryGetValue(msgType, out msgDelegate))
        {
            msgDelegate(msg);
        }
        else
        {
            //NOTE: this throws away the rest of the buffer. Need moar error codes
            Debug.LogError("Unknown message ID " + msgType);// + " connId:" + connectionId);
        }
    }

    private void OnApplicationQuit()
    {
        if (logNetworkMessages) { Debug.Log("[InsightClient] Stopping Client"); }
        StopInsight();
    }

    //------------Virtual Handlers-------------
    public virtual void OnClientConnect(InsightNetworkMessage netMsg)
    {
        if (logNetworkMessages) { Debug.Log("[InsightClient] - Connected to Insight Server"); }
    }

    public virtual void OnClientDisconnect(InsightNetworkMessage netMsg)
    {
        if (logNetworkMessages) { Debug.Log("[InsightClient] - OnDisconnected()"); }
    }

    public virtual void OnStartInsight()
    {
        if (logNetworkMessages) { Debug.Log("[InsightClient] - Connecting to Insight Server: " + networkAddress + ":"); } //+ networkPort); }
    }

    public virtual void OnStopInsight()
    {
        if (logNetworkMessages) { Debug.Log("[InsightClient] - Disconnecting from Insight Server"); }
    }
}
