using Insight;
using System;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class BasicSpawner : InsightModule
{
    InsightServer server;
    InsightClient client;

    [Header("Paths")]
    public string EditorPath;
    public string ProcessPath;

    [Header("Standalone")]
    public string ProcessName;

    [Header("Threads")]
    public int MaximumProcesses = 5;
    private int _processUsageCounter;
    private bool registrationComplete;

    //public List<SpawnedProcesses> spawnedProcessList = new List<SpawnedProcesses>();

    public override void Initialize(InsightServer server, ModuleManager manager)
    {
        this.server = server;
        RegisterHandlers();
    }

    public override void Initialize(InsightClient client, ModuleManager manager)
    {
        this.client = client;
        RegisterHandlers();
    }

    void Update()
    {
        if (client && !registrationComplete) //Needs to be registered to MasterServer
        {
            if (client.isConnected)
            {
                UnityEngine.Debug.LogWarning("[Basic Spawner Module] - Registering to Master");
                client.Send((short)MsgId.RegisterSpawner, new RegisterSpawnerMsg() { UniqueID = "" });
                registrationComplete = true;
            }
        }
    }

    void RegisterHandlers()
    {
        if (client)
        {
            client.RegisterHandler((short)MsgId.RequestSpawn, this.HandleSpawnRequest);
        }
        if (server)
        {
            server.RegisterHandler((short)MsgId.RequestSpawn, this.HandleSpawnRequest);
        }
    }

    private void HandleSpawnRequest(InsightNetworkMessage netMsg)
    {
        RequestSpawn message = netMsg.ReadMessage<RequestSpawn>();

        if (message.NetworkPort != 0)
        {
            SpawnThread(message.NetworkPort);
            netMsg.Reply((short)MsgId.RequestSpawn, new RequestSpawn() { GameName = message.GameName, NetworkAddress = "127.0.0.1", NetworkPort = message.NetworkPort, UniqueID = Guid.NewGuid().ToString() });
        }
        else
        {
            UnityEngine.Debug.LogWarning("[Basic Spawner Module] - Port not provided with HandleSpawnRequest. Using default 7777");
            SpawnThread(7777);
            netMsg.Reply((short)MsgId.RequestSpawn, new RequestSpawn() { GameName = message.GameName, NetworkAddress = "127.0.0.1", NetworkPort = 7777, UniqueID = Guid.NewGuid().ToString() });
        }
    }


    public bool RequestSpawn(int port)
    {
        return SpawnThread(port);
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
            p.StartInfo.FileName = ProcessPath + ProcessName;
            //Args to pass: port, scene, AuthCode, UniqueID...
            p.StartInfo.Arguments = ArgsString() +
                " -AssignedPort " + port;

            if(p.Start())
            {
                print("[BasicSpawnerModule]: spawning: " + p.StartInfo.FileName + "; args=" + p.StartInfo.Arguments);
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

    private static string ArgsString()
    {
        String[] args = System.Environment.GetCommandLineArgs();
        return args != null ? String.Join(" ", args.Skip(1).ToArray()) : "";
    }
}
