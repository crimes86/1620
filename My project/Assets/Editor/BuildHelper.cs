using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;

/// <summary>
/// Build helper for 1620 - creates client and server builds
/// </summary>
public class BuildHelper
{
    private const string BUILD_FOLDER = "Builds";

    [MenuItem("Tools/1620/Build/Windows Server (Local Test)")]
    public static void BuildWindowsServer()
    {
        string path = Path.Combine(BUILD_FOLDER, "WindowsServer", "1620Server.exe");
        BuildServer(path, BuildTarget.StandaloneWindows64);
    }

    [MenuItem("Tools/1620/Build/Linux Server (Droplet)")]
    public static void BuildLinuxServer()
    {
        string path = Path.Combine(BUILD_FOLDER, "LinuxServer", "1620Server");
        BuildServer(path, BuildTarget.StandaloneLinux64);
    }

    [MenuItem("Tools/1620/Build/Windows Client")]
    public static void BuildWindowsClient()
    {
        string path = Path.Combine(BUILD_FOLDER, "WindowsClient", "1620.exe");
        BuildClient(path, BuildTarget.StandaloneWindows64);
    }

    static void BuildServer(string path, BuildTarget target)
    {
        Debug.Log($"[1620] Building server for {target}...");

        string directory = Path.GetDirectoryName(path);
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = GetScenes(),
            locationPathName = path,
            target = target,
            subtarget = (int)StandaloneBuildSubtarget.Server,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[1620] Server build succeeded: {path}");
            Debug.Log($"[1620] Size: {report.summary.totalSize / 1024 / 1024} MB");

            // Open folder
            EditorUtility.RevealInFinder(path);
        }
        else
        {
            Debug.LogError($"[1620] Server build failed: {report.summary.result}");
        }
    }

    static void BuildClient(string path, BuildTarget target)
    {
        Debug.Log($"[1620] Building client for {target}...");

        string directory = Path.GetDirectoryName(path);
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = GetScenes(),
            locationPathName = path,
            target = target,
            subtarget = (int)StandaloneBuildSubtarget.Player,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[1620] Client build succeeded: {path}");
            EditorUtility.RevealInFinder(path);
        }
        else
        {
            Debug.LogError($"[1620] Client build failed: {report.summary.result}");
        }
    }

    static string[] GetScenes()
    {
        var scenes = EditorBuildSettings.scenes;
        string[] paths = new string[scenes.Length];
        for (int i = 0; i < scenes.Length; i++)
            paths[i] = scenes[i].path;
        return paths;
    }

    [MenuItem("Tools/1620/Build/Run Local Server")]
    public static void RunLocalServer()
    {
        string serverPath = Path.GetFullPath(Path.Combine(BUILD_FOLDER, "WindowsServer", "1620Server.exe"));

        if (!File.Exists(serverPath))
        {
            Debug.LogError("[1620] Server not built! Use Tools > 1620 > Build > Windows Server first.");
            return;
        }

        Debug.Log("[1620] Starting local server...");
        System.Diagnostics.Process.Start(serverPath, "-batchmode -nographics -logFile server.log");
        Debug.Log("[1620] Server started. Connect from editor using 'Client' button.");
    }
}
