using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class CopyDll
{
    private static List<string> dllList = new List<string>
    {
        "UnityWebSocket.Runtime.dll",
        "Base.dll",
        "CryPrinter.dll",
        "Game.dll"
    };

    [MenuItem("Tools/Copy Dll")]
    public static void CopyAllDll()
    {
        string srcPath = System.Environment.CurrentDirectory;
        srcPath = Path.Combine(srcPath, "HybridCLRData");
        srcPath = Path.Combine(srcPath, "HotUpdateDlls");
        srcPath = Path.Combine(srcPath, "Android");

        string targetPath = Path.Combine(Application.streamingAssetsPath, "Lib");

        for (int i = 0; i < dllList.Count; i++)
        {
            string filePath = Path.Combine(srcPath, dllList[i]);
            if (File.Exists(filePath))
                File.Copy(filePath, Path.Combine(targetPath, dllList[i]), true);
        }

        Debug.Log("copy finish!!!");
    }

}
