using System.Collections.Generic;
using System.Linq;
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
        server.RegisterHandler((short)MsgId.SpawnerStatus, HandleSpawnerStatusMsg);
    }

    private void HandleRegisterSpawnerMsg(InsightNetworkMessage netMsg)
    {
        RegisterSpawnerMsg message = netMsg.ReadMessage<RegisterSpawnerMsg>();

        //Add the new child spawner to the list of spawners
        registeredSpawners.Add(new SpawnerContainer() { uniqueId = message.UniqueID, connectionId = netMsg.connectionId, MaxThreads = message.MaxThreads });

        if (server.logNetworkMessages) { Debug.Log("[MasterSpawner] - New Process Spawner Regsitered"); }
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

    private void HandleSpawnerStatusMsg(InsightNetworkMessage netMsg)
    {
        SpawnerStatus message = netMsg.ReadMessage<SpawnerStatus>();

        for(int i = 0; i < registeredSpawners.Count; i++)
        {
            if(registeredSpawners[i].connectionId == netMsg.connectionId)
            {
                SpawnerContainer instance = registeredSpawners[i];
                instance.CurrentThreads = message.CurrentThreads;
            }
        }
    }

    //Take in the options here
    public void RequestGameSpawn()
    {
        //sort by least busy spawner first
        registeredSpawners = registeredSpawners.OrderBy(x => x.CurrentThreads).ToList();

        server.SendToClient(registeredSpawners[0].connectionId, (short)MsgId.RequestSpawn, new RequestSpawn() { SpawnAlias = "managedgameserver" }, (success, reader) =>
        {
            if (success == CallbackStatus.Ok)
            {
                RequestSpawn callbackResponse = reader.ReadMessage<RequestSpawn>();
                if (server.logNetworkMessages) { Debug.Log("[Spawn Callback] Game Created on Child Spawner: " + callbackResponse.UniqueID); }

            //netMsg.Reply((short)MsgId.RequestSpawn, callbackResponse);
        }
        });
    }
}

public struct SpawnerContainer
{
    public string uniqueId;
    public int connectionId;
    public int MaxThreads;
    public int CurrentThreads;
}
