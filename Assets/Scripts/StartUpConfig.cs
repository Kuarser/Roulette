using System.Collections.Generic;
using UnityEngine;

public static class StartUpConfig
{
    public const string url = "http://8.138.140.180:8124/SoloHotfix/BaccaratPoker";//要修改//TODO:上线前改成正式服地址

    public static Dictionary<string, AssetBundle> bundleDic = new Dictionary<string, AssetBundle>();
    public static string DllPath
    {
        get
        {
            string path = Application.dataPath + "/StreamingAssets/Lib";
            if (!Application.isEditor)
                path = Application.persistentDataPath;
            return path;
        }
    }

    public static string AssetBundlePath
    {
        get
        {
            string path = Application.dataPath + "/StreamingAssets/AssetBundles";
            if (!Application.isEditor)
                path = Application.persistentDataPath;
            return path;
        }
    }

    public static string VersionPath
    {
        get
        {
            string path = Application.dataPath + "/StreamingAssets/Version.txt";
            if (!Application.isEditor)
                path = Application.persistentDataPath + "/Version.txt";
            return path;
        }
    }
}
