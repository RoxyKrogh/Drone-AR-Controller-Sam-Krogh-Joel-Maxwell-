using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Android;
using UnityEngine;

namespace droneapi
{

    public class GradleProperties : IPostGenerateGradleAndroidProject
    {
        public static string DjiSdkKey { get { return BuildWindow.SavedParams.djiSdkKey; } } // get key from BuildWindow

        public int callbackOrder { get { return 999; } }

        void IPostGenerateGradleAndroidProject.OnPostGenerateGradleAndroidProject(string path)
        {
            // Manually add properties to build.gradle file during build
            Debug.Log("Bulid path : " + path);
            string gradlePropertiesFile = path + "/gradle.properties";
            if (File.Exists(gradlePropertiesFile))
            {
                File.Delete(gradlePropertiesFile);
            }
            StreamWriter writer = File.CreateText(gradlePropertiesFile);
            writer.WriteLine("org.gradle.jvmargs=-Xmx4096M");
            writer.WriteLine("djiSdkKey="+DjiSdkKey);
            writer.Flush();
            writer.Close();
        }
    }
}