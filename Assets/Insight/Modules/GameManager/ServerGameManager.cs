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

        public List<GameContainer> registeredGameServers = new List<GameContainer>();

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

        void HandleRegisterGameMsg(RegisterGameMsg netMsg)
        {
            if (logger.LogEnabled()) logger.Log("[GameManager] - Received GameRegistration request");

            registeredGameServers.Add(new GameContainer()
            {
                NetworkAddress = netMsg.NetworkAddress,
                NetworkPort = netMsg.NetworkPort,
                UniqueId = netMsg.UniqueID,
                SceneName = netMsg.SceneName,
                MaxPlayers = netMsg.MaxPlayers,
                CurrentPlayers = netMsg.CurrentPlayers,

                connectionId = netMsg.connectionId,
            });
        }

        void HandleGameStatusMsg(GameStatusMsg netMsg)
        {
            if (logger.LogEnabled()) logger.Log("[GameManager] - Received Game status update");

            foreach (GameContainer game in registeredGameServers)
            {
                if (game.UniqueId == netMsg.UniqueID)
                {
                    game.CurrentPlayers = netMsg.CurrentPlayers;
                }
            };
        }

        //Checks if the connection that dropped is actually a GameServer
        void HandleDisconnect(INetworkConnection connection)
        {
            foreach (GameContainer game in registeredGameServers)
            {
                if (game.connectionId == connectionId)
                {
                    registeredGameServers.Remove(game);
                    return;
                }
            }
        }

        void HandleGameListMsg(GameListMsg netMsg)
        {
            if (logger.LogEnabled()) logger.Log("[MatchMaking] - Player Requesting Match list");

            gamesListMsg.Load(registeredGameServers);

            netMsg.Reply((short)MsgId.GameList, gamesListMsg);
        }

        void HandleJoinGameMsg(JoinGameMsg netMsg)
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
                netMsg.Reply((short)MsgId.ChangeServers, new ChangeServerMsg()
                {
                    NetworkAddress = game.NetworkAddress,
                    NetworkPort = game.NetworkPort,
                    SceneName = game.SceneName
                });
            }
        }

        //Used by MatchMaker to request a GameServer for a new Match
        public void RequestGameSpawnStart(RequestSpawnStartMsg requestSpawn)
        {
            masterSpawner.InternalSpawnRequest(requestSpawn);
        }

        public GameContainer GetGameByUniqueID(string uniqueID)
        {
            foreach (GameContainer game in registeredGameServers)
            {
                if (game.UniqueId.Equals(uniqueID))
                {
                    return game;
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
        public int connectionId;

        public string SceneName;
        public int MaxPlayers;
        public int MinPlayers;
        public int CurrentPlayers;
    }
}