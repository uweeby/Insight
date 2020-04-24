using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace Insight
{
    public class ServerGameManager : InsightModule
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(ServerGameManager));

        NetworkServer server;
        MasterSpawner masterSpawner;

        public Dictionary<INetworkConnection, GameContainer> registeredGameServers = new Dictionary<INetworkConnection, GameContainer>();

        public void Awake()
        {
            AddDependency<MasterSpawner>();
        }

        public override void Initialize(NetworkServer insight, ModuleManager manager)
        {
            server = insight;
            masterSpawner = manager.GetModule<MasterSpawner>();
            RegisterHandlers();

            server.Disconnected.AddListener(HandleDisconnect);
        }

        void RegisterHandlers()
        {
            server.LocalConnection.RegisterHandler<RegisterGameMsg>(HandleRegisterGameMsg);
            server.LocalConnection.RegisterHandler<GameStatusMsg>(HandleGameStatusMsg);
            server.LocalConnection.RegisterHandler<JoinGameMsg>(HandleJoinGameMsg);
            server.LocalConnection.RegisterHandler<GameListMsg>(HandleGameListMsg);
        }

        void HandleRegisterGameMsg(INetworkConnection connection, RegisterGameMsg netMsg)
        {
            if (logger.LogEnabled()) logger.Log("[GameManager] - Received GameRegistration request");

            registeredGameServers.Add(connection, new GameContainer()
            {
                NetworkAddress = netMsg.NetworkAddress,
                NetworkPort = netMsg.NetworkPort,
                UniqueId = netMsg.UniqueID,
                SceneName = netMsg.SceneName,
                MaxPlayers = netMsg.MaxPlayers,
                CurrentPlayers = netMsg.CurrentPlayers
            });
        }

        void HandleGameStatusMsg(GameStatusMsg netMsg)
        {
            if (logger.LogEnabled()) logger.Log("[GameManager] - Received Game status update");

            foreach (KeyValuePair<INetworkConnection, GameContainer> game in registeredGameServers)
            {
                if (game.Value.UniqueId == netMsg.UniqueID)
                {
                    game.Value.CurrentPlayers = netMsg.CurrentPlayers;
                }
            };
        }

        //Checks if the connection that dropped is actually a GameServer
        void HandleDisconnect(INetworkConnection connection)
        {
            foreach (KeyValuePair<INetworkConnection, GameContainer> game in registeredGameServers)
            {
                if (game.Key == connection)
                {
                    registeredGameServers.Remove(game.Key);
                    return;
                }
            }
        }

        void HandleGameListMsg(INetworkConnection connection, GameListMsg netMsg)
        {
            if (logger.LogEnabled()) logger.Log("[MatchMaking] - Player Requesting Match list");

            netMsg.Load(registeredGameServers);

            connection.SendAsync(netMsg); //This was a reply
        }

        void HandleJoinGameMsg(INetworkConnection connection, JoinGameMsg netMsg)
        {
            if (logger.LogEnabled()) logger.Log("[MatchMaking] - Player joining Match.");

            GameContainer game = GetGameByUniqueID(netMsg.UniqueID);

            if (game == null)
            {
                //Something went wrong
                //netMsg.Reply((short)MsgId.ChangeServers, new ChangeServerMsg());
            }
            else
            {
                connection.SendAsync(new ChangeServerMsg() //This was a reply
                {
                    NetworkAddress = game.NetworkAddress,
                    NetworkPort = game.NetworkPort,
                    SceneName = game.SceneName
                });
            }
        }

        //Used by MatchMaker to request a GameServer for a new Match
        public void RequestGameSpawnStart(INetworkConnection connection, RequestSpawnStartMsg requestSpawn)
        {
            masterSpawner.InternalSpawnRequest(connection, requestSpawn);
        }

        public GameContainer GetGameByUniqueID(string uniqueID)
        {
            foreach (KeyValuePair<INetworkConnection, GameContainer> game in registeredGameServers)
            {
                if (game.Value.UniqueId.Equals(uniqueID))
                {
                    return game.Value;
                }
            }
            return null;
        }
    }

    [Serializable]
    public class GameContainer
    {
        public string NetworkAddress;
        public ushort NetworkPort;
        public string UniqueId;
        public string SceneName;
        public int MaxPlayers;
        public int MinPlayers;
        public int CurrentPlayers;
    }
}