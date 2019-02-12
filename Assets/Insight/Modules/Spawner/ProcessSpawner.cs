﻿using Insight;
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

    public List<RunningProcessStruct> spawnerProcesses = new List<RunningProcessStruct>();

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
                client.Send((short)MsgId.RegisterSpawner, new RegisterSpawnerMsg() {
                    UniqueID = "",
                    MaxThreads = MaximumProcesses }); //Can provide a password to authenticate to the master as a trusted spawner
                registrationComplete = true;
            }
        }
    }

    void RegisterHandlers()
    {
        if (client)
        {
            client.RegisterHandler((short)MsgId.RequestSpawn, HandleSpawnRequest);
            client.RegisterHandler((short)MsgId.KillSpawn, HandleKillSpawn);
        }
        if (server)
        {
            server.RegisterHandler((short)MsgId.RequestSpawn, HandleSpawnRequest);
            server.RegisterHandler((short)MsgId.KillSpawn, HandleKillSpawn);
        }
    }

    private void HandleSpawnRequest(InsightNetworkMessage netMsg)
    {
        RequestSpawnMsg message = netMsg.ReadMessage<RequestSpawnMsg>();

        if (SpawnThread(message))
        {
            //This is the reply to the spawn request. Is it even needed?

            //netMsg.Reply((short)MsgId.RequestSpawn, new RequestSpawn() {
            //    SceneName = message.SceneName,
            //    NetworkAddress = SpawnerNetworkAddress,
            //    UniqueID = Guid.NewGuid().ToString() });
        }
        else
        {
            netMsg.Reply((short)MsgId.Error, new ErrorMsg() { Text = "[ProcessSpawner] - Spawn failed" });
        }
    }

    private void HandleKillSpawn(InsightNetworkMessage netMsg)
    {
        KillSpawnMsg message = netMsg.ReadMessage<KillSpawnMsg>();

        foreach(RunningProcessStruct process in spawnerProcesses)
        {
            if(process.uniqueID.Equals(message.UniqueID))
            {
                process.process.Kill();
                break;
            }
        }
    }

    private bool SpawnThread(RequestSpawnMsg spawnProperties)
    {
        if(_processUsageCounter >= MaximumProcesses)
        {
            UnityEngine.Debug.LogError("[ProcessSpawner] - Maximum Process Count Reached");
            return false;
        }

        if(string.IsNullOrEmpty(spawnProperties.UniqueID))
        {
            //If a UniqueID was not provided add one for GameResitration
            spawnProperties.UniqueID = Guid.NewGuid().ToString();

            UnityEngine.Debug.LogWarning("[ProcessSpawner] - UniqueID was not provided for spawn. Generating: " + spawnProperties.UniqueID);
        }

        //If not ProcessAlias is provided. Use the 0th entry as default.
        if(string.IsNullOrEmpty(spawnProperties.ProcessAlias))
        {
            spawnProperties.ProcessAlias = processArray[0].Alias;
        }

        bool spawnComplete = false;
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
                    " -SceneName " + spawnProperties.SceneName +
                    " -UniqueID " + spawnProperties.UniqueID; //What to do if the UniqueID or any other value is null??

                if (p.Start())
                {
                    spawnComplete = true;
                    print("[ProcessSpawner]: spawning: " + p.StartInfo.FileName + "; args=" + p.StartInfo.Arguments);

                    //Increment current port and process counter after sucessful spawn.
                    _portCounter++;
                    _processUsageCounter++; 

                    //If registered to a master. Notify it of the current thread utilization
                    if(client != null)
                    {
                        client.Send((short)MsgId.SpawnerStatus, new SpawnerStatusMsg() { CurrentThreads = _processUsageCounter });
                    }

                    spawnerProcesses.Add(new RunningProcessStruct() { process = p, pid = p.Id, uniqueID = spawnProperties.UniqueID });
                    break;
                }
                else
                {
                    UnityEngine.Debug.LogError("[ProcessSpawner] - Process Createion Failed.");
                    return false;
                }
            }
        }

        if(!spawnComplete)
        {
            UnityEngine.Debug.LogError("Process Alias not found");
            return false;
        }
        return true;
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

[Serializable]
public struct RunningProcessStruct
{
    public Process process;
    public int pid;
    public string uniqueID;
}
