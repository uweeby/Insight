using Insight;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class ProcessSpawner : InsightModule
{
    InsightServer server;
    InsightClient client;

    [Header("Network")]
    [Tooltip("NetworkAddress that spawned processes will use")]
    public string SpawnerNetworkAddress = "localhost";
    [Tooltip("Port that will be used by the NetworkManager in the spawned game")]
    public int StartingNetworkPort = 7777; //Default port of the NetworkManager.
    private int _portCounter;

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
            client.RegisterHandler((short)MsgId.RequestSpawn, HandleSpawnRequest);
        }
        if (server)
        {
            server.RegisterHandler((short)MsgId.RequestSpawn, HandleSpawnRequest);
        }
    }

    private void HandleSpawnRequest(InsightNetworkMessage netMsg)
    {
        RequestSpawn message = netMsg.ReadMessage<RequestSpawn>();

        if(SpawnThread(message))
        {
            netMsg.Reply((short)MsgId.RequestSpawn, new RequestSpawn() {
                SceneName = message.SceneName,
                NetworkAddress = SpawnerNetworkAddress,
                UniqueID = Guid.NewGuid().ToString() });
        }
        else
        {
            netMsg.Reply((short)MsgId.Error, new ErrorMsg() { Text = "[ProcessSpawner] - Spawn failed" });
        }
    }

    private bool SpawnThread(RequestSpawn spawnProperties)
    {
        if(_processUsageCounter >= MaximumProcesses)
        {
            UnityEngine.Debug.LogError("[ProcessSpawner] - Maximum Process Count Reached");
            return false;
        }

        //Find process name from AlaisStruct
        foreach (ProcessStruct process in processArray)
        {
            if(process.Alias.Equals(spawnProperties.ProcessAlias))
            {
                Process p = new Process();
                p.StartInfo.FileName = ProcessPath + process.Path;
                //Args to pass: Port, Scene, UniqueID...
                p.StartInfo.Arguments = ArgsString() +
                    " -NetworkAddress " + SpawnerNetworkAddress + 
                    " -NetworkPort " + (StartingNetworkPort + _portCounter) +
                    " -NetworkPort " + spawnProperties.SceneName +
                    " -UniqueID " + spawnProperties.UniqueID;

                if (p.Start())
                {
                    print("[ProcessSpawner]: spawning: " + p.StartInfo.FileName + "; args=" + p.StartInfo.Arguments);

                    //Increment current port and process counter after sucessful spawn.
                    _portCounter++;
                    _processUsageCounter++; 

                    //If registered to a master. Notify it of the current thread utilization
                    if(client != null)
                    {
                        client.Send((short)MsgId.SpawnerStatus, new SpawnerStatus() { CurrentThreads = _processUsageCounter });
                    }
                    return true;
                }
                else
                {
                    UnityEngine.Debug.LogError("[ProcessSpawner] - Process Createion Failed.");
                    return false;
                }
            }
        }
        UnityEngine.Debug.LogError("Process Alias not found");
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
