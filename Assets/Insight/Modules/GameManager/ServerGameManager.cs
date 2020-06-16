using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Insight
{
    public class ServerGameManager : InsightModule
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(ServerGameManager));

        InsightServer server;
        MasterSpawner masterSpawner;

        public List<GameContainer> registeredGameServers = new List<GameContainer>();

        public void Awake()
        {
            AddDependency<MasterSpawner>();
        }

        public override void Initialize(InsightServer insight, ModuleManager manager)
        {
            server = insight;
            masterSpawner = manager.GetModule<MasterSpawner>();
            RegisterHandlers();

            server.transport.OnServerDisconnected.AddListener(HandleDisconnect);
        }

        void RegisterHandlers()
        {
            server.RegisterHandler<RegisterGameMsg>(HandleRegisterGameMsg);
            server.RegisterHandler<GameStatusMsg>(HandleGameStatusMsg);
            server.RegisterHandler<JoinGamMsg>(HandleJoinGameMsg);
            server.RegisterHandler<GameListMsg>(HandleGameListMsg);
        }

        void HandleRegisterGameMsg(InsightNetworkMessage netMsg)
        {
            RegisterGameMsg message = netMsg.ReadMessage<RegisterGameMsg>();

            logger.Log("[GameManager] - Received GameRegistration request");

            registeredGameServers.Add(new GameContainer()
            {
                NetworkAddress = message.NetworkAddress,
                NetworkPort = message.NetworkPort,
                UniqueId = message.UniqueID,
                SceneName = message.SceneName,
                MaxPlayers = message.MaxPlayers,
                CurrentPlayers = message.CurrentPlayers,

                connectionId = netMsg.connectionId,
            });
        }

        void HandleGameStatusMsg(InsightNetworkMessage netMsg)
        {
            GameStatusMsg message = netMsg.ReadMessage<GameStatusMsg>();

            logger.Log("[GameManager] - Received Game status update");

            foreach (GameContainer game in registeredGameServers)
            {
                if (game.UniqueId == message.UniqueID)
                {
                    game.CurrentPlayers = message.CurrentPlayers;
                }
            };
        }

        //Checks if the connection that dropped is actually a GameServer
        void HandleDisconnect(int connectionId)
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

        void HandleGameListMsg(InsightNetworkMessage netMsg)
        {
            logger.Log("[MatchMaking] - Player Requesting Match list");

            GameListMsg gamesListMsg = new GameListMsg();
            gamesListMsg.Load(registeredGameServers);

            netMsg.Reply(gamesListMsg);
        }

        void HandleJoinGameMsg(InsightNetworkMessage netMsg)
        {
            JoinGamMsg message = netMsg.ReadMessage<JoinGamMsg>();

            logger.Log("[MatchMaking] - Player joining Match.");

            GameContainer game = GetGameByUniqueID(message.UniqueID);

            if (game == null)
            {
                //Something went wrong
                //netMsg.Reply((short)MsgId.ChangeServers, new ChangeServerMsg());
            }
            else
            {
                netMsg.Reply(new ChangeServerMsg()
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