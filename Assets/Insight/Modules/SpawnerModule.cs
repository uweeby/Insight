using System.Collections.Generic;
using Insight;
using UnityEngine;
using Mirror;

public class SpawnerModule : InsightModule
{
    InsightServer server;
    InsightClient client;

    ModuleManager manager;

    [Header("Spawn Data")]
    [SerializeField]
    private string key;


    Dictionary<string, List<int>> registeredSpawners = new Dictionary<string, List<int>>();


    #region INSIGHT

    public override void Initialize(InsightServer server, ModuleManager manager)
    {
        this.server = server;
        this.manager = manager;
    }
    public override void Initialize(InsightClient client, ModuleManager manager)
    {
        this.client = client;
        this.manager = manager;

        client.OnConnectedEvent.AddListener(ClientOnConnectedEventHandler);
    }


    public override void RegisterHandlers()
    {
        if (client)
        {
            // listen for spawn request messages from the server
            client.RegisterHandler(ServerSpawnRequestMessage.MsgId, ServerSpawnRequestHandler);
        }
        if (server)
        {
            // list for spawn request messages from anyone
            server.RegisterHandler(SpawnRequestMessage.MsgId, SpawnRequestHandler);
            server.RegisterHandler(SpawnerClientRegistrationMessage.MsgId, SpawnClientRegistrationHandler);
        }
    }

    #endregion

    #region SERVER

    // SERVER-ONLY
    private void SpawnClientRegistrationHandler(InsightNetworkMessage netMsg)
    {
        // there would be data about what apps this client/spawner can 
        // create, and so would be put into a dict probably
        // of key (app) to list (connection id). 

        var msg = netMsg.ReadMessage<SpawnerClientRegistrationMessage>();
        if (!registeredSpawners.ContainsKey(msg.key))
        {
            registeredSpawners[msg.key] = new List<int>();
        }
        if (registeredSpawners[msg.key].Contains(netMsg.connectionId))
        {
            // maybe an error, as this key & connection have already been registered...
        }
        else
        {
            Debug.Log("Server registering client '" + netMsg.connectionId + "' for key '" + msg.key + "'.", this);
            registeredSpawners[msg.key].Add(netMsg.connectionId);
        }

        netMsg.Reply(); //empty reply just as a flag that "we got it"
    }

    // SERVER-ONLY
    // From anyone to the server spawn module
    private void SpawnRequestHandler(InsightNetworkMessage netMsg)
    {
        var msg = netMsg.ReadMessage<SpawnRequestMessage>();
        if (!registeredSpawners.ContainsKey(msg.key) ||
        registeredSpawners[msg.key].Count < 1)
        {
            netMsg.Reply(SpawnDataMessage.MsgId, new SpawnDataMessage() { spawnSuccess = false });
            return;
        }

        // find a client to use from the available list of registered clients
        // this is where round-robin, or a dependant module could provide a list
        // of clients to use for the _actual_ spawning. 
        server.SendToClient(registeredSpawners[msg.key][0], ServerSpawnRequestMessage.MsgId, new ServerSpawnRequestMessage() { key = msg.key }, (status, reader) =>
        {
            if (status == CallbackStatus.Ok)
            {
                var sdm = reader.ReadMessage<SpawnDataMessage>();

                // send a SpawnServerRequestReplyData message, OR 
                // just send on the SpawnDataMessage, if it is appropreate. 
                netMsg.Reply(SpawnDataMessage.MsgId, sdm);
            }
            else
            {
                // some kind of error ...
                netMsg.Reply(SpawnDataMessage.MsgId, new SpawnDataMessage() { spawnSuccess = false });
            }
        });
    }

    #endregion

    #region CLIENT

    // CLIENT-ONLY
    // only from the server spawn module to the client spawn module. 
    private void ServerSpawnRequestHandler(InsightNetworkMessage netMsg)
    {
        // can we fulfil this request? 
        var msg = netMsg.ReadMessage<ServerSpawnRequestMessage>();
        if (msg.key != "OurKey")
        {
            // no reply as somebody else on this instance might be able to fulfil it
            UnityEngine.Debug.LogWarning("Unable to spawn for key '" + msg.key + "'.", this);
            return;
        }


        // do the spawning here
        // SpawnItem(netMsg.key)

        // send a reply with the spawn info - IP/PORT/etc. 
        // in a SpawnDataMessage obj. 
        netMsg.Reply(SpawnDataMessage.MsgId, new SpawnDataMessage() { port = 7777, ip = "localhost" });
    }

    // CLIENT ONLY
    private void ClientOnConnectedEventHandler()
    {
        client.Send(SpawnerClientRegistrationMessage.MsgId, new SpawnerClientRegistrationMessage() { key = this.key }, (status, reader) =>
        {
            if (status == CallbackStatus.Ok)
            {
                // excellent, we are registered! 
                Debug.Log("Registered for key '" + this.key + "'", this);
            }
            else
            {
                // bummer, we should try to re-register or throw an error or something. 
                UnityEngine.Debug.LogError("Unable to register spawner key '" + this.key + "'.", this);
                return;
            }
        });
    }

    #endregion

    #region SPAWNING

    // TBD
   

    #endregion

   
}

public class SpawnerClientRegistrationMessage : MessageBase
{
    public static short MsgId = 7000;
    public string key;
}

public class SpawnRequestMessage : MessageBase
{
    public static short MsgId = 7001;
    public string key;
    public string[] args;
}

public class ServerSpawnRequestMessage : MessageBase
{
    public static short MsgId = 7002;
    public string key;
    public string[] args; // this would be whatever the actual arg class is
}

public class SpawnDataMessage : MessageBase
{
    public static short MsgId = 7003;
    public string key;
    public int port;
    public string ip;
    // etc etc
    public bool spawnSuccess = true;
}


