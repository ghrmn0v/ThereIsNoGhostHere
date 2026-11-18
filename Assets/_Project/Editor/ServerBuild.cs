using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ServerBuild
{
    [MenuItem("Build/Windows Dedicated Server")]
    public static void Build()
    {
        var options = new BuildPlayerOptions
        {
            scenes = new[] { SceneManager.GetActiveScene().path },
            locationPathName = "Builds/Server/ThereIsNoGhostHere_Server.exe",
            target = BuildTarget.StandaloneWindows64,
            subtarget = (int)StandaloneBuildSubtarget.Server,
            options = BuildOptions.None
        };
        BuildReport report = BuildPipeline.BuildPlayer(options);
        Debug.Log("Server build: " + report.summary.result);
    }
}