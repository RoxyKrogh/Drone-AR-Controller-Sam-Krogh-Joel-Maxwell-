using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
#if UNITY_EDITOR
using UnityEditor;

public class DroneProjectBuilder : EditorWindow {

    const string OPERATION_TITLE = "Build with DJI integration";
    [SerializeField] private string androidProjectPath = "";
    [SerializeField] private string androidSdkPath = "";
    [SerializeField] private string buildJdkPath = "";

    [MenuItem("Drone/" + OPERATION_TITLE)]
    static void Init()
    {
        DroneProjectBuilder window = ScriptableObject.CreateInstance<DroneProjectBuilder>();
        window.position = new Rect(Screen.width / 2, Screen.height / 2, 250, 150);
        window.wantsMouseMove = true;
        window.titleContent.text = OPERATION_TITLE;
        window.Show();
    }

    private void OnEnable()
    {
        // retreive json from prefs, or write default values
        var data = EditorPrefs.GetString(OPERATION_TITLE, JsonUtility.ToJson(this, false));
        // apply fields to this window
        JsonUtility.FromJsonOverwrite(data, this);
    }

    private void OnDisable()
    {
        // get json from prefs
        var data = JsonUtility.ToJson(this, false);
        // save field values to json
        EditorPrefs.SetString(OPERATION_TITLE, data);
    }

    void OnGUI()
    {
        GUILayout.Space(5);
        EditorGUILayout.LabelField("This dialog is for exporting and building your project with DJI drone control integration.", EditorStyles.wordWrappedLabel);
        GUILayout.Space(20);

        EditorGUILayout.LabelField("First, export your Unity project: ", EditorStyles.wordWrappedLabel);
        if (GUILayout.Button("Export")) ExportProject();
        GUILayout.Space(20);

        EditorGUILayout.LabelField("Next, find the Embedded Unity Drone Control project: ", EditorStyles.wordWrappedLabel);
        string apPath = GUILayout.TextField(this.androidProjectPath);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Browse file system")) apPath = EditorUtility.OpenFolderPanel("Embedded Unity Drone Control project root", apPath, "");
        if (GUILayout.Button("Download from GitHub")) Application.OpenURL("https://github.com/SamKrogh/Embedded-Unity-Drone-Control-Tango-Version-");
        EditorGUILayout.EndHorizontal();
        this.androidProjectPath = apPath;
        GUILayout.Space(20);

        EditorGUILayout.LabelField("Next, find the Android SDK Tools: ", EditorStyles.wordWrappedLabel);
        string localPropFile = this.androidProjectPath + "/local.properties";
        string savedSdkString = GetSavedAndroidSdkPath(localPropFile);
        string sdkPath = GUILayout.TextField(this.androidSdkPath);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Browse file system")) sdkPath = EditorUtility.OpenFolderPanel("Android SDK tools", sdkPath, "");
        if (GUILayout.Button("Download")) Application.OpenURL("https://developer.android.com/studio#command-tools");
        EditorGUILayout.EndHorizontal();
        this.androidSdkPath = sdkPath;
        try
        {
            if (Directory.Exists(this.androidSdkPath) && Directory.Exists(this.androidProjectPath))
            {
                if (!File.Exists(localPropFile))
                {
                    File.Create(localPropFile).Close();
                    File.WriteAllText(localPropFile, "sdk.dir=" + androidSdkPath + "\n");
                }
                else
                {
                    if (savedSdkString == "")
                        savedSdkString = GetSavedAndroidSdkPath(localPropFile);
                    Debug.Log(savedSdkString);
                    string text = File.ReadAllText(localPropFile);
                    string key = "=" + savedSdkString + "\n";
                    if (savedSdkString != "" && text.Contains(key))
                        text = text.Replace(key, "=" + androidSdkPath + "\n");
                    else
                        text = text + "sdk.dir=" + androidSdkPath + "\n";
                    File.WriteAllText(localPropFile, text);
                }
            }
        } catch (IOException e)
        {
            Debug.Log(e);
        }
        GUILayout.Space(20);

        string jdkPath = GUILayout.TextField(this.buildJdkPath);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Browse file system")) jdkPath = EditorUtility.OpenFolderPanel("JDK Home", jdkPath, "");
        //if (GUILayout.Button("Download")) Application.OpenURL("https://developer.android.com/studio#command-tools");
        EditorGUILayout.EndHorizontal();
        this.buildJdkPath = jdkPath;
        GUILayout.Space(20);

        EditorGUILayout.LabelField("Finally, build the Embedded Unity Drone Control project with your Unity game files: ", EditorStyles.wordWrappedLabel);
        if (GUILayout.Button("Build")) BuildProject();
        
        if (GUILayout.Button("Install")) InstallProject();
    }

    string GetSavedAndroidSdkPath(string localPropFile)
    {
        string savedSdkString = androidSdkPath;
        try
        {
            if (!Directory.Exists(this.androidSdkPath) && Directory.Exists(this.androidProjectPath) && File.Exists(localPropFile))
            {
                var match = Regex.Match(File.ReadAllText(localPropFile), "sdk.dir=([^\n]*)\n", RegexOptions.Multiline);
                if (match.Success)
                {
                    savedSdkString = match.Groups[1].Captures[0].Value;
                    androidSdkPath = savedSdkString.Replace(@"\:", @":").Replace(@"\\", @"\");
                }
            }
        }
        catch (IOException e)
        {
            Debug.Log(e);
        }
        return savedSdkString;
    }

    void ExportProject()
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] { "Assets/main.unity", "Assets/start.unity", "Assets/setupScene.unity" };
        buildPlayerOptions.locationPathName = "exported";
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.AcceptExternalModificationsToPlayer;

        string results = BuildPipeline.BuildPlayer(buildPlayerOptions);
        Debug.Log("Export complete." + (results == "" ? "" : " Results: ") + results);
        this.Focus();
    }

    void BuildProject()
    {
        string dataPath = "/src/main/assets/bin/Data";
        string unityPath = Application.dataPath; // get absolute path of Unity projects Assets directory
        unityPath = unityPath.Substring(0, unityPath.Length - "Assets".Length); // change path to Unity project root

        string srcPath = "exported/" + PlayerSettings.productName + dataPath; // path to Data folder in exported app (Unity) files
        string dstPath = this.androidProjectPath + dataPath; // path to Data folder in the embedded drone controller (Android Studio) project
        Debug.Log(FileUtil.GetProjectRelativePath(srcPath));
        Debug.Log("Building Unity project from " + srcPath + " to embedded project at " + dstPath);

        if (!Directory.Exists(srcPath))
            this.ShowNotification(new GUIContent("Build failed - please Export first"));
        if (!Directory.Exists(dstPath))
        {
            Debug.Log("Destination directory does not exist - creating directory");
            Directory.CreateDirectory(dstPath);
        }
        FileUtil.ReplaceDirectory(srcPath, dstPath); // copy srcPath to dstPath
        
        string error = ExecuteGradle("assembleDebug -Dorg.gradle.java.home=" + buildJdkPath);
        bool success = !error.Contains("FAILURE:");
        Debug.Log("Build done. " + error);
    }

    void InstallProject()
    {
        Debug.Log("Install Project " +  ExecuteGradle("installDebug -Dorg.gradle.java.home=" + buildJdkPath));
    }

    string ExecuteGradle(string arguments)
    {
        return ExecuteCommand(androidProjectPath + "/" + GetGradlewExe(), arguments, androidProjectPath);
    }

    string ExecuteCommand(string command, string arguments = null, string workingDirectory = null)
    {
        var outputText = new StringBuilder();
        var errorText = new StringBuilder();

        using (var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(
            command,
            arguments)
        {
            RedirectStandardError = true,
            UseShellExecute = false,
            WorkingDirectory = workingDirectory
        }))
        {

            process.ErrorDataReceived += (sendingProcess, errorLine) =>
            {
                errorText.AppendLine(errorLine.Data); // capture the error
            };
            
            process.BeginErrorReadLine();
            process.WaitForExit();
        }
        return errorText.ToString();
    }

    private static string GetGradlewExe()
    {
        // Determine the name of the shell process to run commands through, based on OS family
        OperatingSystemFamily os = SystemInfo.operatingSystemFamily;
        switch (os)
        {
            case OperatingSystemFamily.Windows: // windows has cmd command line
                return "gradlew.bat";               
            case OperatingSystemFamily.Linux:   // linux has bash shell
            case OperatingSystemFamily.MacOSX:  // mac has bash shell
            case OperatingSystemFamily.Other:   
            default:                            // when in doubt, assume bash and maybe it will work
                return "gradlew";
        }
    }
}
#endif