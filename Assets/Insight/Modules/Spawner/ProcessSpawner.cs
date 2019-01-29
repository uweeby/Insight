using Insight;
using System;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class ProcessSpawner : InsightModule
{
    InsightServer server;
    InsightClient client;

    [Header("Paths")]
    public string EditorPath;
    public string ProcessPath;

    [Header("Standalone")]
    public ProcessStruct[] processArray;

    [Header("Threads")]
    public int MaximumProcesses = 5;

    private int _processUsageCounter;
    private bool registrationComplete;

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

    void Start()
    {
#if UNITY_EDITOR
        ProcessPath = EditorPath;
#endif
    }

    void Update()
    {
        //Used only if acting as a ChildSpawner under a MasterServer
        if (client && !registrationComplete)
        {
            if (client.isConnected)
            {
                UnityEngine.Debug.LogWarning("[ProcessSpawner] - Registering to Master");
                client.Send((short)MsgId.RegisterSpawner, new RegisterSpawnerMsg() { UniqueID = "" }); //Can provide a password to authenticate to the master as a trusted spawner
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

        if(SpawnThread(message.SpawnAlias))
        {
            netMsg.Reply((short)MsgId.RequestSpawn, new RequestSpawn() { GameName = message.GameName, NetworkAddress = "127.0.0.1", UniqueID = Guid.NewGuid().ToString() });
        }
        else
        {
            UnityEngine.Debug.LogError("[ProcessSpawner] - Spawn failed. This needs better error handling");
            netMsg.Reply((short)MsgId.Error, new ErrorMsg() { Text = "[ProcessSpawner] - Spawn failed. This needs better error handling" });
        }
    }

    private bool SpawnThread(string ProcessAlias)
    {
        if(_processUsageCounter >= MaximumProcesses)
        {
            UnityEngine.Debug.LogError("[ProcessSpawner] - Maximum Process Count Reached");
            return false;
        }

        //Find process name from AlaisStruct
        foreach (ProcessStruct process in processArray)
        {
            if(process.Alias.Equals(ProcessAlias))
            {
                Process p = new Process();
                p.StartInfo.FileName = ProcessPath + process.Path;
                //Args to pass: Port, Scene, UniqueID...
                p.StartInfo.Arguments = ArgsString();

                if (p.Start())
                {
                    print("[ProcessSpawner]: spawning: " + p.StartInfo.FileName + "; args=" + p.StartInfo.Arguments);
                    _processUsageCounter++; //Increment current port after sucessful spawn.
                    return true;
                }
                else
                {
                    UnityEngine.Debug.LogError("[ProcessSpawner] - Process Createion Failed");
                    return false;
                }
            }
        }
        return false;
    }

    private static string ArgsString()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        return args != null ? string.Join(" ", args.Skip(1).ToArray()) : "";
    }
}

[Serializable]
public struct ProcessStruct
{
    public string Alias;
    public string Path;
}