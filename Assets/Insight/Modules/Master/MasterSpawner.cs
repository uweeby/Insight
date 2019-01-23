using System.Collections.Generic;
using Insight;
using UnityEngine;

public partial class MasterSpawner : InsightModule
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
        server.RegisterHandler((short)MsgId.RegisterSpawner, HandleRegisterSpawnerMsg);
        server.RegisterHandler((short)MsgId.RequestSpawn, HandleSpawnRequestMsg);
    }

    private void HandleRegisterSpawnerMsg(InsightNetworkMessage netMsg)
    {
        RegisterSpawnerMsg message = netMsg.ReadMessage<RegisterSpawnerMsg>();

        //Add the new child spawner to the list of spawners
        registeredSpawners.Add(new SpawnerContainer() { uniqueId = message.UniqueID, connectionId = netMsg.connectionId });

        if (server.logNetworkMessages) { Debug.Log("HandleRegisterSpawner - Count: " + registeredSpawners.Count); }
    }

    private void HandleSpawnRequestMsg(InsightNetworkMessage netMsg)
    {
        RequestSpawn message = netMsg.ReadMessage<RequestSpawn>();

        //Instead of handling the msg here we will forward it to an available spawner.
        //In the future this is where load balancing should start
        server.SendToClient(registeredSpawners[0].connectionId, (short)MsgId.RequestSpawn, message, (success, reader) =>
        {
            if (success == CallbackStatus.Ok)
            {
                RequestSpawn callbackResponse = reader.ReadMessage<RequestSpawn>();
                if (server.logNetworkMessages) { Debug.Log("[Spawn Callback] Game Created on Child Spawner: " + callbackResponse.UniqueID); }

                netMsg.Reply((short)MsgId.RequestSpawn, callbackResponse);
            }
        });
    }
}

public struct SpawnerContainer
{
    public string uniqueId;
    public int connectionId;
}
