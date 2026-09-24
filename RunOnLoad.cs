using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using HarmonyLib;

namespace Ghosty
{
    [InitializeOnLoad]
    public static class RunOnLoad
    {
        private const string SessionKey = "ProjectStartupOnce_Ran";
        private const string AssetPath = "Assets/GhostysUtilsSettings.asset";

        private static SettingsData _settings;


        public static bool pendingUpload;
        public static string exportedMapPath;

        static RunOnLoad()
        {
            EnsureSettings();

            //Application.logMessageReceived += OnLog;
            EditorApplication.update += OnUpdate;

            var harmony = new Harmony("com.ghosty.ghostysutils");
            harmony.PatchAll();

            if (SessionState.GetBool(SessionKey, false))
                return;

            EditorApplication.delayCall += OnEditorReady;
            SessionState.SetBool(SessionKey, true);
        }

        private static void OnUpdate()
        {
            if (!pendingUpload || string.IsNullOrEmpty(exportedMapPath))
                return;

            pendingUpload = false;

            if ((_settings != null && !_settings.uploadOnExport) || _settings == null) // so technically if settings is nil then it will upload anyway lmao
                return;

            if (_settings != null && _settings.askBeforeUpload)
            {
                bool upload = EditorUtility.DisplayDialog("Wait!", "Do you want to upload the exported map to Mod.io?", "Yes", "No");
                if (!upload)
                    return;
            }

            EditorApplication.delayCall += () =>
            {
                Uploader.Upload(exportedMapPath);
            };
        }

        private static void OnEditorReady()
        {
            if (_settings != null && _settings.openCodeStartup)
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

        public static SettingsData Settings
        {
            get
            {
                if (_settings == null)
                    EnsureSettings();
                return _settings;
            }
        }

        private static void EnsureSettings()
        {
            _settings = AssetDatabase.LoadAssetAtPath<SettingsData>(AssetPath);
            if (_settings != null)
                return;

            _settings = ScriptableObject.CreateInstance<SettingsData>();
            AssetDatabase.CreateAsset(_settings, AssetPath);
            AssetDatabase.SaveAssets();
        }
    }
}
