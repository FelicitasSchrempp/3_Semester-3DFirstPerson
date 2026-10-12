using UnityEditor;
using UnityEngine;

/// <summary>
/// Assigns a dragged material to the submesh under the cursor in the Scene view, with live preview.
/// Determines the submesh via a mesh raycast, so it works with the GPU Resident Drawer enabled.
/// Hold Alt to assign the material to all slots.
/// </summary>
[InitializeOnLoad]
public static class SubmeshMaterialDrop
{
    /// <summary>Renderer that currently shows the preview material.</summary>
    private static Renderer previewRenderer;

    /// <summary>Original materials of the preview renderer, restored when the preview ends.</summary>
    private static Material[] originalMaterials;

    /// <summary>Mesh whose data is currently cached.</summary>
    private static Mesh cachedMesh;

    /// <summary>Cached vertex positions of the cached mesh.</summary>
    private static Vector3[] cachedVertices;

    /// <summary>Cached triangle indices per submesh, null for non-triangle submeshes.</summary>
    private static int[][] cachedTriangles;

    static SubmeshMaterialDrop()
    {
        SceneView.duringSceneGui += OnSceneGui;
    }

    /// <summary>
    /// Handles drag update, perform and exit events for materials.
    /// </summary>
    private static void OnSceneGui(SceneView sceneView)
    {
        Event evt = Event.current;

        if (evt.type == EventType.DragExited)
        {
            EndDrag();
            return;
        }

        if (evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform)
            return;

        if (DragAndDrop.objectReferences.Length != 1 || DragAndDrop.objectReferences[0] is not Material material)
        {
            RestorePreview();
            return;
        }

        GameObject target = HandleUtility.PickGameObject(evt.mousePosition, false);
        if (target == null
            || !target.TryGetComponent(out MeshRenderer renderer)
            || !target.TryGetComponent(out MeshFilter filter)
            || filter.sharedMesh == null)
        {
            RestorePreview();
            return;
        }

        bool assignAll = evt.alt;
        int submesh = -1;

        if (!assignAll)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(evt.mousePosition);
            submesh = FindHitSubmesh(filter.sharedMesh, target.transform, ray);

            if (submesh < 0 || submesh >= renderer.sharedMaterials.Length)
            {
                RestorePreview();
                return;
            }
        }

        DragAndDrop.visualMode = DragAndDropVisualMode.Link;

        if (evt.type == EventType.DragUpdated)
        {
            ApplyPreview(renderer, material, submesh, assignAll);
        }
        else
        {
            RestorePreview();
            DragAndDrop.AcceptDrag();
            AssignMaterial(renderer, material, submesh, assignAll);
            EndDrag();
        }

        evt.Use();
    }

    /// <summary>
    /// Temporarily shows the material on the target slot(s) without recording undo.
    /// </summary>
    private static void ApplyPreview(Renderer renderer, Material material, int submesh, bool assignAll)
    {
        if (previewRenderer != renderer)
        {
            RestorePreview();
            previewRenderer = renderer;
            originalMaterials = renderer.sharedMaterials;
        }

        Material[] preview = (Material[])originalMaterials.Clone();
        if (assignAll)
        {
            for (int i = 0; i < preview.Length; i++)
                preview[i] = material;
        }
        else
        {
            preview[submesh] = material;
        }

        renderer.sharedMaterials = preview;
    }

    /// <summary>
    /// Restores the original materials of the preview renderer, if any.
    /// </summary>
    private static void RestorePreview()
    {
        if (previewRenderer != null)
            previewRenderer.sharedMaterials = originalMaterials;

        previewRenderer = null;
        originalMaterials = null;
    }

    /// <summary>
    /// Ends the drag operation, restores any preview and releases the mesh cache.
    /// </summary>
    private static void EndDrag()
    {
        RestorePreview();
        cachedMesh = null;
        cachedVertices = null;
        cachedTriangles = null;
    }

    /// <summary>
    /// Assigns the material to one slot or to all slots with undo and prefab override support.
    /// </summary>
    private static void AssignMaterial(Renderer renderer, Material material, int submesh, bool assignAll)
    {
        Material[] materials = renderer.sharedMaterials;

        Undo.RecordObject(renderer, "Assign Material");

        if (assignAll)
        {
            for (int i = 0; i < materials.Length; i++)
                materials[i] = material;
        }
        else
        {
            materials[submesh] = material;
        }

        renderer.sharedMaterials = materials;
        PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
    }

    /// <summary>
    /// Caches vertices and triangles of the mesh so repeated raycasts do not allocate.
    /// </summary>
    private static void CacheMesh(Mesh mesh)
    {
        if (cachedMesh == mesh)
            return;

        cachedMesh = mesh;
        cachedVertices = mesh.vertices;
        cachedTriangles = new int[mesh.subMeshCount][];

        for (int s = 0; s < mesh.subMeshCount; s++)
        {
            if (mesh.GetTopology(s) == MeshTopology.Triangles)
                cachedTriangles[s] = mesh.GetTriangles(s);
        }
    }

    /// <summary>
    /// Returns the index of the closest submesh hit by the ray, or -1 if nothing was hit.
    /// </summary>
    private static int FindHitSubmesh(Mesh mesh, Transform transform, Ray worldRay)
    {
        CacheMesh(mesh);

        // Transform the ray into local space; direction is not normalized so hit distances stay comparable.
        Vector3 origin = transform.InverseTransformPoint(worldRay.origin);
        Vector3 direction = transform.InverseTransformVector(worldRay.direction);

        float closest = float.MaxValue;
        int result = -1;

        for (int s = 0; s < cachedTriangles.Length; s++)
        {
            int[] triangles = cachedTriangles[s];
            if (triangles == null)
                continue;

            for (int i = 0; i < triangles.Length; i += 3)
            {
                if (RayTriangle(origin, direction,
                        cachedVertices[triangles[i]], cachedVertices[triangles[i + 1]], cachedVertices[triangles[i + 2]],
                        out float distance)
                    && distance < closest)
                {
                    closest = distance;
                    result = s;
                }
            }
        }

        return result;
    }

    /// <summary>
    /// MÃ¶ller-Trumbore ray/triangle intersection, double-sided.
    /// </summary>
    private static bool RayTriangle(Vector3 origin, Vector3 direction, Vector3 v0, Vector3 v1, Vector3 v2, out float distance)
    {
        distance = 0f;
        Vector3 edge1 = v1 - v0;
        Vector3 edge2 = v2 - v0;
        Vector3 p = Vector3.Cross(direction, edge2);
        float det = Vector3.Dot(edge1, p);

        if (Mathf.Abs(det) < 1e-8f)
            return false;

        float invDet = 1f / det;
        Vector3 t = origin - v0;
        float u = Vector3.Dot(t, p) * invDet;
        if (u < 0f || u > 1f)
            return false;

        Vector3 q = Vector3.Cross(t, edge1);
        float v = Vector3.Dot(direction, q) * invDet;
        if (v < 0f || u + v > 1f)
            return false;

        distance = Vector3.Dot(edge2, q) * invDet;
        return distance > 0f;
    }
}