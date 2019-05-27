using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace droneapi
{
    public class BuildUtil
    {

        public struct BuildVariables
        {
            private string androidSdkHome;
            private string javaHome;
            private string djiSdkKey;
            private string packageName;

            public BuildVariables(string androidSdkHome, string javaHome, string djiSdkKey)
            {
                this.androidSdkHome = androidSdkHome;
                this.javaHome = javaHome;
                this.djiSdkKey = djiSdkKey;
                this.packageName = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);
            }

            public string AndroidSdkHome { get { return AndroidSdkHome; } }
            public string JavaHome { get { return javaHome; } }
            public string DjiSdkKey { get { return djiSdkKey; } }
            public string PackageName { get { return packageName; } }
        }

        public static string GetSavedAndroidSdkPath(string gradleRootPath, BuildVariables buildVars)
        {
            string savedSdkString = buildVars.AndroidSdkHome;
            string propertiesFile = gradleRootPath+"/local.properties";
            try
            {
                if (!Directory.Exists(buildVars.AndroidSdkHome) && Directory.Exists(buildVars.AndroidSdkHome) && File.Exists(propertiesFile))
                {
                    string text = File.ReadAllText(propertiesFile);
                    savedSdkString = GetSavedAndroidSdkPath(text, savedSdkString);
                }
            }
            catch (IOException e)
            {
                Debug.Log(e);
            }
            return savedSdkString;
        }

        private static string GetSavedAndroidSdkPath(string text, string defaultValue)
        {
            string savedSdkString = defaultValue;
            var match = Regex.Match(text, "(^|\\s)sdk.dir=([^\n]*)(\\s|$)", RegexOptions.Multiline);
            if (match.Success)
            {
                var valueGroup = match.Groups[2];
                if (valueGroup.Captures.Count > 0)
                {
                    savedSdkString = valueGroup.Captures[0].Value;
                    savedSdkString = savedSdkString.Replace(@"\:", @":").Replace(@"\\", @"\");
                }
            }
            return savedSdkString;
        }

        public static void WriteAndroidSdkPath(string gradleRootPath, BuildVariables buildVars)
        {
            string propertiesFile = gradleRootPath+"/local.properties";
            try
            {
                if (Directory.Exists(gradleRootPath) && Directory.Exists(gradleRootPath))
                {
                    if (!File.Exists(propertiesFile))
                    {
                        File.Create(propertiesFile).Close();
                        File.WriteAllText(propertiesFile, "sdk.dir="+buildVars.AndroidSdkHome+"\n");
                    }
                    else
                    {
                        if (buildVars.AndroidSdkHome == "")
                            return;
                        string text = File.ReadAllText(propertiesFile);
                        string savedSdkPath = GetSavedAndroidSdkPath(text, buildVars.AndroidSdkHome);
                        if (savedSdkPath == buildVars.AndroidSdkHome)
                            return; // do not rewrite if value is same
                        string key = "="+savedSdkPath+"\n";
                        if (text.Contains(key))
                            text = text.Replace(key, "="+buildVars.AndroidSdkHome+"\n");
                        else
                            text = text + "sdk.dir="+buildVars.AndroidSdkHome+"\n";
                        File.WriteAllText(propertiesFile, text);
                    }
                }
            }
            catch (IOException e)
            {
                Debug.Log(e);
            }
        }
        public static void InstallGradlew(string gradleRootPath, string targetRootPath)
        {
            if (!File.Exists(targetRootPath + "/gradlew.bat"))
                FileUtil.CopyFileOrDirectory(Cleanup.kNativeAppDir + "/gradlew.bat", targetRootPath + "/gradlew.bat");
            if (!File.Exists(targetRootPath + "/gradlew"))
                FileUtil.CopyFileOrDirectory(Cleanup.kNativeAppDir + "/gradlew", targetRootPath + "/gradlew");
            if (!Directory.Exists(targetRootPath + "/gradle"))
                FileUtil.CopyFileOrDirectory(Cleanup.kNativeAppDir + "/gradle", targetRootPath + "/gradle");
        }

        public static string FixPath(string path)
        {
            string fpath = Path.GetFullPath(path);
            return fpath;
            //return path.Replace('/', Path.DirectorySeparatorChar).Replace(@"\\", @"\").Replace(@"\", @"/").Trim(new char[] { '"', '\'' }).Replace(@"/ "," ").Replace(" ",@"\ ");
        }

        public static string Gradlew(string workingDirectory, string task, BuildVariables buildVars, Dictionary<string,string> pvariables = null)
        {
            string arguments = task;
            //arguments += $" -Dorg.gradle.java.home={FixPath(buildVars.JavaHome)}";
            if (pvariables != null && pvariables.Count > 0)
            {
                foreach (KeyValuePair<string,string> p in pvariables)
                {
                    arguments += " -P"+p.Key+"="+p.Value; // append build property value
                }
            }
            return ExecuteCommand(FixPath(workingDirectory + "/" + GetGradlewExe()), arguments, workingDirectory);
        }

        public static string ExecuteCommand(string command, string arguments = null, string workingDirectory = null)
        {
            Debug.Log(command + " " + arguments + " @ " + workingDirectory);
            var outputText = new StringBuilder();
            var errorText = new StringBuilder();

            var startInfo = new System.Diagnostics.ProcessStartInfo(command, arguments)
            {
                RedirectStandardError = true,
                UseShellExecute = false,
                WorkingDirectory = workingDirectory
            };
            using ( var process = System.Diagnostics.Process.Start(startInfo) )
            {
                // capture errors
                process.OutputDataReceived +=
                    (sendingProcess, errorLine) => { errorText.AppendLine(errorLine.Data); };
                process.ErrorDataReceived += 
                    (sendingProcess, errorLine) => { errorText.AppendLine(errorLine.Data); };
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
}