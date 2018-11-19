using UnityEngine;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class CopyFilesOnBuild : IPostprocessBuildWithReport
{
    int IOrderedCallback.callbackOrder { get { return 0; } }

    // Where the files are located relative to the inputPath
    string[] inputFiles =
    {
        "/steam_api.dll",
        "/steam_api64.dll",
    };

    // Where the files should be copied to relative to the outputPath
    string[] outputFiles =
    {
        "/steam_api.dll",
        "/steam_api64.dll",
    };

    public void OnPostprocessBuild(BuildReport report)
    {
        // The input path is the directory in which the Assets and Library folders are located
        string inputPath = Application.dataPath + "/..";

        // The output path is the root directory in which the build .exe is located
        int lastSlash = report.summary.outputPath.LastIndexOf('/');
        string outputPath = report.summary.outputPath.Substring(0, lastSlash);

        // Both arrays have to be the same size
        for (int i = 0; i < inputFiles.Length; i++)
        {
            System.IO.File.Copy(inputPath + inputFiles[i], outputPath + outputFiles[i]);
        }
    }
}
