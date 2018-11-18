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

        Server server;

        //Connection Lists
        Dictionary<int, InsightNetworkConnection> connections;
        //Dictionary<int, InsightNetworkConnection> clientConnections;
        //Dictionary<int, InsightNetworkConnection> serverConnections;

        //Peer Lists
        public List<PlayerInformation> playerInformation;
        public List<ZoneInformation> zoneInformation;

        //Handlers
        Dictionary<short, InsightNetworkMessageDelegate> unregisteredMessageHandlers; //Default Handlers
        Dictionary<short, InsightNetworkMessageDelegate> clientMessageHandlers; //Applied only to clients after AuthID provided
        Dictionary<short, InsightNetworkMessageDelegate> serverMessageHandlers; //Applies only to servers after AuthID provided

        protected enum ConnectState
        {
            None,
            Connecting,
            Connected,
            Disconnected,
        }
        protected ConnectState connectState = ConnectState.None;

        public bool isConnected { get { return connectState == ConnectState.Connected; } }

        private void Awake()
        {
            Application.runInBackground = true;
        }

        public InsightServer()
        {
            // use Debug.Log functions for Telepathy so we can see it in the console
            Telepathy.Logger.LogMethod = Debug.Log;
            Telepathy.Logger.LogWarningMethod = Debug.LogWarning;
            Telepathy.Logger.LogErrorMethod = Debug.LogError;

            // create and start the server
            server = new Server();

            connections = new Dictionary<int, InsightNetworkConnection>();
            //clientConnections = new Dictionary<int, InsightNetworkConnection>();
            //serverConnections = new Dictionary<int, InsightNetworkConnection>();

            unregisteredMessageHandlers = new Dictionary<short, InsightNetworkMessageDelegate>();
            clientMessageHandlers = new Dictionary<short, InsightNetworkMessageDelegate>();
            serverMessageHandlers = new Dictionary<short, InsightNetworkMessageDelegate>();
        }

        public void StartServer(int Port)
        {
            networkPort = Port;
            server.Start(Port);
            serverHostId = 0;

            connectState = ConnectState.Connected;

            OnServerStart();
        }

        public void StopServer()
        {
            // stop the server when you don't need it anymore
            server.Stop();
            serverHostId = -1;

            connectState = ConnectState.Disconnected;

            OnServerStop();
        }

        // grab all new messages. do this in your Update loop.
        public void HandleNewMessages()
        {
            if (serverHostId == -1)
                return;

            Message msg;
            while (server.GetNextMessage(out msg))
            {
                switch (msg.eventType)
                {
                    case Telepathy.EventType.Connected:
                        HandleConnect(msg);
                        break;
                    case Telepathy.EventType.Data:
                        HandleData(msg.connectionId, msg.data, 0);
                        break;
                    case Telepathy.EventType.Disconnected:
                        HandleDisconnect(msg);
                        break;
                }
            }
        }

        void HandleConnect(Message msg)
        {
            // get ip address from connection
            string address = GetConnectionInfo(msg.connectionId);

            // add player info
            InsightNetworkConnection conn = new InsightNetworkConnection();
            conn.Initialize(this, address, serverHostId, msg.connectionId);
            AddConnection(conn);

            OnConnected(conn);
        }

        void HandleDisconnect(Message msg)
        {
            InsightNetworkConnection conn;
            if (connections.TryGetValue(msg.connectionId, out conn))
            {
                conn.Disconnect();
                RemoveConnection(msg.connectionId);

                OnDisconnected(conn);
            }
        }

        void HandleData(int connectionId, byte[] data, byte error)
        {
            InsightNetworkConnection conn;
            if (connections.TryGetValue(connectionId, out conn))
            {
                print("unregisteredConnections");
                OnData(conn, data);
                return;
            }
            //if (clientConnections.TryGetValue(connectionId, out conn))
            //{
            //    print("clientConnections");
            //    OnData(conn, data);
            //    return;
            //}
            //if (serverConnections.TryGetValue(connectionId, out conn))
            //{
            //    print("serverConnections");
            //    OnData(conn, data);
            //    return;
            //}
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
            if (unregisteredMessageHandlers.ContainsKey(msgType))
            {
                //if (LogFilter.Debug) { Debug.Log("NetworkConnection.RegisterHandler replacing " + msgType); }
                Debug.Log("NetworkConnection.RegisterHandler replacing " + msgType);
            }
            unregisteredMessageHandlers[msgType] = handler;
        }

        public void RegisterClientHandler(short msgType, InsightNetworkMessageDelegate handler)
        {
            if (clientMessageHandlers.ContainsKey(msgType))
            {
                //if (LogFilter.Debug) { Debug.Log("NetworkConnection.RegisterHandler replacing " + msgType); }
                Debug.Log("NetworkConnection.RegisterHandler replacing " + msgType);
            }
            clientMessageHandlers[msgType] = handler;
        }

        public void RegisterServerHandler(short msgType, InsightNetworkMessageDelegate handler)
        {
            if (serverMessageHandlers.ContainsKey(msgType))
            {
                //if (LogFilter.Debug) { Debug.Log("NetworkConnection.RegisterHandler replacing " + msgType); }
                Debug.Log("NetworkConnection.RegisterHandler replacing " + msgType);
            }
            serverMessageHandlers[msgType] = handler;
        }

        public string GetConnectionInfo(int connectionId)
        {
            string address;
            server.GetConnectionInfo(connectionId, out address);
            return address;
        }

        public bool AddConnection(InsightNetworkConnection conn)
        {
            if (!connections.ContainsKey(conn.connectionId))
            {
                // connection cannot be null here or conn.connectionId
                // would throw NRE
                connections[conn.connectionId] = conn;
                conn.SetHandlers(unregisteredMessageHandlers);
                return true;
            }
            // already a connection with this id
            return false;
        }

        public bool SetClientHandlers(InsightNetworkConnection conn)
        {
            conn.SetHandlers(clientMessageHandlers);
            return true;
        }

        public bool SetServerHandlers(InsightNetworkConnection conn)
        {
            conn.SetHandlers(serverMessageHandlers);
            return true;
        }

        public bool RemoveConnection(int connectionId)
        {
            return connections.Remove(connectionId);
        }

        //public bool RemoveClientConnection(int connectionId)
        //{
        //    return clientConnections.Remove(connectionId);
        //}

        //public bool RemoveServerConnection(int connectionId)
        //{
        //    return serverConnections.Remove(connectionId);
        //}

        public NetworkConnection FindConnectionByPlayer(string PlayerName)
        {
            foreach(PlayerInformation player in playerInformation)
            {
                if(player.PlayerName.Equals(PlayerName))
                {
                    return player.masterNetworkConnection;
                }
            }
            return null;
        }

        private void OnApplicationQuit()
        {
            server.Stop();
        }

        //----------virtual handlers--------------//

        public virtual void OnConnected(InsightNetworkConnection conn)
        {

        }

        public virtual void OnDisconnected(InsightNetworkConnection conn)
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

public struct PlayerInformation
{
    public string AuthID; //Generated by LoginServer after authentication
    public NetworkConnection masterNetworkConnection; //Connection from Client to MasterServer. Should not change
    public string AccountName; //Used to look up account information for Character without asking DB.
    public string PlayerName; //Name of currently logged in character for account
}

public struct ZoneInformation
{
    public string NetworkAddress;
    public int NetworkPort;
}