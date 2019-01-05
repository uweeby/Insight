using System;
using System.Collections.Generic;
using Insight;

public class SpawnRequestModule : InsightModule
{
    InsightServer server;
    ModuleManager manager;

    Dictionary<string, List<int>> registeredSpawners = new Dictionary<string, List<int>>();

    private void Awake()
    {
        AddDependency<BasicSpawnerModule>();
    }

    public override void Initialize(InsightServer insight, ModuleManager manager)
    {
        server = insight;
        this.manager = manager;
    }

    public override void RegisterHandlers()
    {
        server.RegisterHandler(SpawnDataMessage.MsgId, SpawnDataMessageHandler);
    }

    private void SpawnDataMessageHandler(InsightNetworkMessage netMsg)
    {
        SpawnDataMessage message = netMsg.ReadMessage<SpawnDataMessage>();

        //This links to the BasicSpawnModule

        //Reply to ack the request
        netMsg.Reply(SpawnDataMessage.MsgId, new SpawnDataMessage() { GameName = message.GameName, NetworkAddress = "test.com", NetworkPort = 420, UniqueID = Guid.NewGuid().ToString() });
    }
}