using UnityEditor;

/// <summary>
/// Instead of editing this script, I would recommend to write your own
/// (or copy and change it). Otherwise, your changes will be overwriten when you
/// update project :)
/// </summary>
public class DemoBuildScript
{
    /// <summary>
    /// Have in mind that if you change it, it might take "a while" 
    /// for the editor to pick up changes 
    /// </summary>
    public static string ScenesRoot = "Assets/Scenes/";

    /// <summary>
    /// Build with "Development" flag, so that we can see the console if something 
    /// goes wrong
    /// </summary>
    public static BuildOptions BuildOptions = BuildOptions.Development;

    public static string PrevPath = null;

    [MenuItem("Tools/Build/SpawnerExample/Spawner", false, 11)]
    public static void BuildSpawnerMenu()
    {
        var path = GetPath();
        if (!string.IsNullOrEmpty(path))
        {
            BuildSpawner(path);
        }
    }

    [MenuItem("Tools/Build/SpawnerExample/GameServer", false, 11)]
    public static void BuildGameServerMenu()
    {
        var path = GetPath();
        if (!string.IsNullOrEmpty(path))
        {
            BuildGameServer(path);
        }
    }

    /// <summary>
    /// Creates a build for ZoneServer
    /// </summary>
    /// <param name="path"></param>
    public static void BuildSpawner(string path)
    {
        BuildTarget TargetPlatform = BuildTarget.StandaloneWindows;

        var gameServerScenes = new[]
        {
            ScenesRoot+"SpawnerExample/Spawner.unity"
        };
        PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.HiddenByDefault;
        PlayerSettings.productName = "Spawner";
        BuildPipeline.BuildPlayer(gameServerScenes, path + "/Spawner.exe", TargetPlatform, BuildOptions);
    }

    /// <summary>
    /// Creates a build for ZoneServer
    /// </summary>
    /// <param name="path"></param>
    public static void BuildGameServer(string path)
    {
        BuildTarget TargetPlatform = BuildTarget.StandaloneWindows;

        var gameServerScenes = new[]
        {
            ScenesRoot+"SpawnerExample/GameServer.unity"
        };
        PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.HiddenByDefault;
        PlayerSettings.productName = "GameServer";
        BuildPipeline.BuildPlayer(gameServerScenes, path + "/GameServer.exe", TargetPlatform, BuildOptions);
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