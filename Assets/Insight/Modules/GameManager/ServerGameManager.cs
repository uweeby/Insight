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
    }

    private void HandleRegisterGameMsg(InsightNetworkMessage netMsg)
    {
        RegisterGameMsg message = netMsg.ReadMessage<RegisterGameMsg>();

        if (server.logNetworkMessages) { Debug.Log("Received GameRegistration request"); }

        registeredGameServers.Add(new GameContainer() {
            connectionId = netMsg.connectionId,
            uniqueId = message.UniqueID,
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

    public void SendGameServerStopMsg()
    {

    }
}

public struct GameContainer
{
    public string NetworkAddress;
    public ushort NetworkPort;
    public string uniqueId;
    public int connectionId;

    public string SceneName;
    public int MaxPlayer;
    public int CurrentPlayers;
}
