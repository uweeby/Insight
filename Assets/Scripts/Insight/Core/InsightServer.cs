using Mirror;
using System.Collections.Generic;
using Telepathy;
using UnityEngine;

namespace Insight
{
    [SerializeField]
    public class InsightServer : MonoBehaviour
    {
        [SerializeField]
        public LogFilter.FilterLevel logLevel = LogFilter.FilterLevel.Developer;

        public int networkPort;
        protected int serverHostId = -1;

        Telepathy.Server server;

        Dictionary<int, InsightNetworkConnection> clientConnections;
        Dictionary<short, InsightNetworkMessageDelegate> messageHandlers;

        private void Awake()
        {
            Application.runInBackground = true;
        }

        public InsightServer()
        {
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
            networkPort = Port;
            server.Start(Port);
            serverHostId = 0;

            OnServerStart();
        }

        public void StopServer()
        {
            // stop the server when you don't need it anymore
            server.Stop();
            serverHostId = -1;

            OnServerStop();
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
                        OnConnect(msg);
                        break;
                    case Telepathy.EventType.Data:
                        HandleData(msg.connectionId, msg.data, 0);
                        break;
                    case Telepathy.EventType.Disconnected:
                        OnDisconnect(msg);
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

        //----------virtual handlers--------------//

        public virtual void OnConnect(Message msg)
        {

        }

        public virtual void OnDisconnect(Message msg)
        {

        }

        public virtual void OnServerStart()
        {

        }

        public virtual void OnServerStop()
        {

        }
    }
}