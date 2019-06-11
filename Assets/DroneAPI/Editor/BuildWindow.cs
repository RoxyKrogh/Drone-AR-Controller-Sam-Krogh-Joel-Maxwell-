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
        const string OPERATION_TITLE = "Configure DJI SDK";
        [SerializeField] private string djiSdkKey;
        
        public class SdkParams { public string djiSdkKey; };
        
        /// <summary>
        /// External access to certain saved properties of BuildWindow.
        /// </summary>
        public static SdkParams SavedParams
        {
            get
            {
                SdkParams cfg = new SdkParams();
                var data = EditorPrefs.GetString(OPERATION_TITLE, JsonUtility.ToJson(cfg, true)); // get saved fields of BuildWindow
                JsonUtility.FromJsonOverwrite(data, cfg); // apply relevant fields to cfg
                return cfg;
            }
        }

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
            EditorGUILayout.LabelField("Configuration for DJI drone control integration.", EditorStyles.boldLabel);
            GUILayout.Space(20);

            djiSdkKey = EditorGUILayout.TextField("SDK Key: ", djiSdkKey);
            GUILayout.Space(20);

            EditorGUILayout.LabelField("DJI SDK Registration", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Every application needs a unique App Key to initialize the SDK.", EditorStyles.wordWrappedLabel);
            EditorGUILayout.LabelField("To create an App Key for an application:", EditorStyles.wordWrappedLabel);
            EditorGUILayout.LabelField("Go to the DJI developer Developer Center", EditorStyles.wordWrappedLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("• Select the \"Apps\" tab on the left.", EditorStyles.wordWrappedLabel);
            EditorGUILayout.LabelField("• Select the \"Create App\" button on the right.", EditorStyles.wordWrappedLabel);
            EditorGUILayout.LabelField("• Enter the name, platform, package identifier, category and description of the application.", EditorStyles.wordWrappedLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("• For Android, the package identifier is the Package Name.", EditorStyles.wordWrappedLabel);
            EditorGUI.indentLevel--;
            EditorGUILayout.LabelField("• An application activation email will be sent to complete App Key generation.", EditorStyles.wordWrappedLabel);
            EditorGUILayout.LabelField("• The App Key will appear in the developer center, and can be copied and pasted into the \"SDK Key\" field, above.", EditorStyles.wordWrappedLabel);
            EditorGUI.indentLevel--;
            GUILayout.Space(20);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Package Name: ",EditorStyles.boldLabel, GUILayout.MaxWidth(110));
            EditorGUILayout.LabelField(PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android),EditorStyles.textField);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Change Package Name in Player Settings")) Selection.activeObject = Unsupported.GetSerializedAssetInterfaceSingleton("PlayerSettings");
            if (GUILayout.Button("Manage SDK Keys")) Application.OpenURL("http://developer.dji.com/en/user/apps");
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(20);
        }
    }
}
#endif