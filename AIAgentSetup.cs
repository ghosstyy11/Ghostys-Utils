using System;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using GT_CustomMapSupportRuntime;
using GT_CustomMapSupportEditor;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Ghosty
{
    public class AIAgentSetup : EditorWindow
    {

        private int agentIndex = 0;
        private string[] agentNames = { "Humanoid", "Small", "Medium", "Large" };

        private string newAgentName = "AgentName";
        private float moveSpeed = 6f;
        private float turnSpeed = 300f;
        private float acceleration = 12f;

        private int behaviourIndex = 0;
        private string[] behaviourTypes = { "Default", "Preset Luau" };


        [MenuItem("Utils/AI Agent Setup", priority = 100)]
        public static void ShowWindow()
        {
            var window = EditorWindow.GetWindow<AIAgentSetup>();
            window.minSize = new Vector2(350, 350);
            window.titleContent = new GUIContent("AI Agent Setup");
        }

        void OnGUI()
        {
            GUILayout.Space(8);
            /*Color oldColor = GUI.color;
            GUI.backgroundColor = Color.lightBlue;
            if (GUILayout.Button("Documentation"))
            {
                Application.OpenURL("https://ghosty.uk/utils/docs/");
            }

            GUI.backgroundColor = oldColor;*/

            GUILayout.Space(4);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Space(2);

            GUILayout.Label("Click the below button before trying anything else.");

            if (GUILayout.Button("Setup Project for AI Agents", EditorStyles.miniButton))
            {
                SetupProject();
            }

            GUILayout.Space(4);

            GUIContent agentName = new GUIContent("Agent Name", "This will be what the agent is called in Unity.");
            newAgentName = EditorGUILayout.TextField(agentName, newAgentName);

            GUIContent navmeshAgentContent = new GUIContent("Agent Type", "Gorilla Tag provides 4 preset Agent Types; which can be viewed inside of the built in Functionality Overview scene.");
            agentIndex = EditorGUILayout.Popup(navmeshAgentContent, agentIndex, agentNames);
            GUIContent agentBehaviourContent = new GUIContent("Behaviour Type", "For now there are two behaviours; the default Gorilla Tag one and a Luau one made by me.\nThe Luau behaviour will generate a file for the gamemode in Assets/Scripts.");
            behaviourIndex = EditorGUILayout.Popup(agentBehaviourContent, behaviourIndex, behaviourTypes);

            GUILayout.Space(8);

            moveSpeed = EditorGUILayout.FloatField("Move Speed", moveSpeed);
            turnSpeed = EditorGUILayout.FloatField("Turn Speed", turnSpeed);
            acceleration = EditorGUILayout.FloatField("Acceleration", acceleration);

            GUILayout.Space(6);

            if (GUILayout.Button("Add Agent", EditorStyles.miniButton))
            {
                AddAgent();
            }
        }

        void SetupProject()
        {
            // check to make sure they can setup project without issues

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

            GameObject Agents = GameObject.Find("Agents");
            Agents ??= new GameObject("Agents");
            Agents.transform.parent = mapDescriptor.transform;

            GameObject navMeshSurfaces = GameObject.Find("Navmesh Surfaces");
            navMeshSurfaces ??= new GameObject("Navmesh Surfaces");
            navMeshSurfaces.transform.parent = mapDescriptor.transform;

            UnityEngine.Debug.Log("Project setup for AI Agents!");
        }

        void AddAgent()
        {
            MapDescriptor mapDescriptor = GameObject.FindFirstObjectByType<MapDescriptor>();
            MapSpawnManager mapSpawnManager = GameObject.FindFirstObjectByType<MapSpawnManager>();
            GameObject Agents = GameObject.Find("Agents");
            GameObject navMeshSurfaces = GameObject.Find("Navmesh Surfaces");

            if (!mapDescriptor || !mapSpawnManager || !Agents || !navMeshSurfaces)
            {
                UnityEngine.Debug.LogError("There was an error when creating the Agent: something was missing! Make sure your scene has a map descriptor, and make sure that you have clicked the 'Setup Project' button");
                return;
            }
            else if (string.IsNullOrEmpty(newAgentName))
            {
                UnityEngine.Debug.LogError("You must set a name for the Agent!");
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


            GameObject templateObj = new GameObject(newAgentName + "_TEMPLATE");
            templateObj.transform.parent = mapSpawnManager.transform;

            GameObject modelHere = new GameObject("AGENT_MODEL_HERE");
            modelHere.transform.parent = templateObj.transform;

            AIAgent templateAgent = templateObj.AddComponent<AIAgent>();
            templateAgent.isTemplate = true;
            templateAgent.entityTypeId = (byte)selectedID;
            templateAgent.navAgentType = (NavAgentType)agentIndex;
            templateAgent.movementSpeed = moveSpeed;
            templateAgent.turnSpeed = turnSpeed;
            templateAgent.acceleration = acceleration;

            if (behaviourIndex == 0)
            {
                templateAgent.agentBehaviours = new List<AgentBehaviours> { AgentBehaviours.Chase, AgentBehaviours.Search };
            }

            if (!Directory.Exists("Assets/Prefabs/Agents"))
            {
                Directory.CreateDirectory("Assets/Prefabs/Agents");
                AssetDatabase.Refresh();
            }

            GameObject agentPrefab = PrefabUtility.SaveAsPrefabAsset(templateObj, $"Assets/Prefabs/Agents/{newAgentName}_TEMPLATE.prefab");
            GameObject.DestroyImmediate(templateObj);
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/Agents/{newAgentName}_TEMPLATE.prefab");
            GameObject prefabInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset);
            prefabInstance.transform.parent = mapSpawnManager.transform;


            GameObject nonTemplateObj = (GameObject)PrefabUtility.InstantiatePrefab(agentPrefab);
            nonTemplateObj.name = newAgentName;
            nonTemplateObj.transform.parent = Agents.transform;

            AIAgent nonTemplateAgent = nonTemplateObj.GetComponent<AIAgent>();
            nonTemplateAgent.isTemplate = false;
            nonTemplateAgent.transform.position = new Vector3 (0, 0, 10);

            GameObject existingNavmesh = GameObject.Find("Navmesh Surface (" + agentNames[agentIndex] + ")");
            if (existingNavmesh == null)
            {
                GameObject NavmeshSurfaceObj = new GameObject("Navmesh Surface (" + agentNames[agentIndex] + ")");
                NavmeshSurfaceObj.transform.parent = navMeshSurfaces.transform;
                NavMeshSurface surface = NavmeshSurfaceObj.AddComponent<NavMeshSurface>();
                surface.agentTypeID = NavMesh.GetSettingsByIndex(agentIndex).agentTypeID;
                surface.BuildNavMesh();
            }

            if (behaviourIndex == 1)
            {
                MapToolsMenuButtons.GenerateEntityIDs();

                GameObject pointsObj = GameObject.Find("LuauAgentPoints");
                pointsObj ??= new GameObject("LuauAgentPoints");
                pointsObj.transform.parent = mapDescriptor.transform;

                GameObject agentNumPoints = new GameObject("LuauAgent" + nonTemplateAgent.lua_EntityID.ToString());
                agentNumPoints.transform.parent = pointsObj.transform;

                for (int i = 1; i < 5; i++)
                {
                    GameObject agentNumPoint = new GameObject("LuauAgent" + nonTemplateAgent.lua_EntityID.ToString() + "Point" + i.ToString());
                    agentNumPoint.transform.parent = agentNumPoints.transform;
                }

                // now generate the gamemode
                // where is it

                string template = LoadLuauCodeThing();

                string ids = "";
                string nums = "";

                AIAgent[] allNewAgents = GameObject.FindObjectsOfType<AIAgent>();
                foreach (AIAgent agent in allNewAgents)
                {
                    if ((agent.agentBehaviours == null || agent.agentBehaviours.Count == 0) && !agent.isTemplate)
                    {
                        ids += agent.lua_EntityID + ", ";
                        nums += "11, ";
                    }
                }

                template = template.Replace("{AGENT_IDS}", ids);
                template = template.Replace("{SIGHT_DISTS}", nums);

                string outputPath = "Assets/Scripts/CustomAgentBehaviour.txt";

                if (!Directory.Exists("Assets/Scripts"))
                {
                    Directory.CreateDirectory("Assets/Scripts");
                }

                File.WriteAllText(outputPath, template);
                AssetDatabase.ImportAsset(outputPath, ImportAssetOptions.ForceUpdate);

                UnityEngine.Debug.Log("Make sure to move the Luau Agent Points around, their positions are where the monsters will move between when not chasing a person!");
            }


            UnityEngine.Debug.Log($"Agent created successfully! You can find it at: Assets/Prefabs/Agents/{newAgentName}_TEMPLATE.prefab");
            if (behaviourIndex == 1 )
            {
                UnityEngine.Debug.LogWarning("MAKE SURE you actually put the generated script inside of Assets/Scripts into the map export window.");
            }
        }

        static string LoadLuauCodeThing()
        {
            var assembly = Assembly.GetExecutingAssembly();

            using var stream = assembly.GetManifestResourceStream(
                "Ghosty.CustomAgentBehaviour.txt"
            );

            if (stream == null)
                throw new FileNotFoundException("Embedded resource not found");

            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
