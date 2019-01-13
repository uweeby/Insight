using System.Collections.Generic;
using Insight;
using UnityEngine;

public class MasterSpawner : InsightModule
{
    InsightServer server;

    public List<SpawnerContainer> registeredSpawners = new List<SpawnerContainer>();

    public override void Initialize(InsightServer insight, ModuleManager manager)
    {
        server = insight;
        RegisterHandlers();
    }

    void RegisterHandlers()
    {
        server.RegisterHandler(RegisterSpawner.MsgId, HandleRegisterSpawner);
        server.RegisterHandler(SpawnRequest.MsgId, HandleSpawnRequest);
    }

    private void HandleRegisterSpawner(InsightNetworkMessage netMsg)
    {
        RegisterSpawner message = netMsg.ReadMessage<RegisterSpawner>();

        //Add the new child spawner to the list of spawners
        registeredSpawners.Add(new SpawnerContainer() { uniqueId = message.UniqueID, connectionId = netMsg.conn.connectionId });

        if (server.logNetworkMessages) { Debug.Log("HandleRegisterSpawner - Count: " + registeredSpawners.Count); }
    }

    private void HandleSpawnRequest(InsightNetworkMessage netMsg)
    {
        SpawnRequest message = netMsg.ReadMessage<SpawnRequest>();

        //Instead of handling the msg here we will forward it to an available spawner.
        //In the future this is where load balancing should start
        server.SendToClient(registeredSpawners[0].connectionId, SpawnRequest.MsgId, message, (success, reader) =>
        {
            if (success == CallbackStatus.Ok)
            {
                SpawnRequest callbackResponse = reader.ReadMessage<SpawnRequest>();
                if (server.logNetworkMessages) { Debug.Log("[Spawn Callback] Game Created on Child Spawner: " + callbackResponse.UniqueID); }

                netMsg.Reply(SpawnRequest.MsgId, callbackResponse);
            }
        });
    }
}

public struct SpawnerContainer
{
    public string uniqueId;
    public int connectionId;
}