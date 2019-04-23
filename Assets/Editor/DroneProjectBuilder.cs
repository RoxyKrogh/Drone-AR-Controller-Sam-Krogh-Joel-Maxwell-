using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;

public class DroneProjectBuilder : EditorWindow {

    const string OPERATION_TITLE = "Build with DJI integration";
    [SerializeField] private string androidProjectPath = "";

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
        if (GUILayout.Button("Browse your file system")) apPath = EditorUtility.OpenFolderPanel("Embedded Unity Drone Control project root", apPath, "");
        EditorGUILayout.LabelField(" or ", GUILayout.Width(25.0f));
        if (GUILayout.Button("Download from GitHub")) Application.OpenURL("https://github.com/SamKrogh/Embedded-Unity-Drone-Control-Tango-Version-");
        EditorGUILayout.EndHorizontal();
        this.androidProjectPath = apPath;
        GUILayout.Space(20);

        EditorGUILayout.LabelField("Finally, build the Embedded Unity Drone Control project with your Unity game files: ", EditorStyles.wordWrappedLabel);
        if (GUILayout.Button("Build")) BuildProject();
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

        // TODO: Run grep build on asp
        ExecuteCommand("gradlew", "assembleDebug", androidProjectPath);
        
    }

    void ExecuteCommand(string command, string arguments = null, string workingDirectory=null) 
    {
        System.Diagnostics.ProcessStartInfo pInfo = new System.Diagnostics.ProcessStartInfo(command)
        {
            UseShellExecute = true
        };
        if (null != arguments)
            pInfo.Arguments = arguments;
        if (null != workingDirectory)
            pInfo.WorkingDirectory = workingDirectory;

        System.Diagnostics.Process p = new System.Diagnostics.Process
        {
            StartInfo = pInfo
        };
        p.Start();
        p.WaitForExit();
        p.Close();
        Debug.Log("Build complete. Generated APK is in " + this.androidProjectPath + "/build/outputs/apk/");
    }

    private static string GetShellExe()
    {
        // Determine the name of the shell process to run commands through, based on OS family
        OperatingSystemFamily os = SystemInfo.operatingSystemFamily;
        switch (os)
        {
            case OperatingSystemFamily.Windows: // windows has cmd command line
                return "cmd.exe";               
            case OperatingSystemFamily.Linux:   // linux has bash shell
            case OperatingSystemFamily.MacOSX:  // mac has bash shell
            case OperatingSystemFamily.Other:   
            default:                            // when in doubt, assume bash and maybe it will work
                return "/bin/bash";
        }
    }
}
#endif