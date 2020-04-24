using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

namespace Insight
{
    public class MasterSpawner : InsightModule
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(MasterSpawner));

        NetworkServer server;

        public List<SpawnerContainer> registeredSpawners = new List<SpawnerContainer>();

        public override void Initialize(NetworkServer insight, ModuleManager manager)
        {
            server = insight;
            RegisterHandlers();

            server.Disconnected.AddListener(HandleDisconnect);
        }

        void RegisterHandlers()
        {
            server.LocalConnection.RegisterHandler<RegisterSpawnerMsg>(HandleRegisterSpawnerMsg);
            server.LocalConnection.RegisterHandler<RequestSpawnStartMsg>(HandleSpawnRequestMsg);
            server.LocalConnection.RegisterHandler<SpawnerStatusMsg>(HandleSpawnerStatusMsg);
        }

        //Checks if the connection that dropped is actually a Spawner
        void HandleDisconnect(INetworkConnection conn)
        {
            foreach (SpawnerContainer spawner in registeredSpawners)
            {
                if (spawner.connection == conn)
                {
                    registeredSpawners.Remove(spawner);
                    return;
                }
            }
        }

        void HandleRegisterSpawnerMsg(INetworkConnection conn, RegisterSpawnerMsg netMsg)
        {
            //Add the new child spawner to the list of spawners
            registeredSpawners.Add(new SpawnerContainer()
            {
                uniqueId = netMsg.UniqueID,
                connection = conn,
                MaxThreads = netMsg.MaxThreads
            });

            if (logger.LogEnabled()) logger.Log("[MasterSpawner] - New Process Spawner Regsitered");
        }

        //Instead of handling the msg here we will forward it to an available spawner.
        void HandleSpawnRequestMsg(INetworkConnection conn, RequestSpawnStartMsg netMsg)
        {
            if(registeredSpawners.Count == 0)
            {
                Debug.LogWarning("[MasterSpawner] - No Spawner Regsitered To Handle Spawn Request");
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
            conn.SendAsync(netMsg);//, (callbackStatus, reader) =>
            {
                //if (callbackStatus == CallbackStatus.Ok)
                //{
                //    RequestSpawnStartMsg callbackResponse = reader.ReadMessage<RequestSpawnStartMsg>();
                //    if (server.logNetworkMessages) { Debug.Log("[Spawn Callback] Game Created on Child Spawner: " + callbackResponse.UniqueID); }

                ////If callback from original message is present
                //if (netMsg.callbackId != 0)
                //    {
                //        netMsg.Reply((short)MsgId.RequestSpawnStart, callbackResponse);
                //    }
                //}
                //if (callbackStatus == CallbackStatus.Timeout)
                //{
                //    RequestSpawnStartMsg callbackResponse = reader.ReadMessage<RequestSpawnStartMsg>();
                //    Debug.Log("[Spawn Callback] Createion Timed Out: " + callbackResponse.UniqueID);
                //}
                //if (callbackStatus == CallbackStatus.Error)
                //{
                //    Debug.Log("[Spawn Callback] Error in SpawnRequest.");
                //}
            };
        }

        void HandleSpawnerStatusMsg(INetworkConnection conn, SpawnerStatusMsg netMsg)
        {
            for (int i = 0; i < registeredSpawners.Count; i++)
            {
                if (registeredSpawners[i].connection == conn)
                {
                    registeredSpawners[i].CurrentThreads = netMsg.CurrentThreads;
                }
            }
        }

        public void InternalSpawnRequest(INetworkConnection conn, RequestSpawnStartMsg message)
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
            freeSlotSpawners[0].connection.Send(message);
        }
    }

    [Serializable]
    public class SpawnerContainer
    {
        public string uniqueId;
        public INetworkConnection connection;
        public int MaxThreads;
        public int CurrentThreads;
    }
}