using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Insight;
using Mirror;
using UnityEngine;

public class SpawnerModule : InsightModule
{
    InsightServer insightServer;

    public string EditorPath;
    public string ProcessPath;
    public string ZoneProcessName;
    public string LoginProcessName;
    public int MaxProcesses;
    private int processCount;

    public int SpawnerStartingPort;
    private int portCounter;

    // paths to the scenes to spawn
    public string[] scenePathsToSpawn;

    public bool SpawnLoginServer = true;

    [Header("AliveCheck")]
    [Range(1, 10)] public float writeInterval = 1;
    [Range(2, 10)] public float timeoutMultiplier = 3;
    public float timeoutInterval { get { return writeInterval * timeoutMultiplier; } }

    public override void Initialize(InsightServer server)
    {
        portCounter = SpawnerStartingPort;
        insightServer = server;
        SpawnStaticZones();
    }

    public override void RegisterHandlers()
    {
        insightServer.RegisterHandler(ZoneServerSpawnRequest.MsgId, HandleZoneServerSpawnRequest);
    }

    private void HandleZoneServerSpawnRequest(InsightNetworkMessage netMsg)
    {
        ZoneServerSpawnRequest message = netMsg.ReadMessage<ZoneServerSpawnRequest>();

        print("HandleZoneServerSpawnRequest - " + message.SceneName);
    }

    private void SpawnLogin()
    {
        if (!SpawnLoginServer)
            return;

        //Login Server does not use Args
        SpawnZone(LoginProcessName, "");
    }

    private void SpawnZone(string ProcessName, string SceneName)
    {
        if (processCount >= MaxProcesses)
        {
            print("[Zones]: ERROR: Spawner at Max Processes");
            //return;
        }

        //Generate AuthID:
        string _authID = Guid.NewGuid().ToString();

#if UNITY_EDITOR
        ProcessPath = EditorPath;
#endif
        // spawn process, pass scene argument
        Process p = new Process();
        p.StartInfo.FileName = ProcessPath + ProcessName;
        //Args to pass: port, scene, AuthCode, UniqueID...
        p.StartInfo.Arguments = ArgsString() + 
            " -ScenePath " + SceneName + 
            " -UniqueID " + _authID +
            " -AssignedPort " + (portCounter++) +
            " -MasterIp " + "localhost" + 
            " -MasterPort " + insightServer.networkPort;
        print("[Zones]: spawning: " + p.StartInfo.FileName + "; args=" + p.StartInfo.Arguments);
        p.Start();

        processCount++;

        //registeredServersList.Add(new RegisteredServers() { AuthID = _authID, ServerType = serverType });
    }

    private void SpawnStaticZones()
    {
        SpawnLogin();

        print("[Zones]: Spawn Static Zones...");
        foreach (string scenePath in scenePathsToSpawn)
        {
            SpawnZone(ZoneProcessName, scenePath);
        }
    }

    private static string ArgsString()
    {
        // note: first arg is always process name or empty
        // note: args are null on android
        String[] args = System.Environment.GetCommandLineArgs();
        return args != null ? String.Join(" ", args.Skip(1).ToArray()) : "";
    }

    private static string processPath
    {
        get
        {
            // note: args are null on android
            String[] args = System.Environment.GetCommandLineArgs();
            return args != null ? args[0] : "";
        }
    }
}

public class ZoneServerSpawnRequest : MessageBase
{
    public static short MsgId = 10001;
    public string SceneName;
    public bool IsStatic;
    public bool IsPublic;
    public string Password;
}