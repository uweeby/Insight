using Insight;
using System.Collections.Generic;
using UnityEngine;

public class ServerGameManager : InsightModule
{
    public InsightServer server;
    public MasterSpawner masterSpawner;

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
        server.RegisterHandler((short)MsgId.RegisterGame, HandleRegisterGameMsg);
        server.RegisterHandler((short)MsgId.JoinGame, HandleJoinGameMsg);
        server.RegisterHandler((short)MsgId.GameList, HandleGameListMsgMsg);
    }

    private void HandleRegisterGameMsg(InsightNetworkMessage netMsg)
    {
        RegisterGameMsg message = netMsg.ReadMessage<RegisterGameMsg>();

        if (server.logNetworkMessages) { Debug.Log("[GameManager] - Received GameRegistration request"); }

        registeredGameServers.Add(new GameContainer() {
            NetworkAddress = message.NetworkAddress,
            NetworkPort = message.NetworkPort,
            UniqueId = message.UniqueID,
            SceneName = message.SceneName,
            MaxPlayers = message.MaxPlayers,
            CurrentPlayers = message.CurrentPlayers,

            connectionId = netMsg.connectionId,
        });
    }

    private void HandleDisconnect(int connectionId)
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

    private void HandleGameListMsgMsg(InsightNetworkMessage netMsg)
    {
        if (server.logNetworkMessages) { UnityEngine.Debug.Log("[MatchMaking] - Player Requesting Match list"); }

        GameListMsg gamesListMsg = new GameListMsg();
        gamesListMsg.Load(registeredGameServers);

        netMsg.Reply((short)MsgId.GameList, gamesListMsg);
    }

    private void HandleJoinGameMsg(InsightNetworkMessage netMsg)
    {
        JoinGamMsg message = netMsg.ReadMessage<JoinGamMsg>();

        if (server.logNetworkMessages) { UnityEngine.Debug.Log("[MatchMaking] - Player joining Match."); }

        GameContainer game = GetGameByUniqueID(message.UniqueID);

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
    public void RequestGameSpawn(RequestSpawnMsg requestSpawn)
    {
        masterSpawner.InternalSpawnRequest(requestSpawn);
    }

    public GameContainer GetGameByUniqueID(string uniqueID)
    {
        foreach(GameContainer game in registeredGameServers)
        {
            if (game.UniqueId.Equals(uniqueID))
            {
                return game;
            }
        }
        return null;
    }
}

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
