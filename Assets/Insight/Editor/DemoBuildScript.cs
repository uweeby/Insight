using UnityEditor;

public class DemoBuildScript
{
    public static string ScenesRoot = "Assets/Insight/Scenes/";
    public static BuildOptions BuildOptions = BuildOptions.Development;
    public static string PrevPath = null;

    [MenuItem("Tools/Build/MatchMaker", false, 100)]
    public static void BuildMatchMakerMenu()
    {
        var path = GetPath();
        if (!string.IsNullOrEmpty(path))
        {
            BuildMatchMaker(path);
        }
    }

    [MenuItem("Tools/Build/RemoteSpawner", false, 101)]
    public static void BuildRemoteSpawnerMenu()
    {
        var path = GetPath();
        if (!string.IsNullOrEmpty(path))
        {
            BuildRemoteSpawner(path);
        }
    }

    [MenuItem("Tools/Build/ManagedGameServer", false, 102)]
    public static void BuildManagedGameServerMenu()
    {
        var path = GetPath();
        if (!string.IsNullOrEmpty(path))
        {
            BuildManagedGameServer(path);
        }
    }

    [MenuItem("Tools/Build/MatchMaking Player", false, 103)]
    public static void BuildMatchMakingPlayerMenu()
    {
        var path = GetPath();
        if (!string.IsNullOrEmpty(path))
        {
            BuildMatchMakingPlayer(path);
        }
    }

    public static void BuildMatchMaker(string path)
    {
        var scenes = new[]
        {
            ScenesRoot+"MatchMaking.unity"
        };
        PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Enabled;
        PlayerSettings.productName = "MatchMaking";
        BuildPipeline.BuildPlayer(scenes, path + "/MatchMaking.exe", GetBuildTarget(), BuildOptions);
    }

    public static void BuildRemoteSpawner(string path)
    {
        var gameServerScenes = new[]
        {
            ScenesRoot+"RemoteSpawner.unity"
        };
        PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Enabled;
        PlayerSettings.productName = "RemoteSpawner";
        BuildPipeline.BuildPlayer(gameServerScenes, path + "/RemoteSpawner.exe", GetBuildTarget(), BuildOptions);
    }

    public static void BuildManagedGameServer(string path)
    {
        var gameServerScenes = new[]
        {
            ScenesRoot+"ManagedGameServer.unity",
            //Scene used for MatchMaking Demo
            ScenesRoot+"SuperAwesomeGame.unity"
        };
        PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Enabled;
        PlayerSettings.productName = "ManagedGameServer";
        BuildPipeline.BuildPlayer(gameServerScenes, path + "/ManagedGameServer.exe", GetBuildTarget(), BuildOptions);
    }

    public static void BuildMatchMakingPlayer(string path)
    {
        var scenes = new[]
        {
            ScenesRoot+"MatchMakingPlayer.unity",
            //Add all scenes from game
            ScenesRoot+"SuperAwesomeGame.unity"
        };
        PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Enabled;
        PlayerSettings.productName = "MatchMakingPlayer";
        BuildPipeline.BuildPlayer(scenes, path + "/MatchMakingPlayer.exe", GetBuildTarget(), BuildOptions);
    }

    #region Helpers
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

    public static BuildTarget GetBuildTarget()
    {
        return EditorUserBuildSettings.activeBuildTarget;
    }
    #endregion
}
