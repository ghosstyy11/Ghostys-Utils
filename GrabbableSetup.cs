using System;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using GT_CustomMapSupportRuntime;
using GT_CustomMapSupportEditor;
using System.Collections.Generic;
using System.IO;
using UnityEditor.PackageManager;

namespace Ghosty
{
    public class GrabbableSetup : EditorWindow // this still sucks but just wait until v0.4
    {
        private string newGrabbableName = "GrabbableName";

        private AudioClip catchSound;
        private float catchSoundVolume = 1f;
        private AudioClip throwSound;
        private float throwSoundVolume = 1f;


        [MenuItem("Utils/Grabbable Setup", priority = 100)]
        public static void ShowWindow()
        {
            var window = EditorWindow.GetWindow<GrabbableSetup>();
            window.minSize = new Vector2(350, 350);
            window.titleContent = new GUIContent("Grabbable Setup");
        }

        void OnGUI()
        {
            GUILayout.Space(8);
            GUILayout.Label("Documentation doesn't exist yet lol");
            /*Color oldColor = GUI.color;
            GUI.backgroundColor = Color.lightBlue;
            if (GUILayout.Button("Documentation"))
            {
                Application.OpenURL("https://ghostysstuff.com/maps/");
            }

            GUI.backgroundColor = oldColor;*/

            GUILayout.Space(4);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Space(2);

            GUILayout.Label("Click the below button before trying anything else.");

            if (GUILayout.Button("Setup Project for Grabbables", EditorStyles.miniButton))
            {
                SetupProject();
            }

            GUILayout.Space(4);

            GUIContent grabbableName = new GUIContent("Grabbable Name", "This will be what the grabbable is called in Unity.");
            newGrabbableName = EditorGUILayout.TextField(grabbableName, newGrabbableName);

            GUILayout.Space(8);

            GUIContent catchS = new GUIContent("Catch Sound", "This is the sound which will play when you catch/pick up the grabbable.");
            catchSound = (AudioClip)EditorGUILayout.ObjectField(catchS, catchSound, typeof(AudioClip), false);

            EditorGUI.indentLevel++;
            EditorGUI.BeginDisabledGroup(catchSound==null);
            GUIContent catchSVolume = new GUIContent("Catch Sound Volume", "This is the volume at which the catch sound will play at.");
            catchSoundVolume = EditorGUILayout.Slider(catchSVolume, catchSoundVolume, 0f, 1f);
            EditorGUI.EndDisabledGroup();
            EditorGUI.indentLevel--;

            GUILayout.Space(2);

            GUIContent throwS = new GUIContent("Throw Sound", "This is the sound which will play when you throw/let go of the grabbable.");
            throwSound = (AudioClip)EditorGUILayout.ObjectField(throwS, throwSound, typeof(AudioClip), false);

            EditorGUI.indentLevel++;
            EditorGUI.BeginDisabledGroup(throwSound == null);
            GUIContent throwSVolume = new GUIContent("Throw Sound Volume", "This is the volume at which the throw sound will play at.");
            throwSoundVolume = EditorGUILayout.Slider(throwSVolume, throwSoundVolume, 0f, 1f);
            EditorGUI.EndDisabledGroup();
            EditorGUI.indentLevel--;

            GUILayout.Space(6);

            if (GUILayout.Button("Add Grabbable", EditorStyles.miniButton))
            {
                AddGrabbable();
            }

        }

        void SetupProject()
        {
            MapDescriptor mapDescriptor = GameObject.FindFirstObjectByType<MapDescriptor>();
            if (mapDescriptor == null)
            {
                UnityEngine.Debug.LogError("You are missing a Map Descriptor!");
                return;
            }

            MapSpawnManager mapSpawnManager = GameObject.FindFirstObjectByType<MapSpawnManager>();
            if (mapSpawnManager == null)
            {
                GameObject mapSpawnObj = new GameObject("Map Spawn Manager");
                mapSpawnManager = mapSpawnObj.AddComponent<MapSpawnManager>();
                mapSpawnManager.transform.parent = mapDescriptor.transform;
            }

            GameObject Grabbables = GameObject.Find("Grabbables");
            Grabbables ??= new GameObject("Grabbables");
            Grabbables.transform.parent = mapDescriptor.transform;

            UnityEngine.Debug.Log("Project setup for Grabbables!");
        }

        void AddGrabbable()
        {
            MapDescriptor mapDescriptor = GameObject.FindFirstObjectByType<MapDescriptor>();
            MapSpawnManager mapSpawnManager = GameObject.FindFirstObjectByType<MapSpawnManager>();
            GameObject Grabbables = GameObject.Find("Grabbables");

            if (!mapDescriptor || !mapSpawnManager || !Grabbables)
            {
                UnityEngine.Debug.LogError("There was an error when creating the Grabbable: something was missing! Make sure your scene has a map descriptor, and make sure that you have clicked the 'Setup Project' button");
                return;
            }
            else if (string.IsNullOrEmpty(newGrabbableName))
            {
                UnityEngine.Debug.LogError("You must set a name for the Grabbable!");
                return;
            }

            MapEntity[] allEntities = GameObject.FindObjectsByType<MapEntity>(FindObjectsSortMode.InstanceID);
            HashSet<byte> usedIDs = new HashSet<byte>();

            foreach (MapEntity entity in allEntities)
            {
                if (!entity.isTemplate)
                    continue;

                usedIDs.Add(entity.entityTypeId);
            }

            byte selectedID = 1;
            while (usedIDs.Contains(selectedID))
            {
                selectedID++;
            }

            GameObject templateObj = new GameObject(newGrabbableName + "_TEMPLATE");
            templateObj.transform.parent = mapSpawnManager.transform;

            GameObject modelHere = new GameObject("GRABBABLE_MODEL_HERE");
            modelHere.transform.parent = templateObj.transform;

            GameObject audioObj = new GameObject("AudioSource");
            audioObj.transform.parent = templateObj.transform;
            AudioSource audio = audioObj.AddComponent<AudioSource>();

            GrabbableEntity templateGrabbable = templateObj.AddComponent<GrabbableEntity>();
            templateGrabbable.isTemplate = true;
            templateGrabbable.entityTypeId = (byte)selectedID;

            SphereCollider collider = templateObj.AddComponent<SphereCollider>();
            collider.radius = 0.1f;
            
            templateGrabbable.audioSource = audio;
            templateGrabbable.catchSound = catchSound;
            templateGrabbable.catchSoundVolume = catchSoundVolume;
            templateGrabbable.throwSound = throwSound;
            templateGrabbable.throwSoundVolume = throwSoundVolume;

            if (!Directory.Exists("Assets/Prefabs/Grabbables"))
            {
                Directory.CreateDirectory("Assets/Prefabs/Grabbables");
                AssetDatabase.Refresh();
            }

            GameObject grabbablePrefab = PrefabUtility.SaveAsPrefabAsset(templateObj, $"Assets/Prefabs/Grabbables/{newGrabbableName}_TEMPLATE.prefab");
            GameObject.DestroyImmediate(templateObj);
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/Grabbables/{newGrabbableName}_TEMPLATE.prefab");
            GameObject prefabInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset);
            prefabInstance.transform.parent = mapSpawnManager.transform;


            GameObject nonTemplateObj = (GameObject)PrefabUtility.InstantiatePrefab(grabbablePrefab);
            nonTemplateObj.name = newGrabbableName;
            nonTemplateObj.transform.parent = Grabbables.transform;

            GrabbableEntity nonTemplateGrabbable = nonTemplateObj.GetComponent<GrabbableEntity>();
            nonTemplateGrabbable.isTemplate = false;
            nonTemplateGrabbable.transform.position = new Vector3(0, 0, 10);

            MapToolsMenuButtons.GenerateEntityIDs();

            UnityEngine.Debug.Log($"Grabbable added successfully! You can find it at: Assets/Prefabs/Grabbables/{newGrabbableName}_TEMPLATE.prefab");
        }
    }
}
