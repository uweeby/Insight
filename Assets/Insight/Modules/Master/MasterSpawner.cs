using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Insight
{
    public class MasterSpawner : InsightModule
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(MasterSpawner));

        InsightServer server;

        public List<SpawnerContainer> registeredSpawners = new List<SpawnerContainer>();

        public override void Initialize(InsightServer insight, ModuleManager manager)
        {
            server = insight;
            RegisterHandlers();

            server.transport.OnServerDisconnected.AddListener(HandleDisconnect);
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

        void HandleRegisterSpawnerMsg(InsightNetworkConnection conn, RegisterSpawnerMsg message)
        {
            //Add the new child spawner to the list of spawners
            registeredSpawners.Add(new SpawnerContainer()
            {
                uniqueId = message.UniqueID,
                connectionId = conn.connectionId,
                MaxThreads = message.MaxThreads
            });

            logger.Log("[MasterSpawner] - New Process Spawner Regsitered");
        }

        //Instead of handling the msg here we will forward it to an available spawner.
        void HandleSpawnRequestMsg(InsightNetworkConnection conn, RequestSpawnStartMsg message)
        {
            if(registeredSpawners.Count == 0)
            {
                logger.LogWarning("[MasterSpawner] - No Spawner Regsitered To Handle Spawn Request");
                return;
            }

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
            server.SendToClient(freeSlotSpawners[0].connectionId, message, (reader)  =>
            {
                RequestSpawnStartMsg callbackResponse = reader as RequestSpawnStartMsg;

                if (callbackResponse.Status == CallbackStatus.Success)
                {
                    logger.Log("[Spawn Callback] Game Created on Child Spawner: " + callbackResponse.UniqueID);

                    //If callback from original message is present
                    if (callbackResponse.callbackId != 0)
                    {
                        conn.Reply(callbackResponse);
                    }
                }
                if (reader.Status == CallbackStatus.Timeout)
                {
                    logger.Log("[Spawn Callback] Createion Timed Out: " + callbackResponse.UniqueID);
                }
                if (reader.Status == CallbackStatus.Error)
                {
                    logger.Log("[Spawn Callback] Error in SpawnRequest.");
                }
            });
        }

        void HandleSpawnerStatusMsg(InsightNetworkConnection conn, SpawnerStatusMsg message)
        {
            for (int i = 0; i < registeredSpawners.Count; i++)
            {
                if (registeredSpawners[i].connectionId == conn.connectionId)
                {
                    registeredSpawners[i].CurrentThreads = message.CurrentThreads;
                }
            }
        }

        public void InternalSpawnRequest(RequestSpawnStartMsg message)
        {
            if(registeredSpawners.Count == 0)
            {
                logger.LogWarning("[MasterSpawner] - No Spawner Regsitered To Handle Internal Spawn Request");
                return;
            }

            //Get all spawners that have atleast 1 slot free
            List<SpawnerContainer> freeSlotSpawners = registeredSpawners.Where(x => (x.CurrentThreads < x.MaxThreads)).ToList();

            if (freeSlotSpawners.Count == 0)
            {
                logger.LogError("[MasterSpawner] - No Spawners with slots free available to service SpawnRequest.");
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
