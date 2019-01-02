using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Insight;
using UnityEngine;

public class SpawnerModule : InsightModule
{
    InsightServer server;
    InsightClient client;

    ModuleManager manager;

    [Header("Paths")]
    public string EditorPath;
    public string ProcessPath;

    [Header("Standalone")]
    public string ProcessName;

    [Header("Ports")]
    public int StartingPort = 7000;
    private int _portUsageCounter = 0;

    [Header("Threads")]
    public int MaximumProcesses = 5;
    public int StaticProcessCount; //Number of processes to spawn on Start
    private int _processUsageCounter;

    public List<SpawnedProcesses> spawnedProcessList = new List<SpawnedProcesses>();

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
            client.RegisterHandler(0, ServerSpawnRequestHandler);
        }
        if (server)
        {
            // list for spawn request messages from anyone
            server.RegisterHandler(0, SpawnRequestHandler);
            server.RegisterHandler(0, SpawnClientRegistrationHandler);
        }
    }

    #endregion

    #region SERVER

    Dictionary<string, HashSet<int>> registeredSpawners = new Dictionary<string, HashSet<int>>();

    // SERVER-ONLY
    private void SpawnClientRegistrationHandler(InsightNetworkMessage netMsg)
    {
        // there would be data about what apps this client/spawner can 
        // create, and so would be put into a dict probably
        // of key (app) to list (connection id). 

        var msg = netMsg.ReadMessage<SpawnerClientRegistrationMessage>();
        if (!registeredSpawners.ContainsKey(msg.key))
        {
            registeredSpawners[msg.key] = new HashSet<int>();
        }

        registeredSpawners[msg.key].Add(netMsg.connectionId);
        netMsg.Reply();
    }

    // SERVER-ONLY
    // From anyone to the server spawn module
    private void SpawnRequestHandler(InsightNetworkMessage netMsg)
    {
        var msg = netMsg.ReadMessage<SpawnRequestMessage>();
        if (!registeredSpawners.ContainsKey(msg.key) ||
        registeredSpawners[msg.key].Count < 1)
        {
            netMsg.Reply(0, new SpawnDataMessage() { ErrorMsg = "unable to find spawner for key xxx" });
            return;
        }

        // find a client to use from the available list of registered clients
        server.SendToClient(registeredSpawners[msg.key][0], 0, new EmptyReply(), (status, reader) =>
        {
            if (status == CallbackStatus.Ok)
            {
                // var msg = reader.ReadMessage<SpawnDataMessage>()
                // port = msg.port;
                // ip = msg.ip;

                // send a SpawnServerRequestReplyData message, OR 
                // just send on the SpawnDataMessage, if it is appropreate. 
                netMsg.Reply(0, new EmptyReply());
            }
        });
    }

    #endregion

    #region CLIENT

    // CLIENT-ONLY
    // only from the server spawn module to the client spawn module. 
    private void ServerSpawnRequestHandler(InsightNetworkMessage netMsg)
    {
        // do the spawning

        // send a reply with the spawn info - IP/PORT/etc. 
        // in a SpawnDataMessage obj. 
        netMsg.Reply(0, new EmptyReply());
    }

    // CLIENT ONLY
    private void ClientOnConnectedEventHandler()
    {
        if (client && client.isConnected)
        {
            // register with the server. 
            // client.Send(SpawnClientRegistrationMessage.MsgId, new ....)
            // have to include what we can spawn, so maybe the name of the
            // process? Or a key to denote this specific process/app
            client.Send(0, new EmptyReply(), (status, reader) =>
            {
                if (status == CallbackStatus.Ok)
                {
                    // did the server get our registration? 
                }
            });
        }
        else
        {
            UnityEngine.Debug.LogError("Missing client or is not connected...", this);
        }
    }

    #endregion

    #region SPAWNING

    private void SpawnStaticThreads()
    {
        if (StaticProcessCount > 0)
        {
            UnityEngine.Debug.Log("[BasicSpawnerModule]: Spawning Static Zones...");
            for(int i = 0; i < StaticProcessCount; i++)
            {
                SpawnThread(StartingPort + _portUsageCounter);
            }
        }
    }

    private bool SpawnThread(int port)
    {
        if(_processUsageCounter < MaximumProcesses)
        {
#if UNITY_EDITOR
            ProcessPath = EditorPath;
#endif
            // spawn process, pass scene argument
            Process p = new Process();
            p.StartInfo.FileName = System.IO.Path.Combine(ProcessPath, ProcessName);
            //Args to pass: port, scene, AuthCode, UniqueID...
            p.StartInfo.Arguments = ArgsString() +
                " -AssignedPort " + port;
            p.StartInfo.UseShellExecute = false;

            p.EnableRaisingEvents = true; 
            p.Exited += ProcessExitedEventHandler;

            if(p.Start())
            {
                print("[BasicSpawnerModule]: spawning: " + p.StartInfo.FileName + "; args=" + p.StartInfo.Arguments);
                spawnedProcessList.Add(new SpawnedProcesses() { PID = p.Id, ProcessName = ProcessName });
                _processUsageCounter++; //Increment current port after sucessful spawn.
                return true;
            }
            else
            {
                UnityEngine.Debug.LogError("[BasicSpawnerModule] - Process Createion Failed");
                return false;
            }
        }
        else
        {
            UnityEngine.Debug.LogError("[BasicSpawnerModule] - Maximum Process Count Reached");
            return false;
        }
    }

    private void ProcessExitedEventHandler(object sender, EventArgs e)
    {
        Process p = (Process)sender;
        if(p != null)
        {
            UnityEngine.Debug.Log("process exited: " + p.Id, this);
        }
    }

    #endregion

    private static string ArgsString()
    {
        String[] args = System.Environment.GetCommandLineArgs();
        return args != null ? String.Join(" ", args.Skip(1).ToArray()) : "";
    }

    // private static string processPath
    // {
    //     get
    //     {
    //         // note: args are null on android
    //         String[] args = System.Environment.GetCommandLineArgs();
    //         return args != null ? args[0] : "";
    //     }
    // }

    private void OnApplicationQuit()
    {
        //Kill all the children processes
    }
}
