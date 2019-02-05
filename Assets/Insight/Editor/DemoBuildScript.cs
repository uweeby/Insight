using UnityEditor;

public class DemoBuildScript
{
    public static string ScenesRoot = "Assets/Insight/Scenes/";
    public static BuildOptions BuildOptions = BuildOptions.Development;
    public static string PrevPath = null;

    #region Windows Builds
    [MenuItem("Tools/Build/Windows/Build All", false, 999)]
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

    [MenuItem("Tools/Build/Windows/ChildSpawner", false, 999)]
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
    [MenuItem("Tools/Build/Linux/Build All", false, 999)]
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

    [MenuItem("Tools/Build/Linux/ChildSpawner", false, 999)]
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
