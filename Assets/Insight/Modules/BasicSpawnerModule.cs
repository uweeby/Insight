using Insight;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class BasicSpawnerModule : InsightModule
{
    [Header("Paths")]
    public string EditorPath;
    public string ProcessPath;

    [Header("Standalone")]
    public string ProcessName;

    [Header("Threads")]
    public int MaximumProcesses = 5;
    private int _processUsageCounter;

    //public List<SpawnedProcesses> spawnedProcessList = new List<SpawnedProcesses>();

    public override void Initialize(InsightCommon insight, ModuleManager manager)
    {
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