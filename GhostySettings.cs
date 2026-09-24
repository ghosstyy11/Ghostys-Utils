using Ghosty;
using UnityEditor;
using UnityEngine;

public class GhostySettings : EditorWindow
{
    SettingsData settings;

    //private string resetLuauDataMessage = "";

    [MenuItem("Utils/Settings", priority = 0)]
    public static void ShowWindow()
    {
        var window = GetWindow<GhostySettings>();
        window.minSize = new Vector2(350, 350);
        window.titleContent = new GUIContent("Settings");
    }

    void OnEnable()
    {
        settings = RunOnLoad.Settings;
    }

    void OnGUI()
    {
        GUILayout.Space(8);
        Color oldColor = GUI.color;
        GUI.backgroundColor = Color.lightBlue;
        /*if (GUILayout.Button("Documentation"))
        {
            Application.OpenURL("https://ghosty.uk/utils/docs");
        }*/ //not yet
        GUI.backgroundColor = Color.lightGreen;
        if (GUILayout.Button("Github"))
        {
            Application.OpenURL("https://github.com/lewis6777/Ghostys-Utils");
        }
        /*GUI.backgroundColor = Color.mediumPurple;
        if (GUILayout.Button("Ghosty's Discord"))
        {
            Application.OpenURL("https://discord.gg/cpKA5mtMBF");
        }*/
        GUI.backgroundColor = Color.lightSalmon;
        if (GUILayout.Button("Gorilla Tag Modding Group"))
        {
            Application.OpenURL("https://discord.gg/monkemod");
        }
        GUI.backgroundColor = oldColor;

        GUILayout.Space(4);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("Startup", EditorStyles.boldLabel);
        GUILayout.Space(4);

        EditorGUI.BeginChangeCheck();

        settings.openCodeStartup = EditorGUILayout.ToggleLeft(
            new GUIContent(
                "Open Luau code editor on startup",
                "This will open the Luau folder in your code editor. (e.g. vs code)"
            ),
            settings.openCodeStartup
        );


        GUILayout.Space(4);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("Export", EditorStyles.boldLabel);
        GUILayout.Space(4);

        settings.uploadOnExport = EditorGUILayout.ToggleLeft(
            new GUIContent(
                "Upload to Mod.io on map export",
                "After exporting your map, it will automatically upload it to mod.io.\n(as long as the uploader window is set up correctly)"
            ),
            settings.uploadOnExport
        );
        GUILayout.Label("(Uploader window must be configured)", EditorStyles.miniLabel);

        GUILayout.Space(2);

        EditorGUI.indentLevel++;
        EditorGUI.BeginDisabledGroup(!settings.uploadOnExport);
        settings.askBeforeUpload = !EditorGUILayout.ToggleLeft(
            new GUIContent(
                "Don't ask before uploading",
                "The prompt asking you if you want to upload your map after export will be skipped."
            ),
            settings.askBeforeUpload
        );
        EditorGUI.EndDisabledGroup();
        EditorGUI.indentLevel--;

        /*GUILayout.Space(4);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("Luau", EditorStyles.boldLabel);
        GUILayout.Space(4);

        if (GUILayout.Button("Reset Luau save data"))
        {
            
            if (string.IsNullOrEmpty(settings.luauSaveData))
            {
                resetLuauDataMessage = "<color=red>There is no Luau data saved.</color>";
            }
            else
            {
                settings.luauSaveData = "";
                resetLuauDataMessage = "<color=green>Reset Luau save data!</color>";
            }
        }
        GUIStyle textStyle = new GUIStyle(EditorStyles.miniLabel);
        textStyle.richText = true;
        GUILayout.Label(resetLuauDataMessage, textStyle);*/ //coming in a different version

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(settings);
        }
    }
}