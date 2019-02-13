using UnityEditor;

public class BuildScript
{
    public static string ScenesRoot = "Assets/Insight/Scenes/";
    public static BuildOptions BuildOptions = BuildOptions.Development;
    public static string PrevPath = null;

    [MenuItem("Tools/Build Insight/Build All", false, 0)]
    public static void BuildAllMenu()
    {
        var path = GetPath();
        if (!string.IsNullOrEmpty(path))
        {
            BuildMasterServer(path);
            BuildRemoteSpawner(path);
            BuildGameServer(path);
            BuildPlayerClient(path);
        }
    }

    [MenuItem("Tools/Build Insight/MasterServer", false, 100)]
    public static void BuildMasterServerMenu()
    {
        var path = GetPath();
        if (!string.IsNullOrEmpty(path))
        {
            BuildMasterServer(path);
        }
    }

    [MenuItem("Tools/Build Insight/RemoteSpawner", false, 101)]
    public static void BuildRemoteSpawnerMenu()
    {
        var path = GetPath();
        if (!string.IsNullOrEmpty(path))
        {
            BuildRemoteSpawner(path);
        }
    }

    [MenuItem("Tools/Build Insight/GameServer", false, 102)]
    public static void BuildGameServerMenu()
    {
        var path = GetPath();
        if (!string.IsNullOrEmpty(path))
        {
            BuildGameServer(path);
        }
    }

    [MenuItem("Tools/Build Insight/PlayerClient", false, 103)]
    public static void BuildPlayerClientMenu()
    {
        var path = GetPath();
        if (!string.IsNullOrEmpty(path))
        {
            BuildPlayerClient(path);
        }
    }

    public static void BuildMasterServer(string path)
    {
        var scenes = new[]
        {
        ScenesRoot+"MasterServer.unity"
        };
        PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Enabled;
        PlayerSettings.productName = "MasterServer";
        BuildPipeline.BuildPlayer(scenes, path + "/MasterServer.exe", GetBuildTarget(), BuildOptions);
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

    public static void BuildGameServer(string path)
    {
        var gameServerScenes = new[]
        {
        ScenesRoot+"GameServer.unity",
        //Scene used for MasterServer Demo
        ScenesRoot+"SuperAwesomeGame.unity"
        };
        PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Enabled;
        PlayerSettings.productName = "GameServer";
        BuildPipeline.BuildPlayer(gameServerScenes, path + "/GameServer.exe", GetBuildTarget(), BuildOptions);
    }

    public static void BuildPlayerClient(string path)
    {
        var scenes = new[]
        {
        ScenesRoot+"PlayerClient.unity",
        //Scene used for MasterServer Demo
        ScenesRoot+"SuperAwesomeGame.unity"
        };
        PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Enabled;
        PlayerSettings.productName = "PlayerClient";
        BuildPipeline.BuildPlayer(scenes, path + "/PlayerClient.exe", GetBuildTarget(), BuildOptions);
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