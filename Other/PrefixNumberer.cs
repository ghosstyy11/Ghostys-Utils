using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Ghosty.Other
{
    public class PrefixNumberer : EditorWindow
    {

        private string prefix = "Object";

        [MenuItem("Utils/Other/Prefix Numberer", priority = 500)]
        public static void ShowWindow()
        {
            var window = EditorWindow.GetWindow<PrefixNumberer>();
            window.minSize = new Vector2(350, 350);
            window.titleContent = new GUIContent("Prefix Numberer");
        }

        void OnGUI()
        {
            GUILayout.Space(8);

            GUILayout.Label("To use this, select all the objects you need to number,\nenter a prefix, then click go.");

            GUILayout.Space(4);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Space(2);

            prefix = EditorGUILayout.TextField(prefix);
            GUILayout.Space(2);
            if (GUILayout.Button("Go"))
            {
                GameObject[] selectedObjects = Selection.gameObjects;

                Undo.SetCurrentGroupName("Rename Objects");
                int undoGroup = Undo.GetCurrentGroup();

                float i = 0;
                foreach (GameObject obj in selectedObjects)
                {
                    i++;
                    obj.name = prefix+i.ToString();
                }

                Undo.CollapseUndoOperations(undoGroup);
            }

        }
    }
}
