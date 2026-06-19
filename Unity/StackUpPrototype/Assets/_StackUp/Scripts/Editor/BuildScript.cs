#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace StackUp
{
    /// <summary>
    /// Headless build entry point for CI / command line (M6 #58). Invoke with:
    ///   Unity -quit -batchmode -projectPath Unity/StackUpPrototype \
    ///         -executeMethod StackUp.BuildScript.BuildWindows -logFile - \
    ///         -stackupOutput Builds/Windows
    /// </summary>
    public static class BuildScript
    {
        [MenuItem("StackUp/Build Windows64")]
        public static void BuildWindowsMenu() => BuildWindows();

        public static void BuildWindows()
        {
            string outDir = GetArg("-stackupOutput") ?? "Builds/Windows";
            Directory.CreateDirectory(outDir);

            var options = new BuildPlayerOptions
            {
                scenes = EnabledScenes(),
                locationPathName = Path.Combine(outDir, "StackUp.exe"),
                target = BuildTarget.StandaloneWindows64,
                targetGroup = BuildTargetGroup.Standalone,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary s = report.summary;
            Debug.Log($"[Build] result={s.result} sizeBytes={s.totalSize} time={s.totalTime} errors={s.totalErrors}");

            if (Application.isBatchMode)
                EditorApplication.Exit(s.result == BuildResult.Succeeded ? 0 : 1);
        }

        private static string[] EnabledScenes()
        {
            return EditorBuildSettings.scenes.Where(sc => sc.enabled).Select(sc => sc.path).ToArray();
        }

        private static string GetArg(string name)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
                if (args[i] == name) return args[i + 1];
            return null;
        }
    }
}
#endif
