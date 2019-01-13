using System;
using Insight;

public class SpawnRequestModule : InsightModule
{
    InsightServer server;

    //Dictionary<string, List<int>> registeredSpawners = new Dictionary<string, List<int>>();

    private void Awake()
    {
        AddDependency<BasicSpawnerModule>();
    }

    public override void Initialize(InsightServer insight, ModuleManager manager)
    {
        server = insight;
        RegisterHandlers();
    }

    void RegisterHandlers()
    {
        server.RegisterHandler(SpawnRequest.MsgId, SpawnRequestHandler);
    }

    private void SpawnRequestHandler(InsightNetworkMessage netMsg)
    {
        SpawnRequest message = netMsg.ReadMessage<SpawnRequest>();

        //This links to the BasicSpawnModule

        //Reply to ack the request
        netMsg.Reply(SpawnRequest.MsgId, new SpawnRequest() { GameName = message.GameName, NetworkAddress = "test.com", NetworkPort = 420, UniqueID = Guid.NewGuid().ToString() });
    }
}