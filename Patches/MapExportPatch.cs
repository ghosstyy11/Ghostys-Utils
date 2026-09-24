using System.IO;
using GT_CustomMapSupportEditor;
using HarmonyLib;
using Ghosty;
using UnityEngine;

namespace Ghosty.Patches
{
    [HarmonyPatch(typeof(MapExporter))]
    [HarmonyPatch("PackageMap")]
    public static class MapExportPatch
    {
        static void Postfix(string path)
        {
            if (!File.Exists(path))
            {
                Debug.LogWarning("PackageMap finished, but the exported file does not exist: " + path);
                return;
            }

            Debug.Log("Map successfully exported to: " + path);

            RunOnLoad.exportedMapPath = path;
            RunOnLoad.pendingUpload = true;
        }
    }
}