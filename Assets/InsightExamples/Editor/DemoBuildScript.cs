using UnityEditor;

public class DemoBuildScript
{
    public static string ScenesRoot = "Assets/InsightExamples/";
    public static BuildOptions BuildOptions = BuildOptions.Development;
    public static string PrevPath = null;

    #region Windows Builds
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

    public static void WindowsBuildBasicGameServer(string path)
    {
        BuildTarget TargetPlatform = BuildTarget.StandaloneWindows;

        var gameServerScenes = new[]
        {
            ScenesRoot+"4. Spawner/BasicGameServer.unity"
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
            ScenesRoot+"5. MasterServer/ManagedGameServer.unity"
        };
        PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Enabled;
        PlayerSettings.productName = "ManagedGameServer";
        BuildPipeline.BuildPlayer(gameServerScenes, path + "/ManagedGameServer.exe", TargetPlatform, BuildOptions);
    }
    #endregion

    #region Linux Builds
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

    public static void LinuxBuildBasicGameServer(string path)
    {
        BuildTarget TargetPlatform = BuildTarget.StandaloneLinux64;

        var gameServerScenes = new[]
        {
            ScenesRoot+"4. Spawner/BasicGameServer.unity"
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
            ScenesRoot+"5. MasterServer/ManagedGameServer.unity"
        };
        PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Enabled;
        PlayerSettings.productName = "ManagedGameServer";
        BuildPipeline.BuildPlayer(gameServerScenes, path + "/ManagedGameServer.x86_64", TargetPlatform, BuildOptions);
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
