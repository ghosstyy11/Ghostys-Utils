using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.PackageManager.UI;

namespace Ghosty.Other
{
    public class GizmoMeshPreviewToolWindow : EditorWindow // this is made by grossacid, i had permission from him to add it.
    {
        private Color gizmoColor = new Color(0.3f, 1f, 0.6f, 0.4f);

        [MenuItem("Utils/Other/Gizmo Preview Tool", priority = 500)]
        public static void ShowWindow()
        {
            var window = GetWindow<GizmoMeshPreviewToolWindow>();
            window.minSize = new Vector2(350, 350);
            window.titleContent = new GUIContent("Gizmo Preview Tool");
        }

        private void OnGUI()
        {
            GUILayout.Space(8);

            GUILayout.Label("GrossAcid made this tool :)");

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Space(4);

            gizmoColor = EditorGUILayout.ColorField("Gizmo Color", gizmoColor);

            if (GUILayout.Button("Convert Selected to GizmoMeshPreview"))
            {
                ConvertSelectedToGizmoPreview(gizmoColor);
            }

            if (GUILayout.Button("Delete Converted Preview Meshes"))
            {
                DeleteConvertedPreviews();
            }
        }

        private void ConvertSelectedToGizmoPreview(Color color)
        {
            var selectedGO = Selection.activeGameObject;
            if (selectedGO == null)
            {
                Debug.LogWarning("Select a GameObject first.");
                return;
            }

            Undo.RegisterCompleteObjectUndo(selectedGO, "Convert to GizmoMeshPreview");

            var renderers = selectedGO.GetComponentsInChildren<MeshRenderer>(true);
            if (renderers.Length == 0)
            {
                Debug.LogWarning("No MeshRenderers found under selected object.");
                return;
            }

            var preview = selectedGO.GetComponent<GizmoMeshPreview>();
            if (preview == null)
                preview = Undo.AddComponent<GizmoMeshPreview>(selectedGO);

            preview.gizmoColor = color;
            preview.previewMeshes.Clear();

            int count = 0;
            foreach (var renderer in renderers)
            {
                var meshFilter = renderer.GetComponent<MeshFilter>();
                if (meshFilter == null || meshFilter.sharedMesh == null) continue;

                // Correct matrix calculation relative to root to preserve relative transform
                Matrix4x4 localMatrix = selectedGO.transform.worldToLocalMatrix * renderer.transform.localToWorldMatrix;

                DecomposeMatrix(localMatrix, out Vector3 localPos, out Quaternion localRot, out Vector3 localScale);

                preview.previewMeshes.Add(new GizmoMeshPreview.PreviewMesh
                {
                    mesh = meshFilter.sharedMesh,
                    localPosition = localPos,
                    localRotation = localRot,
                    localScale = localScale,
                    originalObject = renderer.gameObject
                });

                count++;
            }

            Debug.Log($"[GizmoMeshPreview] Converted {count} mesh(es) to gizmo on '{selectedGO.name}'.");
        }

        private void DeleteConvertedPreviews()
        {
            var selectedGO = Selection.activeGameObject;
            if (selectedGO == null)
            {
                Debug.LogWarning("Select the converted object to clean up.");
                return;
            }

            var preview = selectedGO.GetComponent<GizmoMeshPreview>();
            if (preview == null || preview.previewMeshes.Count == 0)
            {
                Debug.LogWarning("No converted gizmo data found on selected object.");
                return;
            }

            int deleted = 0;
            Undo.RegisterCompleteObjectUndo(selectedGO, "Delete Converted Previews");

            foreach (var mesh in preview.previewMeshes)
            {
                if (mesh.originalObject != null)
                {
                    Undo.DestroyObjectImmediate(mesh.originalObject);
                    deleted++;
                }
            }

            Debug.Log($"[GizmoMeshPreview] Deleted {deleted} original mesh object(s) from '{selectedGO.name}'.");
        }

        private static void DecomposeMatrix(Matrix4x4 m, out Vector3 position, out Quaternion rotation, out Vector3 scale)
        {
            position = m.GetColumn(3);

            Vector3 right = m.GetColumn(0);
            Vector3 up = m.GetColumn(1);
            Vector3 forward = m.GetColumn(2);

            scale = new Vector3(right.magnitude, up.magnitude, forward.magnitude);

            right /= scale.x;
            up /= scale.y;
            forward /= scale.z;

            rotation = Quaternion.LookRotation(forward, up);
        }
    }

    [ExecuteInEditMode]
    public class GizmoMeshPreview : MonoBehaviour
    {
        [System.Serializable]
        public class PreviewMesh
        {
            public Mesh mesh;
            public Vector3 localPosition;
            public Quaternion localRotation;
            public Vector3 localScale = Vector3.one;
            public GameObject originalObject; // Track for cleanup
        }

        public List<PreviewMesh> previewMeshes = new List<PreviewMesh>();

        public Color gizmoColor = new Color(0.3f, 1f, 0.6f, 0.4f);

        private void OnDrawGizmos()
        {
            Gizmos.color = gizmoColor;
            foreach (var p in previewMeshes)
            {
                if (p.mesh == null) continue;

                Matrix4x4 matrix = transform.localToWorldMatrix *
                                   Matrix4x4.TRS(p.localPosition, p.localRotation, p.localScale);

                DecomposeMatrix(matrix, out Vector3 pos, out Quaternion rot, out Vector3 scale);

                Gizmos.DrawMesh(p.mesh, pos, rot, scale);
            }
        }

        private static void DecomposeMatrix(Matrix4x4 m, out Vector3 position, out Quaternion rotation, out Vector3 scale)
        {
            position = m.GetColumn(3);

            Vector3 right = m.GetColumn(0);
            Vector3 up = m.GetColumn(1);
            Vector3 forward = m.GetColumn(2);

            scale = new Vector3(right.magnitude, up.magnitude, forward.magnitude);

            right /= scale.x;
            up /= scale.y;
            forward /= scale.z;

            rotation = Quaternion.LookRotation(forward, up);
        }
    }
}