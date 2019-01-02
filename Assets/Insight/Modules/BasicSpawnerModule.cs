using Insight;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class BasicSpawnerModule : InsightModule
{
    InsightClient insight;
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

    // [Header("AliveCheck")]
    // [Range(1, 10)] public float writeInterval = 1;
    // [Range(2, 10)] public float timeoutMultiplier = 3;
    // public float timeoutInterval { get { return writeInterval * timeoutMultiplier; } }

    public override void Initialize(InsightClient insight, ModuleManager manager)
    {
        this.insight = insight;
        this.manager = manager;

        SpawnStaticThreads();
    }

    public override void RegisterHandlers()
    {

    }

    public bool RequestSpawn(int port)
    {
        return SpawnThread(port);
    }

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
            p.StartInfo.FileName = ProcessPath + ProcessName;
            //Args to pass: port, scene, AuthCode, UniqueID...
            p.StartInfo.Arguments = ArgsString() +
                " -AssignedPort " + port;

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

[Serializable]
public struct SpawnedProcesses
{
    public int PID;
    public string ProcessName;
}