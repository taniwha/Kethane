using UnityEditor;
using UnityEngine;

class BuildABs {
    [MenuItem("Assets/Build Asset Bundles")]
    static void BuildAssetBundles()
    {
        // Put the bundles in a folder called "ABs" within the
        // Assets folder.
        var outDir = "../AssetBundles";
        var opts = BuildAssetBundleOptions.DeterministicAssetBundle
            | BuildAssetBundleOptions.ForceRebuildAssetBundle;
        BuildTarget[] platforms = { BuildTarget.StandaloneWindows, };
        string[] platformExts = { "", "-windows", "-macosx", "-linux" };
        for (var i = 0; i < platforms.Length; ++i) {
			Debug.Log($"AB: {platforms[i]}");
            BuildPipeline.BuildAssetBundles(outDir, opts, platforms[i]);
            var outFile = outDir + "/kethane"+ platformExts[i]+".ksp";
            FileUtil.ReplaceFile(outDir + "/kethane", outFile);
			Debug.Log($"AB: {outFile}");
        }
    }
}
