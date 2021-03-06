using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Insight
{
    public class MasterSpawner : InsightModule
    {
        InsightServer server;

        public List<SpawnerContainer> registeredSpawners = new List<SpawnerContainer>();

        public override void Initialize(InsightServer insight, ModuleManager manager)
        {
            server = insight;
            RegisterHandlers();

            server.transport.OnServerDisconnected= HandleDisconnect;
        }

        void RegisterHandlers()
        {
            server.RegisterHandler<RegisterSpawnerMsg>(HandleRegisterSpawnerMsg);
            server.RegisterHandler<RequestSpawnStartMsg>(HandleSpawnRequestMsg);
            server.RegisterHandler<SpawnerStatusMsg>(HandleSpawnerStatusMsg);
        }

        //Checks if the connection that dropped is actually a Spawner
        void HandleDisconnect(int connectionId)
        {
            foreach (SpawnerContainer spawner in registeredSpawners)
            {
                if (spawner.connectionId == connectionId)
                {
                    registeredSpawners.Remove(spawner);
                    return;
                }
            }
        }

        void HandleRegisterSpawnerMsg(InsightNetworkMessage netMsg)
        {
            RegisterSpawnerMsg message = netMsg.ReadMessage<RegisterSpawnerMsg>();

            //Add the new child spawner to the list of spawners
            registeredSpawners.Add(new SpawnerContainer()
            {
                uniqueId = message.UniqueID,
                connectionId = netMsg.connectionId,
                MaxThreads = message.MaxThreads
            });

            Debug.Log("[MasterSpawner] - New Process Spawner Regsitered");
        }

        //Instead of handling the msg here we will forward it to an available spawner.
        void HandleSpawnRequestMsg(InsightNetworkMessage netMsg)
        {
            if(registeredSpawners.Count == 0)
            {
                Debug.LogWarning("[MasterSpawner] - No Spawner Regsitered To Handle Spawn Request");
                return;
            }

            RequestSpawnStartMsg message = netMsg.ReadMessage<RequestSpawnStartMsg>();

            //Get all spawners that have atleast 1 slot free
            List<SpawnerContainer> freeSlotSpawners = new List<SpawnerContainer>();
            foreach (SpawnerContainer spawner in registeredSpawners)
            {
                if (spawner.CurrentThreads < spawner.MaxThreads)
                {
                    freeSlotSpawners.Add(spawner);
                }
            }

            //sort by least busy spawner first
            freeSlotSpawners = freeSlotSpawners.OrderBy(x => x.CurrentThreads).ToList();
            server.SendToClient(freeSlotSpawners[0].connectionId, message, (reader) =>
            {
                RequestSpawnStartMsg callbackResponse = reader.ReadMessage<RequestSpawnStartMsg>();

                if (callbackResponse.Status == CallbackStatus.Success)
                {
                    Debug.Log("[Spawn Callback] Game Created on Child Spawner: " + callbackResponse.UniqueID);

                    //If callback from original message is present
                    if (netMsg.callbackId != 0)
                    {
                        netMsg.Reply(callbackResponse);
                    }
                }
                if (callbackResponse.Status == CallbackStatus.Timeout)
                {
                    Debug.Log("[Spawn Callback] Createion Timed Out: " + callbackResponse.UniqueID);
                }
                if (callbackResponse.Status == CallbackStatus.Error)
                {
                    Debug.Log("[Spawn Callback] Error in SpawnRequest.");
                }
            });
        }

        void HandleSpawnerStatusMsg(InsightNetworkMessage netMsg)
        {
            SpawnerStatusMsg message = netMsg.ReadMessage<SpawnerStatusMsg>();

            for (int i = 0; i < registeredSpawners.Count; i++)
            {
                if (registeredSpawners[i].connectionId == netMsg.connectionId)
                {
                    registeredSpawners[i].CurrentThreads = message.CurrentThreads;
                }
            }
        }

        public void InternalSpawnRequest(RequestSpawnStartMsg message)
        {
            if(registeredSpawners.Count == 0)
            {
                Debug.LogWarning("[MasterSpawner] - No Spawner Regsitered To Handle Internal Spawn Request");
                return;
            }

            //Get all spawners that have atleast 1 slot free
            List<SpawnerContainer> freeSlotSpawners = registeredSpawners.Where(x => (x.CurrentThreads < x.MaxThreads)).ToList();

            if (freeSlotSpawners.Count == 0)
            {
                Debug.LogError("[MasterSpawner] - No Spawners with slots free available to service SpawnRequest.");
                return;
            }

            //sort by least busy spawner first
            freeSlotSpawners = freeSlotSpawners.OrderBy(x => x.CurrentThreads).ToList();

            //Send SpawnRequest to the least busy Spawner
            server.SendToClient(freeSlotSpawners[0].connectionId, message);
        }
    }

    [Serializable]
    public class SpawnerContainer
    {
        public string uniqueId;
        public int connectionId;
        public int MaxThreads;
        public int CurrentThreads;
    }
}