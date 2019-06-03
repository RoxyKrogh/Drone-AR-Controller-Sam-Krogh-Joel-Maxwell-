using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

[InitializeOnLoad]
public static class LinkNativeView
{
    static LinkNativeView()
    {
        // Create symlinks if necessary
        if (!Directory.Exists("EmbeddedView/src/main/java/com/uwb/xr/djiBridge"))
        {
            AutomationUtility.Symlink("EmbeddedView/mainTemplate.gradle", "Assets/Plugins/Android/mainTemplate.gradle");
            AutomationUtility.Symlink("EmbeddedView/gradle.properties", "Assets/Plugins/Android/gradle.properties");
            AutomationUtility.Symlink("EmbeddedView/src/main/AndroidManifest.xml", "Assets/Plugins/Android/AndroidManifest.xml");
            AutomationUtility.Symlink("EmbeddedView/src/main/java/com/uwb/xr", "Assets/DroneAPI/Plugin/",true);
        }
    }
}
