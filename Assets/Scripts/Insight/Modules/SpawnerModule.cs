using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Insight;
using UnityEngine;

public class SpawnerModule : InsightModule
{
    InsightCommon insightServer;

    [Header("Paths")]
    public string EditorPath;
    public string ProcessPath;

    [Header("LoginServer")]
    public string LoginProcessName = "LoginServer.exe";
    public bool AutoStartLoginServer = true;
    public int LoginServerPort = 7070;

    [Header("ChatServer")]
    public string ChatProcessName = "ChatServer.exe";
    public bool AutoStartChatServer = true;
    public int ChatServerPort = 7000;

    [Header("ZoneServer")]
    public string ZoneServerProcessName = "ZoneServer.exe";
    public bool AutoStartZoneServers = true;
    public int MaxZoneServerProcesses = 5;
    private int _zoneServerProcessCount = 0;
    public int ZoneServerStartingPort = 7777;
    private int _zoneServerPortCounter = 0;

    public string[] staticScenePaths;

    public List<SpawnedProcesses> processDict = new List<SpawnedProcesses>();

    [Header("AliveCheck")]
    [Range(1, 10)] public float writeInterval = 1;
    [Range(2, 10)] public float timeoutMultiplier = 3;
    public float timeoutInterval { get { return writeInterval * timeoutMultiplier; } }

    public override void Initialize(InsightCommon server)
    {
        insightServer = server;

        //LoginServer
        SpawnLoginServer();

        //ChatServer
        SpawnChatServer();

        //ZoneServer
        SpawnStaticZones();
    }

    public override void RegisterHandlers()
    {

    }

    public ZoneContainer SpawnZone(string SceneName)
    {
        if (MaxZoneServerProcesses <= _zoneServerProcessCount)
        {
            print("Error: Over the ZoneServer thread limit---------------------------------");
            //return new ZoneContainer();
            return SpawnThread(ZoneServerProcessName, SceneName, _zoneServerPortCounter++);
        }
        else
        {
            return SpawnThread(ZoneServerProcessName, SceneName, _zoneServerPortCounter++);
        }
    }

    private void SpawnLoginServer()
    {
        if (!AutoStartLoginServer)
            return;

        print("[LoginServer Start]");

        //Login Server does not use Args
        SpawnThread(LoginProcessName, "", LoginServerPort);
    }

    private void SpawnChatServer()
    {
        if (!AutoStartChatServer)
            return;

        print("[ChatServer Start]");

        //Chat Server does not use Args
        SpawnThread(ChatProcessName, "", ChatServerPort);
    }

    private void SpawnStaticZones()
    {
        if (!AutoStartZoneServers)
            return;

        print("[ZoneServer]: Spawning Static Zones...");

        _zoneServerPortCounter = ZoneServerStartingPort;

        foreach (string scenePath in staticScenePaths)
        {
            SpawnThread(ZoneServerProcessName, scenePath, _zoneServerPortCounter++);
        }
    }

    private ZoneContainer SpawnThread(string ProcessName, string SceneName, int Port)
    {
#if UNITY_EDITOR
        ProcessPath = EditorPath;
#endif

        ZoneContainer container = new ZoneContainer();
        container.SceneName = SceneName;
        container.UniqueID = Guid.NewGuid().ToString();
        container.NetworkPort = Port;

        // spawn process, pass scene argument
        Process p = new Process();
        p.StartInfo.FileName = ProcessPath + ProcessName;
        //Args to pass: port, scene, AuthCode, UniqueID...
        p.StartInfo.Arguments = ArgsString() +
            " -ScenePath " + SceneName +
            " -UniqueID " + container.UniqueID +
            " -AssignedPort " + (Port) +
            " -MasterIp " + "localhost" +
            " -MasterPort " + insightServer.networkPort;
        print("[Zones]: spawning: " + p.StartInfo.FileName + "; args=" + p.StartInfo.Arguments);
        p.Start();

        processDict.Add(new SpawnedProcesses() { PID = p.Id, ProcessName = ProcessName });

        return container;
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

    private void OnApplicationQuit()
    {
        //Kill all the children processes
    }
}

[Serializable]
public struct SpawnedProcesses
{
    public int PID;
    public string ProcessName;
}