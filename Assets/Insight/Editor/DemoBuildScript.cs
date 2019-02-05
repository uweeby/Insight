﻿using UnityEditor;

public class DemoBuildScript
{
    public static string ScenesRoot = "Assets/Insight/Scenes/";
    public static BuildOptions BuildOptions = BuildOptions.Development;
    public static string PrevPath = null;

    #region Windows Builds
    [MenuItem("Tools/Build/Windows/BasicGameServer", false, 999)]
    public static void WindowsBuildAllMenu()
    {
        var path = GetPath();
        if (!string.IsNullOrEmpty(path))
        {
            WindowsBuildBasicGameServer(path);
            WindowsBuildManagedGameServer(path);
            WindowsBuildChildSpawner(path);
            WindowsBuildPlayerClient(path);
        }
    }

    [MenuItem("Tools/Build/Windows/BasicGameServer", false, 999)]
    public static void WindowsBuildBasicGameServerMenu()
    {
        var path = GetPath();
        if (!string.IsNullOrEmpty(path))
        {
            WindowsBuildBasicGameServer(path);
        }
    }

    [MenuItem("Tools/Build/Windows/ManagedGameServer", false, 999)]
    public static void WindowsBuildComplexGameServerMenu()
    {
        var path = GetPath();
        if (!string.IsNullOrEmpty(path))
        {
            WindowsBuildManagedGameServer(path);
        }
    }

    [MenuItem("Tools/Build/Windows/ManagedGameServer", false, 999)]
    public static void WindowsBuildChildSpawnerMenu()
    {
        var path = GetPath();
        if (!string.IsNullOrEmpty(path))
        {
            WindowsBuildChildSpawner(path);
        }
    }

    [MenuItem("Tools/Build/Windows/PlayerClient", false, 999)]
    public static void WindowsBuildPlayerClientMenu()
    {
        var path = GetPath();
        if (!string.IsNullOrEmpty(path))
        {
            WindowsBuildPlayerClient(path);
        }
    }

    public static void WindowsBuildBasicGameServer(string path)
    {
        BuildTarget TargetPlatform = BuildTarget.StandaloneWindows;

        var gameServerScenes = new[]
        {
            ScenesRoot+"BasicGameServer.unity"
        };
        PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Enabled;
        PlayerSettings.productName = "BasicGameServer";
        BuildPipeline.BuildPlayer(gameServerScenes, path + "/BasicGameServer.exe", TargetPlatform, BuildOptions);
    }

    public static void WindowsBuildManagedGameServer(string path)
    {
        BuildTarget TargetPlatform = BuildTarget.StandaloneWindows;

        var gameServerScenes = new[]
        {
            ScenesRoot+"ManagedGameServer.unity"
        };
        PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Enabled;
        PlayerSettings.productName = "ManagedGameServer";
        BuildPipeline.BuildPlayer(gameServerScenes, path + "/ManagedGameServer.exe", TargetPlatform, BuildOptions);
    }

    public static void WindowsBuildChildSpawner(string path)
    {
        BuildTarget TargetPlatform = BuildTarget.StandaloneWindows;

        var gameServerScenes = new[]
        {
            ScenesRoot+"ChildSpawner.unity"
        };
        PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Enabled;
        PlayerSettings.productName = "ChildSpawner";
        BuildPipeline.BuildPlayer(gameServerScenes, path + "/ChildSpawner.exe", TargetPlatform, BuildOptions);
    }

    public static void WindowsBuildPlayerClient(string path)
    {
        BuildTarget TargetPlatform = BuildTarget.StandaloneWindows;

        var gameServerScenes = new[]
        {
            ScenesRoot+"PlayerClient.unity"
        };
        PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Enabled;
        PlayerSettings.productName = "PlayerClient";
        BuildPipeline.BuildPlayer(gameServerScenes, path + "/PlayerClient.exe", TargetPlatform, BuildOptions);
    }
    #endregion

    #region Linux Builds
    [MenuItem("Tools/Build/Linux/BasicGameServer", false, 999)]
    public static void LinuxBuildAllMenu()
    {
        var path = GetPath();
        if (!string.IsNullOrEmpty(path))
        {
            LinuxBuildBasicGameServer(path);
            LinuxBuildManagedGameServer(path);
            LinuxBuildChildSpawner(path);
        }
    }

    [MenuItem("Tools/Build/Linux/BasicGameServer", false, 999)]
    public static void LinuxBuildBasicGameServerMenu()
    {
        var path = GetPath();
        if (!string.IsNullOrEmpty(path))
        {
            LinuxBuildBasicGameServer(path);
        }
    }

    [MenuItem("Tools/Build/Linux/ManagedGameServer", false, 999)]
    public static void LinuxBuildComplexGameServerMenu()
    {
        var path = GetPath();
        if (!string.IsNullOrEmpty(path))
        {
            LinuxBuildManagedGameServer(path);
        }
    }

    [MenuItem("Tools/Build/Linux/BasicGameServer", false, 999)]
    public static void LinuxBuildChildSpawnerMenu()
    {
        var path = GetPath();
        if (!string.IsNullOrEmpty(path))
        {
            LinuxBuildChildSpawner(path);
        }
    }

    [MenuItem("Tools/Build/Linux/PlayerClient", false, 999)]
    public static void LinuxBuildPlayerClientMenu()
    {
        var path = GetPath();
        if (!string.IsNullOrEmpty(path))
        {
            LinuxBuildPlayerClient(path);
        }
    }

    public static void LinuxBuildBasicGameServer(string path)
    {
        BuildTarget TargetPlatform = BuildTarget.StandaloneLinux64;

        var gameServerScenes = new[]
        {
            ScenesRoot+"BasicGameServer.unity"
        };
        PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Enabled;
        PlayerSettings.productName = "BasicGameServer";
        BuildPipeline.BuildPlayer(gameServerScenes, path + "Linux/BasicGameServer.x86_64", TargetPlatform, BuildOptions);
    }

    public static void LinuxBuildManagedGameServer(string path)
    {
        BuildTarget TargetPlatform = BuildTarget.StandaloneLinux64;

        var gameServerScenes = new[]
        {
            ScenesRoot+"ManagedGameServer.unity"
        };
        PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Enabled;
        PlayerSettings.productName = "ManagedGameServer";
        BuildPipeline.BuildPlayer(gameServerScenes, path + "/ManagedGameServer.x86_64", TargetPlatform, BuildOptions);
    }

    public static void LinuxBuildChildSpawner(string path)
    {
        BuildTarget TargetPlatform = BuildTarget.StandaloneLinux64;

        var gameServerScenes = new[]
        {
            ScenesRoot+"ChildSpawner.unity"
        };
        PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Enabled;
        PlayerSettings.productName = "ChildSpawner";
        BuildPipeline.BuildPlayer(gameServerScenes, path + "/ChildSpawner.x86_64", TargetPlatform, BuildOptions);
    }

    public static void LinuxBuildPlayerClient(string path)
    {
        BuildTarget TargetPlatform = BuildTarget.StandaloneLinux64;

        var gameServerScenes = new[]
        {
            ScenesRoot+"PlayerClient.unity"
        };
        PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Enabled;
        PlayerSettings.productName = "PlayerClient";
        BuildPipeline.BuildPlayer(gameServerScenes, path + "/PlayerClient.x86_64", TargetPlatform, BuildOptions);
    }
    #endregion


    #region MatchMakingTestBuilds
    //MatchMaking Builds
    [MenuItem("Tools/Build/Windows MatchMaking/Client", false, 999)]
    public static void WindowsBuildMatchMakingClient()
    {
        var path = GetPath();
        if (!string.IsNullOrEmpty(path))
        {
            WindowsBuildMMPlayerClient(path);
        }
    }

    //MatchMaking Builds
    [MenuItem("Tools/Build/Windows MatchMaking/MatchMaker", false, 999)]
    public static void WindowsBuildMatchMakingMatchMaker()
    {
        var path = GetPath();
        if (!string.IsNullOrEmpty(path))
        {
            WindowsBuildMMServer(path);
        }
    }

    //MatchMaking Builds
    [MenuItem("Tools/Build/Windows MatchMaking/MatchedGameServer", false, 999)]
    public static void WindowsBuildMatchMakingMatchedGameServer()
    {
        var path = GetPath();
        if (!string.IsNullOrEmpty(path))
        {
            WindowsBuildMMGame(path);
        }
    }

    //MatchMaking Builds
    public static void WindowsBuildMMPlayerClient(string path)
    {
        ScenesRoot = "Assets/InsightExamples/7. MatchMaking/";
        BuildTarget TargetPlatform = BuildTarget.StandaloneWindows;

        var scenes = new[]
        {
            ScenesRoot+"PlayerClient.unity",
            //Add all scenes from game
            ScenesRoot+"SuperAwesomeGame.unity"
        };
        PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Enabled;
        PlayerSettings.productName = "PlayerClient";
        BuildPipeline.BuildPlayer(scenes, path + "/MM_PlayerClient.exe", TargetPlatform, BuildOptions);
    }

    //MatchMaking Builds
    public static void WindowsBuildMMServer(string path)
    {
        ScenesRoot = "Assets/InsightExamples/7. MatchMaking/";
        BuildTarget TargetPlatform = BuildTarget.StandaloneWindows;

        var scenes = new[]
        {
            ScenesRoot+"MatchMaking.unity"
        };
        PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Enabled;
        PlayerSettings.productName = "MatchMaking";
        BuildPipeline.BuildPlayer(scenes, path + "/MatchMaking.exe", TargetPlatform, BuildOptions);
    }

    //MatchMaking Builds
    public static void WindowsBuildMMGame(string path)
    {
        ScenesRoot = "Assets/InsightExamples/7. MatchMaking/";
        BuildTarget TargetPlatform = BuildTarget.StandaloneWindows;

        var scenes = new[]
        {
            ScenesRoot+"MatchedGameServer.unity",
            //Add all scenes from game
            ScenesRoot+"SuperAwesomeGame.unity"
        };
        PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Enabled;
        PlayerSettings.productName = "MatchedGameServer";
        BuildPipeline.BuildPlayer(scenes, path + "/MatchedGameServer.exe", TargetPlatform, BuildOptions);
    }
    #endregion

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