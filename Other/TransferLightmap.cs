using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Ghosty.Other
{
    public class TransferLightmap : EditorWindow
    {
        private Renderer targetRend;
        private Renderer originalRend;


        [MenuItem("Utils/Other/Transfer lightmap", priority = 500)]
        public static void ShowWindow()
        {
            var window = EditorWindow.GetWindow<TransferLightmap>();
            window.minSize = new Vector2(350, 350);
            window.titleContent = new GUIContent("Transfer lightmap");
        }

        void OnGUI()
        {
            GUILayout.Space(8);

            GUILayout.Label("This can be used to place lightmaps from one object\nonto another object. (ideally the same mesh)");

            GUILayout.Space(4);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Space(2);

            GUILayout.Label("Target is the object to change lightmap of.\n(The dupe object)");
            targetRend = (Renderer)EditorGUILayout.ObjectField("Target Renderer", targetRend, typeof(Renderer), true);
            GUILayout.Label("Original is the object to get the lightmap from.\n(The original object)");
            originalRend = (Renderer)EditorGUILayout.ObjectField("Original Renderer", originalRend, typeof(Renderer), true);

            if (GUILayout.Button("Set Lightmaps", EditorStyles.miniButton))
            {
                targetRend.lightmapIndex = originalRend.lightmapIndex;
                targetRend.lightmapScaleOffset = originalRend.lightmapScaleOffset;
                EditorUtility.SetDirty(targetRend);
            }
        }
    }
}
