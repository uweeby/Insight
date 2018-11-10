using System;
using System.Diagnostics;
using System.Linq;
using Insight;
using UnityEngine;

public class SpawnerModule : InsightModule
{
    InsightServer insightServer;

    public string ProcessPath;
    public string ProcessName;
    public int MaxProcesses;
    private int processCount;

    // paths to the scenes to spawn
    public string[] scenePathsToSpawn;

    [Header("AliveCheck")]
    [Range(1, 10)] public float writeInterval = 1;
    [Range(2, 10)] public float timeoutMultiplier = 3;
    public float timeoutInterval { get { return writeInterval * timeoutMultiplier; } }

    public override void Initialize(InsightServer server)
    {
        insightServer = server;
        SpawnStaticZones();
    }

    public override void RegisterHandlers()
    {
        
    }

    public void SpawnStaticZones()
    {
        print("[Zones]: Spawn Static Zones...");
        foreach (string scenePath in scenePathsToSpawn)
        {
            SpawnZone(scenePath);
        }
    }

    private void SpawnZone(string SceneName)
    {
        if (processCount >= MaxProcesses)
        {
            print("[Zones]: ERROR: Spawner at Max Processes");
            //return;
        }

        // spawn process, pass scene argument
        Process p = new Process();
        p.StartInfo.FileName = ProcessPath + ProcessName;
        //Args to pass: port, scene, AuthCode, UniqueID...
        p.StartInfo.Arguments = ArgsString() + " -ScenePath " + SceneName;
        print("[Zones]: spawning: " + p.StartInfo.FileName + "; args=" + p.StartInfo.Arguments);
        p.Start();

        processCount++;
    }

    //public static string ParseScenePathFromArgs()
    //{
    //    // note: args are null on android
    //    String[] args = System.Environment.GetCommandLineArgs();
    //    if (args != null)
    //    {
    //        int index = args.ToList().FindIndex(arg => arg == "-scenePath");
    //        return 0 <= index && index < args.Length - 1 ? args[index + 1] : "";
    //    }
    //    return "";
    //}

    public static string ArgsString()
    {
        // note: first arg is always process name or empty
        // note: args are null on android
        String[] args = System.Environment.GetCommandLineArgs();
        return args != null ? String.Join(" ", args.Skip(1).ToArray()) : "";
    }

    public static string processPath
    {
        get
        {
            // note: args are null on android
            String[] args = System.Environment.GetCommandLineArgs();
            return args != null ? args[0] : "";
        }
    }
}