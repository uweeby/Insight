using UnityEditor;

public class MatchMakingBuild
{
    public static string ScenesRoot = "Assets/Insight/Scenes/";
    public static BuildOptions BuildOptions = BuildOptions.Development;
    public static string PrevPath = null;

    [MenuItem("Tools/Build/Windows MatchMaking/Build All", false, 999)]
    public static void WindowsBuildAllMatchMaking()
    {
        var path = GetPath();
        if (!string.IsNullOrEmpty(path))
        {
            WindowsBuildMMPlayerClient(path);
            WindowsBuildMMServer(path);
            WindowsBuildMMGame(path);
        }
    }

    [MenuItem("Tools/Build/Windows MatchMaking/Client", false, 999)]
    public static void WindowsBuildMatchMakingClient()
    {
        var path = GetPath();
        if (!string.IsNullOrEmpty(path))
        {
            WindowsBuildMMPlayerClient(path);
        }
    }

    [MenuItem("Tools/Build/Windows MatchMaking/MatchMaker", false, 999)]
    public static void WindowsBuildMatchMakingMatchMaker()
    {
        var path = GetPath();
        if (!string.IsNullOrEmpty(path))
        {
            WindowsBuildMMServer(path);
        }
    }

    [MenuItem("Tools/Build/Windows MatchMaking/MatchedGameServer", false, 999)]
    public static void WindowsBuildMatchMakingMatchedGameServer()
    {
        var path = GetPath();
        if (!string.IsNullOrEmpty(path))
        {
            WindowsBuildMMGame(path);
        }
    }

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
