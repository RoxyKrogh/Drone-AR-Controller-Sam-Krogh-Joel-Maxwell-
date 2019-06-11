using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;

public static class AutomationUtility
{
    private static string LocalPath(string path, string root="./")
    {
        Uri p = new Uri(Path.GetFullPath(path));
        Uri r = new Uri(Path.GetFullPath(root));
        return r.MakeRelativeUri(p).ToString();
    }

    private static string ConcatArgs(params string[] args)
    {
        return String.Join(" ", args);
    }

    private static bool IsWindows { get { return SystemInfo.operatingSystemFamily == OperatingSystemFamily.Windows; } }

    public static void Symlink(string link, string target, bool isDirectory = false)
    {
        link = "\"" + Path.GetFullPath(link) + "\"";
        target = "\"" + Path.GetFullPath(target) + "\"";
        string args = IsWindows ? ConcatArgs(link,target) : ConcatArgs(target,link); // parameter order reverse for cmd vs bash
        if (IsWindows && isDirectory)
            args = "/D " + args;
        args = (IsWindows ? "mklink " : "ln -s ") + args;
        ExecuteShell(args);
    }


    public static string ExecuteShell(string commands, string workingDirectory = null)
    {
        return ExecuteCommand(IsWindows ? "cmd" : "bash", (IsWindows ? "/c " : "-c ") + commands, workingDirectory, true);
    }

    public static string ExecuteCommand(string command, string arguments = null, string workingDirectory = null, bool useShell = false)
    {
        workingDirectory = Path.GetFullPath(workingDirectory ?? "./");
        if (!useShell)
            Debug.Log(command + " " + arguments + " @ " + workingDirectory);
        var outputText = new StringBuilder();
        var errorText = new StringBuilder();

        var startInfo = new System.Diagnostics.ProcessStartInfo(command, arguments)
        {
            RedirectStandardOutput = !useShell,
            RedirectStandardError = !useShell,
            UseShellExecute = useShell,
            WorkingDirectory = workingDirectory
        };
        using (var process = System.Diagnostics.Process.Start(startInfo))
        {
            if (!useShell)
            {
                // capture errors
                process.OutputDataReceived +=
                    (sendingProcess, errorLine) => { errorText.AppendLine(errorLine.Data); };
                process.ErrorDataReceived +=
                    (sendingProcess, errorLine) => { errorText.AppendLine(errorLine.Data); };
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }
            process.WaitForExit();
        }
        return errorText.ToString();
    }
}
