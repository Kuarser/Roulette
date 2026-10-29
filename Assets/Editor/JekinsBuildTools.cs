using HybridCLR.Editor.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Process = System.Diagnostics.Process;
using ProcessStartInfo = System.Diagnostics.ProcessStartInfo;

public class JekinsBuildTools
{
    private const string ApkBaseNameEditorPrefsKey = "SBoxBase.BuildApp.ApkBaseName";

    private const string DefaultApkBaseName = "SBoxBase";

    private static bool buildAssetBundle;

    private static bool compileDll;

    private static readonly string tempPah = Application.dataPath + $"/../Export/Temp";

    private static readonly string buildPath = Application.dataPath + $"/../Export/Build";

    private static readonly string signPath = Application.dataPath + $"/../Export/Signapk";

    [MenuItem("Tools/Build App")]
    public static void BuildApp()
    {
        try
        {
            ValidateBuildEnvironment();
            string version = IncrementVersion();

            BuildAssetBundel();
            CompileDll();
            ExportProject();
            BuildAndroidApp();
            string apkPath = SignAndroidApp();

            Debug.Log($"<color=#42DF0A>Build App Finish! Version:{version} Path:{apkPath}</color>");
            if (EditorUtility.DisplayDialog("Build App", $"Build App完成\n版本：{version}\n路径：{apkPath}", "打开文件夹", "确定"))
                EditorUtility.RevealInFinder(apkPath);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            BuildAppErrorWindow.Open(e.Message);
        }
    }

    [MenuItem("Tools/Build App Settings")]
    private static void OpenBuildAppSettings()
    {
        BuildAppSettingsWindow.Open();
    }

    public static void JekinsBuild()
    {
        GetArges();
        if (buildAssetBundle)
            BuildAssetBundel();
        if (compileDll)
            CompileDll();
        ExportProject();
    }

    #region 读取参数
    private static void GetArges()
    {
        string[] args = Environment.GetCommandLineArgs();
        foreach (var s in args)
        {
            if (s.Contains("--productName:"))
            {
                string productName = s.Split('#')[1];
                // 设置app名字
                PlayerSettings.productName = productName;
                Debug.Log($"app名称:{productName}");
            }

            if (s.Contains("--version:"))
            {
                string version = s.Split('#')[1];
                // 设置版本号
                PlayerSettings.bundleVersion = version;
                PlayerSettings.Android.bundleVersionCode = GetAndroidVersionCode(version);

                VersionData versionData = new VersionData
                {
                    Version = version
                };

                File.WriteAllText(StartUpConfig.VersionPath, JsonConvert.SerializeObject(versionData));
                Debug.Log($"新版本号已写入:{versionData.Version}");
            }

            if (s.Contains("--buildAssetBundle:"))
            {
                string buildAssetBundleStr = s.Split('#')[1];
                buildAssetBundle = buildAssetBundleStr.Equals("true", StringComparison.OrdinalIgnoreCase);
            }

            if (s.Contains("--compileDll:"))
            {
                string compileDllStr = s.Split('#')[1];
                compileDll = compileDllStr.Equals("true", StringComparison.OrdinalIgnoreCase);
            }

        }
        AssetDatabase.SaveAssets();
    }
    #endregion

    #region 导出工程
    public static void ExportProject()
    {
        PrepareTempDirectory();
        EditorUserBuildSettings.exportAsGoogleAndroidProject = true;

        BuildPlayerOptions opt = new BuildPlayerOptions
        {
            scenes = GetScenePaths(),
            locationPathName = tempPah,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(opt);
        if (report.summary.result != BuildResult.Succeeded)
            throw new Exception($"Unity导出失败：{report.summary.result}");

        Debug.Log($"Unity Android工程导出完成:{opt.locationPathName}");
        string sourcePath = Path.Combine(tempPah, "unityLibrary", "src", "main");
        string sourceAssetsPath = Path.Combine(sourcePath, "assets");
        string sourceIl2CppPath = Path.Combine(sourcePath, "Il2CppOutputProject");
        string sourceResPath = Path.Combine(tempPah, "launcher", "src", "main", "res");

        string targetPath = Path.Combine(buildPath, "unityLibrary", "src", "main");
        string targetAssetsPath = Path.Combine(targetPath, "assets");
        string targetIl2CppPath = Path.Combine(targetPath, "Il2CppOutputProject");
        string targetResPath = Path.Combine(buildPath, "launcher", "src", "main", "res");

        DeleteDirectoryIfExists(targetAssetsPath);
        DeleteDirectoryIfExists(targetIl2CppPath);
        DeleteDirectoryIfExists(targetResPath);

        CopyDirectory(sourceAssetsPath, targetAssetsPath, true);
        CopyDirectory(sourceIl2CppPath, targetIl2CppPath, true);
        CopyDirectory(sourceResPath, targetResPath, true);
        SyncImportedAndroidLibraries();
        SetAndroidProjectVersion(buildPath);
    }

    private static void PrepareTempDirectory()
    {
        string exportPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../Export"));
        string expectedTempPath = Path.GetFullPath(Path.Combine(exportPath, "Temp"));
        string actualTempPath = Path.GetFullPath(tempPah);
        if (!actualTempPath.Equals(expectedTempPath, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Temp目录不正确:{actualTempPath}");

        if (!Directory.Exists(actualTempPath))
        {
            Directory.CreateDirectory(actualTempPath);
            Debug.Log($"已创建Temp目录:{actualTempPath}");
            return;
        }

        if ((new DirectoryInfo(actualTempPath).Attributes & FileAttributes.ReparsePoint) != 0)
            throw new InvalidOperationException($"Temp目录不能是链接目录:{actualTempPath}");

        ClearDirectory(actualTempPath);
        Debug.Log($"已清空Temp目录:{actualTempPath}");
    }

    private static void SetAndroidProjectVersion(string locationPathName)
    {
        int versionCode = GetAndroidVersionCode(PlayerSettings.bundleVersion);
        string[] gradleFilePaths =
        {
            Path.Combine(locationPathName, "launcher", "build.gradle"),
            Path.Combine(locationPathName, "unityLibrary", "build.gradle")
        };

        foreach (string gradleFilePath in gradleFilePaths)
        {
            if (!File.Exists(gradleFilePath))
                throw new FileNotFoundException($"未找到Gradle文件:{gradleFilePath}", gradleFilePath);

            string[] lines = File.ReadAllLines(gradleFilePath);
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("versionCode"))
                {
                    lines[i] = Regex.Replace(lines[i], @"versionCode\s+\d+", $"versionCode {versionCode}");
                }
                else if (lines[i].Contains("versionName"))
                {
                    lines[i] = Regex.Replace(lines[i], "versionName\\s+['\"][^'\"]*['\"]", $"versionName '{PlayerSettings.bundleVersion}'");
                }
            }
            File.WriteAllLines(gradleFilePath, lines);
            Debug.Log($"更新Android工程版本:{gradleFilePath} VersionCode:{versionCode} VersionName:{PlayerSettings.bundleVersion}");
        }
    }

    private static string[] GetScenePaths()
    {
        List<string> scenePaths = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
                scenePaths.Add(scene.path);
        }

        if (scenePaths.Count == 0)
            throw new InvalidOperationException("Build Settings中没有启用的场景");

        return scenePaths.ToArray();
    }

    private static void SyncImportedAndroidLibraries()
    {
        string sourceUnityLibraryPath = Path.Combine(tempPah, "unityLibrary");
        string targetUnityLibraryPath = Path.Combine(buildPath, "unityLibrary");

        ValidateLocalAarDependencies(
            Path.Combine(sourceUnityLibraryPath, "build.gradle"),
            Path.Combine(targetUnityLibraryPath, "build.gradle"));

        SyncLibraryDirectory(
            Path.Combine(sourceUnityLibraryPath, "libs"),
            Path.Combine(targetUnityLibraryPath, "libs"),
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".aar", ".jar" });

        SyncLibraryDirectory(
            Path.Combine(sourceUnityLibraryPath, "src", "main", "jniLibs"),
            Path.Combine(targetUnityLibraryPath, "src", "main", "jniLibs"),
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".so" });
    }

    private static void ValidateLocalAarDependencies(string sourceBuildGradlePath, string targetBuildGradlePath)
    {
        HashSet<string> sourceDependencies = GetLocalAarDependencies(sourceBuildGradlePath);
        HashSet<string> targetDependencies = GetLocalAarDependencies(targetBuildGradlePath);
        if (sourceDependencies.SetEquals(targetDependencies))
            return;

        List<string> addedDependencies = new List<string>();
        foreach (string dependency in sourceDependencies)
        {
            if (!targetDependencies.Contains(dependency))
                addedDependencies.Add(dependency);
        }

        List<string> removedDependencies = new List<string>();
        foreach (string dependency in targetDependencies)
        {
            if (!sourceDependencies.Contains(dependency))
                removedDependencies.Add(dependency);
        }

        addedDependencies.Sort(StringComparer.OrdinalIgnoreCase);
        removedDependencies.Sort(StringComparer.OrdinalIgnoreCase);
        string addedMessage = addedDependencies.Count > 0 ? string.Join(", ", addedDependencies) : "无";
        string removedMessage = removedDependencies.Count > 0 ? string.Join(", ", removedDependencies) : "无";
        throw new InvalidOperationException(
            $"Android本地AAR依赖声明发生变化，已停止同步，避免覆盖Export/Build中的Gradle配置。新增:{addedMessage}；移除:{removedMessage}。请确认后手动更新Export/Build/unityLibrary/build.gradle");
    }

    private static HashSet<string> GetLocalAarDependencies(string buildGradlePath)
    {
        if (!File.Exists(buildGradlePath))
            throw new FileNotFoundException($"Android Gradle文件不存在:{buildGradlePath}", buildGradlePath);

        HashSet<string> dependencies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        string content = File.ReadAllText(buildGradlePath);
        MatchCollection matches = Regex.Matches(
            content,
            "(?:implementation|api|compile)\\s*\\(\\s*name\\s*:\\s*['\\\"](?<name>[^'\\\"]+)['\\\"]\\s*,\\s*ext\\s*:\\s*['\\\"]aar['\\\"]",
            RegexOptions.IgnoreCase);
        foreach (Match match in matches)
            dependencies.Add(match.Groups["name"].Value);

        return dependencies;
    }

    private static void SyncLibraryDirectory(string sourceDirectory, string targetDirectory, HashSet<string> extensions)
    {
        if (!Directory.Exists(sourceDirectory))
            throw new DirectoryNotFoundException($"Android导入库目录不存在:{sourceDirectory}");

        Directory.CreateDirectory(targetDirectory);
        string sourceRoot = sourceDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        string targetRoot = targetDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        HashSet<string> sourceRelativePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (string sourceFilePath in Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            if (!extensions.Contains(Path.GetExtension(sourceFilePath)))
                continue;

            string relativePath = sourceFilePath.Substring(sourceRoot.Length);
            sourceRelativePaths.Add(relativePath);
            CopyFileIfChanged(sourceFilePath, Path.Combine(targetDirectory, relativePath));
        }

        foreach (string targetFilePath in Directory.GetFiles(targetDirectory, "*", SearchOption.AllDirectories))
        {
            if (!extensions.Contains(Path.GetExtension(targetFilePath)))
                continue;

            string relativePath = targetFilePath.Substring(targetRoot.Length);
            if (sourceRelativePaths.Contains(relativePath))
                continue;

            File.SetAttributes(targetFilePath, FileAttributes.Normal);
            File.Delete(targetFilePath);
            Debug.Log($"移除旧Android导入库:{targetFilePath}");
        }
    }

    private static void CopyFileIfChanged(string sourceFilePath, string targetFilePath)
    {
        if (!File.Exists(sourceFilePath))
            throw new FileNotFoundException($"Android导入文件不存在:{sourceFilePath}", sourceFilePath);

        if (File.Exists(targetFilePath) && FilesAreEqual(sourceFilePath, targetFilePath))
            return;

        string targetDirectory = Path.GetDirectoryName(targetFilePath);
        if (!Directory.Exists(targetDirectory))
            Directory.CreateDirectory(targetDirectory);

        if (File.Exists(targetFilePath))
            File.SetAttributes(targetFilePath, FileAttributes.Normal);
        File.Copy(sourceFilePath, targetFilePath, true);
        Debug.Log($"更新Android导入文件:{targetFilePath}");
    }

    private static bool FilesAreEqual(string firstFilePath, string secondFilePath)
    {
        FileInfo firstFile = new FileInfo(firstFilePath);
        FileInfo secondFile = new FileInfo(secondFilePath);
        if (firstFile.Length != secondFile.Length)
            return false;

        using (SHA256 sha256 = SHA256.Create())
        using (FileStream firstStream = File.OpenRead(firstFilePath))
        using (FileStream secondStream = File.OpenRead(secondFilePath))
        {
            byte[] firstHash = sha256.ComputeHash(firstStream);
            byte[] secondHash = sha256.ComputeHash(secondStream);
            if (firstHash.Length != secondHash.Length)
                return false;

            for (int i = 0; i < firstHash.Length; i++)
            {
                if (firstHash[i] != secondHash[i])
                    return false;
            }
        }

        return true;
    }
    #endregion

    #region 生成Assetbundle
    private static void BuildAssetBundel()
    {
        BuildAssetBundle.BulidAllAssetBundles();
    }

    private static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
    {
        // Get information about the source directory
        var dir = new DirectoryInfo(sourceDir);

        // Check if the source directory exists
        if (!dir.Exists)
            throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

        // Cache directories before we start copying
        DirectoryInfo[] dirs = dir.GetDirectories();

        // Create the destination directory
        Directory.CreateDirectory(destinationDir);

        // Get the files in the source directory and copy to the destination directory
        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath, true);
        }

        // If recursive and copying subdirectories, recursively call this method
        if (recursive)
        {
            foreach (DirectoryInfo subDir in dirs)
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir, true);
            }
        }
    }
    #endregion

    #region 生成热更Dll
    /// <summary>
    /// 生成热更dll
    /// </summary>
    private static void CompileDll()
    {
        CompileDllCommand.CompileDllActiveBuildTarget();
        CopyDll.CopyAllDll();
    }
    #endregion

    #region 生成并签名APK
    private static void BuildAndroidApp()
    {
        string launcherBuildPath = Path.Combine(buildPath, "launcher", "build");
        DeleteDirectoryIfExists(launcherBuildPath);

        string gradlePath = Path.Combine(buildPath, "gradlew.bat");
        RunBat(gradlePath, ":launcher:assembleRelease", buildPath, "Gradle Release构建");
    }

    private static string SignAndroidApp()
    {
        string sourceApkPath = Path.Combine(buildPath, "launcher", "build", "outputs", "apk", "release", "launcher-release.apk");
        if (!File.Exists(sourceApkPath))
            throw new FileNotFoundException($"未找到Gradle生成的APK:{sourceApkPath}", sourceApkPath);

        string signSourceApkPath = Path.Combine(signPath, "launcher-release.apk");
        string signedApkName = $"{GetConfiguredApkBaseName()}_ver{PlayerSettings.bundleVersion}.apk";
        string signedApkPath = Path.Combine(signPath, signedApkName);
        File.Copy(sourceApkPath, signSourceApkPath, true);
        if (File.Exists(signedApkPath))
            File.Delete(signedApkPath);

        string signBatPath = Path.Combine(signPath, "signapk.bat");
        RunBat(signBatPath, $"\"{signSourceApkPath}\" \"{signedApkPath}\"", signPath, "平台签名");

        FileInfo apkFile = new FileInfo(signedApkPath);
        if (!apkFile.Exists || apkFile.Length == 0)
            throw new InvalidOperationException($"签名APK生成失败:{signedApkPath}");

        return signedApkPath;
    }

    private static void RunBat(string batPath, string arguments, string workingDirectory, string stepName)
    {
        StringBuilder output = new StringBuilder();
        StringBuilder error = new StringBuilder();
        ProcessStartInfo processStartInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/d /s /c \"\"{batPath}\" {arguments}\"",
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using (Process process = new Process { StartInfo = processStartInfo })
        {
            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data != null)
                    output.AppendLine(e.Data);
            };
            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data != null)
                    error.AppendLine(e.Data);
            };

            Debug.Log($"开始{stepName}:{batPath} {arguments}");
            if (!process.Start())
                throw new InvalidOperationException($"无法启动{stepName}");

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                string outputText = output.ToString();
                string errorText = error.ToString();
                if (!string.IsNullOrWhiteSpace(outputText))
                    Debug.LogError($"{stepName}输出：\n{outputText}");
                if (!string.IsNullOrWhiteSpace(errorText))
                    Debug.LogError($"{stepName}错误：\n{errorText}");

                string errorSummary = GetProcessErrorSummary(outputText, errorText);
                throw new InvalidOperationException($"{stepName}失败，ExitCode:{process.ExitCode}\n{errorSummary}");
            }

            Debug.Log($"{stepName}完成\n{output}");
        }
    }

    private static string GetProcessErrorSummary(string output, string error)
    {
        string allText = $"{error}\n{output}";
        string[] lines = allText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        string[] keywords = { "missing", "failure", "what went wrong", "execution failed", "error", "exception" };
        StringBuilder summary = new StringBuilder();

        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();
            foreach (string keyword in keywords)
            {
                if (trimmedLine.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                if (summary.Length > 0)
                    summary.AppendLine();
                summary.Append(trimmedLine);
                break;
            }

            if (summary.Length >= 1200)
                break;
        }

        if (summary.Length == 0)
        {
            int startIndex = Math.Max(0, lines.Length - 8);
            for (int i = startIndex; i < lines.Length; i++)
                summary.AppendLine(lines[i].Trim());
        }

        if (summary.Length == 0)
            return "未获得进程错误输出，请查看Unity Console。";

        if (summary.Length > 1200)
            return summary.ToString(0, 1200) + "\n……\n详细信息请查看Unity Console。";

        return summary + "\n详细信息请查看Unity Console。";
    }
    #endregion

    #region 构建检查及目录处理
    private static void ValidateBuildEnvironment()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("请退出Play Mode后再执行Build App");
        if (EditorApplication.isCompiling)
            throw new InvalidOperationException("Unity正在编译脚本，请稍后再执行Build App");
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            throw new InvalidOperationException("当前Build Target不是Android");

        GetConfiguredApkBaseName();

        string[] requiredPaths =
        {
            buildPath,
            Path.Combine(buildPath, "gradlew.bat"),
            Path.Combine(buildPath, "launcher", "build.gradle"),
            Path.Combine(buildPath, "unityLibrary", "build.gradle"),
            signPath,
            Path.Combine(signPath, "signapk.bat"),
            Path.Combine(signPath, "signapk.jar"),
            Path.Combine(signPath, "platform.x509.pem"),
            Path.Combine(signPath, "platform.pk8")
        };

        foreach (string path in requiredPaths)
        {
            if (!Directory.Exists(path) && !File.Exists(path))
                throw new FileNotFoundException($"构建所需路径不存在:{path}", path);
        }
    }

    private static string IncrementVersion()
    {
        if (!File.Exists(StartUpConfig.VersionPath))
            throw new FileNotFoundException($"未找到版本文件:{StartUpConfig.VersionPath}", StartUpConfig.VersionPath);

        VersionData versionData = JsonConvert.DeserializeObject<VersionData>(File.ReadAllText(StartUpConfig.VersionPath));
        if (versionData == null)
            throw new InvalidOperationException($"版本文件内容无效:{StartUpConfig.VersionPath}");

        int[] versionParts = ParseVersion(versionData.Version);
        versionParts[2]++;
        if (versionParts[2] >= 10)
        {
            versionParts[2] = 0;
            versionParts[1]++;
            if (versionParts[1] >= 10)
            {
                versionParts[1] = 0;
                versionParts[0]++;
            }
        }

        versionData.Version = $"{versionParts[0]}.{versionParts[1]}.{versionParts[2]}";
        PlayerSettings.bundleVersion = versionData.Version;
        PlayerSettings.Android.bundleVersionCode = GetAndroidVersionCode(versionData.Version);
        File.WriteAllText(StartUpConfig.VersionPath, JsonConvert.SerializeObject(versionData));
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"新版本号已写入:{versionData.Version}");
        return versionData.Version;
    }

    private static int GetAndroidVersionCode(string version)
    {
        int[] versionParts = ParseVersion(version);
        return checked(versionParts[0] * 100 + versionParts[1] * 10 + versionParts[2]);
    }

    private static int[] ParseVersion(string version)
    {
        if (string.IsNullOrEmpty(version))
            throw new ArgumentException("版本号不能为空", nameof(version));

        string[] parts = version.Split('.');
        if (parts.Length != 3)
            throw new FormatException("版本号格式应为 主版本.次版本.修订版本");

        int[] versionParts = new int[3];
        for (int i = 0; i < parts.Length; i++)
        {
            if (!int.TryParse(parts[i], out versionParts[i]) || versionParts[i] < 0)
                throw new FormatException("版本号各部分必须是非负整数");
        }
        return versionParts;
    }

    private static string GetConfiguredApkBaseName()
    {
        string apkBaseName = EditorPrefs.GetString(ApkBaseNameEditorPrefsKey, DefaultApkBaseName);
        if (!TryNormalizeApkBaseName(apkBaseName, out string normalizedName, out string errorMessage))
            throw new InvalidOperationException($"APK基础名称配置无效：{errorMessage}");
        return normalizedName;
    }

    private static bool TryNormalizeApkBaseName(string apkBaseName, out string normalizedName, out string errorMessage)
    {
        normalizedName = apkBaseName == null ? string.Empty : apkBaseName.Trim();
        if (normalizedName.EndsWith(".apk", StringComparison.OrdinalIgnoreCase))
            normalizedName = normalizedName.Substring(0, normalizedName.Length - 4).Trim();

        if (string.IsNullOrEmpty(normalizedName))
        {
            errorMessage = "名称不能为空";
            return false;
        }

        if (normalizedName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            errorMessage = "名称包含非法文件名字符";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }

    private static void ClearDirectory(string path)
    {
        DirectoryInfo directory = new DirectoryInfo(path);
        foreach (FileInfo file in directory.GetFiles())
        {
            file.Attributes = FileAttributes.Normal;
            file.Delete();
        }

        foreach (DirectoryInfo childDirectory in directory.GetDirectories())
        {
            if ((childDirectory.Attributes & FileAttributes.ReparsePoint) != 0)
            {
                childDirectory.Delete();
                continue;
            }

            ClearDirectory(childDirectory.FullName);
            childDirectory.Attributes = FileAttributes.Normal;
            childDirectory.Delete();
        }
    }

    private static void DeleteDirectoryIfExists(string path)
    {
        if (!Directory.Exists(path))
            return;

        ClearDirectory(path);
        DirectoryInfo directory = new DirectoryInfo(path)
        {
            Attributes = FileAttributes.Normal
        };
        directory.Delete();
    }
    #endregion

    private class BuildAppSettingsWindow : EditorWindow
    {
        private string apkBaseName;

        public static void Open()
        {
            BuildAppSettingsWindow window = GetWindow<BuildAppSettingsWindow>(true, "Build App Settings");
            window.minSize = new Vector2(420, 130);
            window.maxSize = window.minSize;
            window.Show();
        }

        private void OnEnable()
        {
            apkBaseName = EditorPrefs.GetString(ApkBaseNameEditorPrefsKey, DefaultApkBaseName);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("最终APK文件名配置", EditorStyles.boldLabel);
            apkBaseName = EditorGUILayout.TextField("APK基础名称", apkBaseName);

            string previewBaseName = string.IsNullOrWhiteSpace(apkBaseName) ? DefaultApkBaseName : apkBaseName.Trim();
            if (previewBaseName.EndsWith(".apk", StringComparison.OrdinalIgnoreCase))
                previewBaseName = previewBaseName.Substring(0, previewBaseName.Length - 4).Trim();
            EditorGUILayout.LabelField("最终格式", $"{previewBaseName}_ver{{当前版本号}}.apk");

            EditorGUILayout.Space();
            if (!GUILayout.Button("保存"))
                return;

            if (!TryNormalizeApkBaseName(apkBaseName, out string normalizedName, out string errorMessage))
            {
                EditorUtility.DisplayDialog("Build App Settings", errorMessage, "确定");
                return;
            }

            EditorPrefs.SetString(ApkBaseNameEditorPrefsKey, normalizedName);
            Close();
        }
    }

    private class BuildAppErrorWindow : EditorWindow
    {
        private string errorMessage;

        private Vector2 scrollPosition;

        public static void Open(string message)
        {
            BuildAppErrorWindow window = CreateInstance<BuildAppErrorWindow>();
            window.titleContent = new GUIContent("Build App失败");
            window.errorMessage = message;
            window.minSize = new Vector2(680, 300);
            window.ShowUtility();
            window.Focus();
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox("Build App失败，详细日志已输出到Unity Console。", MessageType.Error);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.SelectableLabel(errorMessage, EditorStyles.textArea, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("确定"))
                Close();
        }
    }
}
