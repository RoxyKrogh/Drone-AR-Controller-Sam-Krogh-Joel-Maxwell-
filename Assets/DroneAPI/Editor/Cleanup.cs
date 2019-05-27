/*
 * Hide the NativeAndroidBuild directory and delete .meta files after loading 
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace droneapi
{

    [InitializeOnLoad]
    public class Cleanup
    {
        public const string kApiRootDir = "Assets/DroneAPI";
        public const string kBuildDir = kApiRootDir + "/Editor/Build";
        public const string kHiddenBuildDir = kApiRootDir + "/Editor/.Build";
        public const string kNativeAppDir = kHiddenBuildDir + "/NativeAndroid";
        public const string kExportDir = "exported";

        static Cleanup()
        {
            string hiddenDir = kHiddenBuildDir;
            if (HiddenBuildDirExists)
                CleanupMetaFiles(hiddenDir);
        }

        public static string ExportedGameDir
        {
            get { return kExportDir + "/" + PlayerSettings.productName; }
        }

        public static void CleanupMetaFiles(string rootdir)
        {
            if (Directory.GetFiles(rootdir, "*.meta", SearchOption.TopDirectoryOnly).Length == 0)
                return; // if no meta files in top directory, skip full scan
            Debug.Log("Removing .meta files from: " + rootdir);
            string[] metas = Directory.GetFiles(rootdir, "*.meta", SearchOption.AllDirectories);
            foreach (string f in metas)
                FileUtil.DeleteFileOrDirectory(f);
        }

        public static bool VisibleBuildDirExists
        {
            get { return Directory.Exists(kBuildDir); }
        }

        public static bool HiddenBuildDirExists
        {
            get { return Directory.Exists(kHiddenBuildDir); }
        }

        public static bool HideBuildDir()
        {
            if (VisibleBuildDirExists)
            {
                if (HiddenBuildDirExists)
                    return false;
                Debug.Log("Hiding directory: " + kBuildDir);
                FileUtil.CopyFileOrDirectory(kBuildDir, kHiddenBuildDir);
                /*if (HiddenDirExists(dirname))
                    FileUtil.DeleteFileOrDirectory(dirname);*/
                return true;
            }
            return HiddenBuildDirExists;
        }
    }
}