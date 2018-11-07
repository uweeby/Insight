using UnityEditor;
using System;
using System.Collections.Generic;
using UnityEngine;

public class BuildScript
{
    public static string ScenesRoot = "Assets/Scenes/";

    public static BuildOptions BuildOptions = BuildOptions.Development;

    public static string PrevPath = null;

    [MenuItem("Tools/Build/Build All", false, 0)]
    public static void BuildGame()
    {
        var path = GetPath();
        if (string.IsNullOrEmpty(path))
            return;

        BuildMaster(path);
        BuildClient(path);
        BuildZoneServer(path);
    }

    public static void BuildClient(string path)
    {
        BuildTarget TargetPlatform = BuildTarget.StandaloneWindows;

        var clientScenes = new[]
        {
            ScenesRoot+ "Client.unity",
            // Add all the game scenes
        };
        BuildPipeline.BuildPlayer(clientScenes, path + "/Client.exe", TargetPlatform, BuildOptions);
    }

    public static void BuildMaster(string path)
    {
        BuildTarget TargetPlatform = BuildTarget.StandaloneWindows;

        var masterScenes = new[]
        {
            ScenesRoot+ "MasterServer.unity"
        };
        BuildPipeline.BuildPlayer(masterScenes, path + "/MasterServer.exe", TargetPlatform, BuildOptions);
    }

    public static void BuildZoneServer(string path)
    {
        BuildTarget TargetPlatform = BuildTarget.StandaloneWindows;
        
        var gameServerScenes = new[]
        {
            ScenesRoot+"ZoneServer.unity",
            // Add all the game scenes
        };
        BuildPipeline.BuildPlayer(gameServerScenes, path + "/ZoneServer.exe", TargetPlatform, BuildOptions);
    }

    public static void BuildJenkins()
    {
        BuildZoneServer("./Builds");
    }

    [MenuItem("Tools/Build/Build Master", false, 11)]
    public static void BuildMasterMenu()
    {
        var path = GetPath();
        if (!string.IsNullOrEmpty(path))
        {
            BuildMaster(path);
        }
    }

    [MenuItem("Tools/Build/Build Client", false, 11)]
    public static void BuildClientMenu()
    {
        var path = GetPath();
        if (!string.IsNullOrEmpty(path))
        {
            BuildClient(path);
        }
    }

    [MenuItem("Tools/Build/Build Zone Server", false, 11)]
    public static void BuildZoneServerMenu()
    {
        var path = GetPath();
        if (!string.IsNullOrEmpty(path))
        {
            BuildZoneServer(path);
        }
    }

    public static string GetPath()
    {
        var prevPath = EditorPrefs.GetString("msf.buildPath", "");
        string path = EditorUtility.SaveFolderPanel("Choose Location for binaries", prevPath, "");

        if (!string.IsNullOrEmpty(path))
        {
            EditorPrefs.SetString("msf.buildPath", path);
        }
        return path;
    }
}