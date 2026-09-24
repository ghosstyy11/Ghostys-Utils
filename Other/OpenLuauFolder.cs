using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Ghosty.Other
{
    public class OpenLuauFolder : Editor
    {
        [MenuItem("Utils/Other/Open Luau code editor", priority =500)]
        private static void OpenThing()
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string luauPath = Path.Combine(projectRoot, "Luau");

            if (!Directory.Exists(luauPath))
            {
                UnityEngine.Debug.LogError($"Luau folder not found at: {luauPath}");
                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c code \"{luauPath}\"",
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            });
        }
    }
}
