using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Ghosty.Other
{
    public class RandomRotation : EditorWindow
    {

        private bool x = true;
        private float xMin = 0f;
        private float xMax = 360f;
        private bool y = true;
        private float yMin = 0f;
        private float yMax = 360f;
        private bool z = true;
        private float zMin = 0f;
        private float zMax = 360f;


        [MenuItem("Utils/Other/Random Rotation", priority = 500)]
        public static void ShowWindow()
        {
            var window = EditorWindow.GetWindow<RandomRotation>();
            window.minSize = new Vector2(350, 350);
            window.titleContent = new GUIContent("Random Rotation");
        }
        
        void OnGUI()
        {
            GUILayout.Space(8);

            GUILayout.Label("To use this, select all the objects you need to rotate,\nselect which axis you want to rotate on,\nthen click go.");

            GUILayout.Space(4);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Space(2);

            x = EditorGUILayout.ToggleLeft("X", x);
            if (x)
            {
                EditorGUI.indentLevel++;
                xMin = EditorGUILayout.Slider("Min", xMin, 0, 359);
                xMax = EditorGUILayout.Slider("Max", xMax, 1, 360);
                if (xMax <= xMin)
                    xMax = xMin+1f;
                EditorGUI.indentLevel--;
            }
            y = EditorGUILayout.ToggleLeft("Y", y);
            if (y)
            {
                EditorGUI.indentLevel++;
                yMin = EditorGUILayout.Slider("Min" ,yMin, 0, 359);
                yMax = EditorGUILayout.Slider("Max", yMax, 1, 360);
                if (yMax <= yMin)
                    yMax = yMin + 1f;
                EditorGUI.indentLevel--;
            }
            z = EditorGUILayout.ToggleLeft("Z", z);
            if (z)
            {
                EditorGUI.indentLevel++;
                zMin = EditorGUILayout.Slider("Min", zMin, 0, 359);
                zMax = EditorGUILayout.Slider("Max", zMax, 1, 360);
                if (zMax <= zMin)
                    zMax = zMin + 1f;
                EditorGUI.indentLevel--;
            }
            GUILayout.Space(2);
            if (GUILayout.Button("Go"))
            {
                foreach (Transform t in Selection.transforms)
                {
                    Undo.RecordObject(t, "Random Rotation");

                    Vector3 euler = t.eulerAngles;

                    if (x)
                        euler.x = UnityEngine.Random.Range(xMin, xMax);

                    if (y)
                        euler.y = UnityEngine.Random.Range(yMin, yMax);

                    if (z)
                        euler.z = UnityEngine.Random.Range(zMin, zMax);

                    t.rotation = Quaternion.Euler(euler);

                    EditorUtility.SetDirty(t);
                }
            }

        }
    }
}
