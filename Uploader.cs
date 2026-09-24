using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Security.Cryptography;
using System.IO;
using System.Net.Http;
using System.Net;

namespace Ghosty
{
    class ProgressStreamContent : HttpContent
    {
        private const int defaultBufferSize = 81920;
        private Stream content;
        private int bufferSize;
        private Action<long> progress;

        public ProgressStreamContent(Stream content, Action<long> progress, int bufferSize = defaultBufferSize)
        {
            this.content = content;
            this.bufferSize = bufferSize;
            this.progress = progress;
        }

        protected override async System.Threading.Tasks.Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            var buffer = new byte[bufferSize];
            int bytesRead;

            while ((bytesRead = await content.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await stream.WriteAsync(buffer, 0, bytesRead);
                progress?.Invoke(bytesRead);
            }
        }

        protected override bool TryComputeLength(out long length)
        {
            length = content.Length;
            return true;
        }
    }

    public class Uploader : EditorWindow
    {
        SettingsData settings;

        public string mapId = "";
        public string filePath = "";
        public string modioEndpoint = "";
        public string token = "";

        static string response = "";

        static string version = "";
        static string changelog = "";

        bool showToken = false;

        static float uploadProgress = 0f; // gonna use this for a sort of upload percentage and time thing
        static long totalBytes = 0;
        static long uploadedBytes = 0;
        static DateTime uploadStartTime;
        static string progressText = "";

        [MenuItem("Utils/Uploader", priority = 200)]
        public static void ShowWindow()
        {
            Uploader wnd = GetWindow<Uploader>();
            wnd.minSize = new Vector2(350, 500);
            wnd.titleContent = new GUIContent("Uploader");
        }

        void OnEnable()
        {
            settings = RunOnLoad.Settings;
            mapId = settings.mapId;
            filePath = settings.filePath;
            modioEndpoint = EditorPrefs.GetString("Uploader_modioEndpoint", "");
            token = EditorPrefs.GetString("Uploader_token", "");
        }

        void OnDisable()
        {
            settings = RunOnLoad.Settings;
            settings.mapId = mapId;
            settings.filePath = filePath;
            EditorPrefs.SetString("Uploader_modioEndpoint", modioEndpoint);
            EditorPrefs.SetString("Uploader_token", token);
        }

        public void OnGUI()
        {
            GUILayout.Label("Map ID:");
            mapId = GUILayout.TextField(mapId);

            GUILayout.Label("Zip file path:");

            EditorGUILayout.BeginHorizontal();

            GUI.SetNextControlName("ZipPathField");
            filePath = EditorGUILayout.TextField(filePath);

            if (GUILayout.Button("Browse", GUILayout.Width(65)))
            {
                string projectRoot = Directory.GetParent(Application.dataPath).FullName;
                string exportsPath = Path.Combine(projectRoot, "Exports");
                string startDirectory = !string.IsNullOrEmpty(filePath)
                    ? Path.GetDirectoryName(filePath)
                    : Directory.Exists(exportsPath)
                        ? exportsPath
                        : projectRoot;

                string selected = EditorUtility.OpenFilePanel(
                    "Select Zip File",
                    startDirectory,
                    "zip"
                );
                if (!string.IsNullOrEmpty(selected))
                {
                    filePath = selected;
                    GUI.FocusControl(null);
                }
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Label("Mod.io API url:");
            modioEndpoint = EditorGUILayout.TextField(modioEndpoint);

            GUILayout.Label("Token:");
            GUILayout.BeginHorizontal();
            token = showToken
                ? EditorGUILayout.TextField(token)
                : EditorGUILayout.PasswordField(token);
            showToken = GUILayout.Toggle(showToken, "", GUILayout.Width(20));
            GUILayout.EndHorizontal();

            GUILayout.Space(6);

            if (settings != null)
            {
                settings.mapId = mapId;
                settings.filePath = filePath;
                EditorUtility.SetDirty(settings);
            }

            GUILayout.Label("Version:");
            version = EditorGUILayout.TextField(version);

            GUILayout.Label("Changelog:");
            changelog = EditorGUILayout.TextArea(changelog, GUILayout.Height(60));

            GUILayout.Space(6);

            if (GUILayout.Button("Upload"))
            {
                EditorPrefs.SetString("Uploader_modioEndpoint", modioEndpoint);
                EditorPrefs.SetString("Uploader_token", token);
                var settings = RunOnLoad.Settings;
                Upload(settings.filePath);
            }

            if (uploadProgress > 0f && uploadProgress < 1f)
            {
                GUILayout.Label(progressText);
            }

            GUILayout.Label("Response:");
            EditorGUILayout.LabelField(response, EditorStyles.wordWrappedLabel);
        }

        static string CalculateMD5(string filename)
        {
            using (var md5 = MD5.Create())
            using (var stream = File.OpenRead(filename))
            {
                var hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        public static void Upload(string path)
        {
            var settings = RunOnLoad.Settings;
            var mapId = settings.mapId;
            var filePath = path;
            var modioEndpoint = EditorPrefs.GetString("Uploader_modioEndpoint", "");
            var token = EditorPrefs.GetString("Uploader_token", "");

            response = "Uploading...";
            foreach (var w in Resources.FindObjectsOfTypeAll<Uploader>())
                w.Repaint();

            System.Threading.Tasks.Task.Run(() => // run this on a seperate thread so that it doesn't freeze unity
            {
                try
                {
                    var hash = CalculateMD5(filePath);
                    var url = $"{modioEndpoint}/games/6657/mods/{mapId}/files";

                    using (HttpClient client = new HttpClient())
                    {
                        client.Timeout = TimeSpan.FromMinutes(30);
                        MultipartFormDataContent formContent = new MultipartFormDataContent();

                        using (var stream = File.OpenRead(filePath))
                        {
                            totalBytes = stream.Length;
                            uploadedBytes = 0;
                            uploadProgress = 0f;
                            uploadStartTime = DateTime.UtcNow;
                            progressText = "Starting upload...";

                            var fileContent = new ProgressStreamContent(stream, (bytes) => // using this avoids issues with larger map files
                            {
                                uploadedBytes += bytes;
                                uploadProgress = (float)uploadedBytes / totalBytes;

                                double seconds = (DateTime.UtcNow - uploadStartTime).TotalSeconds;
                                double speed = uploadedBytes / seconds; // bytes/sec
                                double remainingBytes = totalBytes - uploadedBytes;
                                double eta = speed > 0 ? remainingBytes / speed : 0;

                                double speedMB = speed / 1024 / 1024;

                                progressText = $"{uploadProgress * 100f:0}% ({speedMB:0.0} MB/s, {eta:0}s remaining)";

                                EditorApplication.delayCall += () =>
                                {
                                    foreach (var w in Resources.FindObjectsOfTypeAll<Uploader>())
                                        w.Repaint();
                                };
                            }, 1024 * 1024);

                            formContent.Add(fileContent, "filedata", Path.GetFileName(filePath));
                            formContent.Add(new StringContent("true"), "active");
                            formContent.Add(new StringContent(hash), "filehash");

                            formContent.Add(new StringContent(version), "version");
                            formContent.Add(new StringContent(changelog), "changelog");

                            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url))
                            {
                                request.Headers.Add("Authorization", "Bearer " + token);
                                request.Headers.Add("Accept", "application/json");
                                request.Content = formContent;

                                using (HttpResponseMessage resp = client.SendAsync(request).Result)
                                {
                                    string result = resp.StatusCode + " " + resp.Content.ReadAsStringAsync().Result;

                                    EditorApplication.delayCall += () =>
                                    {
                                        response = result;
                                        Debug.Log(result);
                                        foreach (var w in Resources.FindObjectsOfTypeAll<Uploader>())
                                            w.Repaint();
                                    };
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    EditorApplication.delayCall += () =>
                    {
                        response = ex.Message;
                        Debug.LogException(ex);
                        foreach (var w in Resources.FindObjectsOfTypeAll<Uploader>())
                            w.Repaint();
                    };
                }
            });
        }


    }
}
