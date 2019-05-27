using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
#if UNITY_EDITOR
using UnityEditor;

namespace droneapi
{
    public class BuildWindow : EditorWindow
    {
        const string OPERATION_TITLE = "Build with DJI integration";
        //[SerializeField] private string androidProjectPath = null;
        [SerializeField] private string androidSdkPath = null;
        [SerializeField] private string buildJdkPath = null;
        [SerializeField] private string djiSdkKey = null;
        [SerializeField] private string[] exportScenes = { "Assets/start.unity", "Assets/setupScene.unity", "Assets/main.unity" };

        private const BuildOptions kDefaultExportOptions = BuildOptions.AllowDebugging | BuildOptions.Development;

        BuildUtil.BuildVariables BuildVars { get { return new BuildUtil.BuildVariables(androidSdkPath, buildJdkPath, djiSdkKey); } }

        [MenuItem("Drone/" + OPERATION_TITLE)]
        static void Init()
        {
            BuildWindow window = ScriptableObject.CreateInstance<BuildWindow>();
            window.position = new Rect(Screen.width / 2, Screen.height / 2, 250, 150);
            window.wantsMouseMove = true;
            window.titleContent.text = OPERATION_TITLE;
            window.Show();
        }

        private void OnEnable()
        {
            var data = EditorPrefs.GetString(OPERATION_TITLE, JsonUtility.ToJson(this, false)); // retreive json from prefs, or write default values
            JsonUtility.FromJsonOverwrite(data, this); // apply fields to this window
        }

        private void OnDisable()
        {
            var data = JsonUtility.ToJson(this, false); // get json from prefs
            EditorPrefs.SetString(OPERATION_TITLE, data); // save field values to json
        }

        void OnGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("This dialog is for exporting and building your project with DJI drone control integration.", EditorStyles.wordWrappedLabel);
            GUILayout.Space(20);

            #region Embedded Project Path Field (no longer required - this is now included in the Unity package)
            /*EditorGUILayout.LabelField("Next, find the Embedded Unity Drone Control project: ", EditorStyles.wordWrappedLabel);
            string apPath = GUILayout.TextField(this.androidProjectPath);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Browse file system")) apPath = EditorUtility.OpenFolderPanel("Embedded Unity Drone Control project root", apPath, "");
            if (GUILayout.Button("Download from GitHub")) Application.OpenURL("https://github.com/SamKrogh/Embedded-Unity-Drone-Control-Tango-Version-");
            EditorGUILayout.EndHorizontal();
            this.androidProjectPath = apPath;
            GUILayout.Space(20);*/
            #endregion

            #region Android SDK Path Field
            EditorGUILayout.LabelField("Android SDK Tools: ", EditorStyles.wordWrappedLabel);
            string localPropFile = Cleanup.kNativeAppDir + "/local.properties";
            string savedSdkString = androidSdkPath ?? BuildUtil.GetSavedAndroidSdkPath(localPropFile, BuildVars);
            string sdkPath = GUILayout.TextField(this.androidSdkPath);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Browse file system")) sdkPath = EditorUtility.OpenFolderPanel("Android SDK tools", sdkPath, "");
            if (GUILayout.Button("Download")) Application.OpenURL("https://developer.android.com/studio#command-tools");
            EditorGUILayout.EndHorizontal();
            if (this.androidSdkPath != sdkPath)
            {
                this.androidSdkPath = sdkPath;
                BuildUtil.WriteAndroidSdkPath(Cleanup.kNativeAppDir, BuildVars);
            }
            #endregion
            GUILayout.Space(20);

            #region JDK Path Field
            EditorGUILayout.LabelField("JDK Path: ", EditorStyles.wordWrappedLabel);
            string jdkPath = GUILayout.TextField(this.buildJdkPath);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Browse file system")) jdkPath = EditorUtility.OpenFolderPanel("JDK Home", jdkPath, "");
            if (GUILayout.Button("Download")) Application.OpenURL("https://www.oracle.com/technetwork/java/javase/downloads/index.html");
            EditorGUILayout.EndHorizontal();
            this.buildJdkPath = jdkPath;
            #endregion
            GUILayout.Space(20);

            // 1) Export Game
            if (GUILayout.Button("Export Game Module")) ExportGame(Cleanup.kExportDir, exportScenes);
            // 2) Build Game
            if (GUILayout.Button("Build Game Module")) BuildGame();
            // 3) Build Application
            if (GUILayout.Button("Build App")) BuildApp();
            // 4) Install Application
            if (GUILayout.Button("Install App")) InstallApp();
        }

        public static void ExportGame(string outputPath, string[] scenes, BuildOptions options = kDefaultExportOptions)
        {
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = BuildOptions.AcceptExternalModificationsToPlayer | options
            };

            var results = BuildPipeline.BuildPlayer(buildPlayerOptions);
            Debug.Log(System.DateTime.Now.ToShortTimeString() + "   Export completed. See results below: \n" + results);
        }

        void BuildGame()
        {
            var buildVars = BuildVars;
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "uwb.xr.unityembed"); // change package name to unityembed
            BuildUtil.InstallGradlew(Cleanup.kNativeAppDir, Cleanup.ExportedGameDir);
            string error = BuildUtil.Gradlew(Cleanup.ExportedGameDir, "assembleDebug", buildVars);
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, buildVars.PackageName); // change package name back to custom setting
            bool success = !error.Contains("FAILURE:");
            string message = System.DateTime.Now.ToShortTimeString() + "   Build game done. See results below: \n" + error;
            if (success)
                Debug.Log(message);
            else
                Debug.LogError(message);
        }

        void BuildApp()
        {
            string error = BuildUtil.Gradlew(Cleanup.kNativeAppDir, "assembleDebug", BuildVars);
            bool success = !error.Contains("FAILURE:");
            string message = System.DateTime.Now.ToShortTimeString() + "   Build app done. See results below: \n" + error;
            if (success)
                Debug.Log(message);
            else
                Debug.LogError(message);
        }

        void InstallApp()
        {
            string error = BuildUtil.Gradlew(Cleanup.kNativeAppDir, "installDebug", BuildVars);
            bool success = !error.Contains("FAILURE:");
            string message = System.DateTime.Now.ToShortTimeString() + "   Install app done. See results below: \n" + error;
            if (success)
                Debug.Log(message);
            else
                Debug.LogError(message);
        }
    }
}
#endif