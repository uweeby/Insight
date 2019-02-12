using Insight;
using System.Collections.Generic;
using UnityEngine;

public class ServerGameManager : InsightModule
{
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
        server.RegisterHandler((short)MsgId.RegisterGame, HandleRegisterGameMsg);
        server.RegisterHandler((short)MsgId.JoinGame, HandleJoinGameMsg);
        server.RegisterHandler((short)MsgId.GameList, HandleGameListMsgMsg);
    }

    private void HandleRegisterGameMsg(InsightNetworkMessage netMsg)
    {
        RegisterGameMsg message = netMsg.ReadMessage<RegisterGameMsg>();

        if (server.logNetworkMessages) { Debug.Log("Received GameRegistration request"); }

        registeredGameServers.Add(new GameContainer() {
            connectionId = netMsg.connectionId,
            UniqueId = message.UniqueID,
            SceneName = message.SceneName,
            NetworkAddress = message.NetworkAddress,
            NetworkPort = message.NetworkPort});
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

        netMsg.Reply((short)MsgId.GameList, new GameListMsg());
    }

    private void HandleJoinGameMsg(InsightNetworkMessage netMsg)
    {
        JoinGamMsg message = netMsg.ReadMessage<JoinGamMsg>();

        if (server.logNetworkMessages) { UnityEngine.Debug.Log("[MatchMaking] - Player joining Match."); }

        GameContainer game = GetGameByUniqueID(message.UniqueID);

        netMsg.Reply((short)MsgId.ChangeServers, new ChangeServers() {
            NetworkAddress = game.NetworkAddress,
            NetworkPort = game.NetworkPort,
            SceneName = game.SceneName
        });
    }

    //Take in the options here
    public void RequestGameSpawn(RequestSpawn requestSpawn)
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
