
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DungeonColliderGenerator : EditorWindow
{
    private GameObject targetRoot;

    [MenuItem("NEXELYTH/Tools/Dungeon Collider Generator")]
    public static void ShowWindow()
    {
        GetWindow<DungeonColliderGenerator>(
            "Dungeon Colliders");
    }

    private void OnGUI()
    {
        GUILayout.Label(
            "NEXELYTH Dungeon Collider Generator",
            EditorStyles.boldLabel);

        EditorGUILayout.Space();

        targetRoot = (GameObject)EditorGUILayout.ObjectField(
            "Dungeon Models Root",
            targetRoot,
            typeof(GameObject),
            true);

        EditorGUILayout.HelpBox(
            "Select Aurevane > Environment_Aurevane > Models.\n" +
            "Only children of this object will be processed.",
            MessageType.Info);

        if (targetRoot == null)
            return;

        if (GUILayout.Button("Preview Collider Count"))
        {
            Preview();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate Colliders"))
        {
            if (EditorUtility.DisplayDialog(
                "Generate Dungeon Colliders",
                "Add colliders to eligible dungeon models?",
                "Generate",
                "Cancel"))
            {
                Generate();
            }
        }
    }

    private static bool IsSupported(string name)
    {
        return name.StartsWith("Tile_") ||
               name.StartsWith("Wall_") ||
               name.StartsWith("Pillar_") ||
               name.StartsWith("Arch_") ||
               name.StartsWith("Step_") ||
               name.StartsWith("Handrail_");
    }

    private static bool UseMeshCollider(string name)
    {
        return name.StartsWith("Arch_") ||
               name.StartsWith("Step_");
    }

    private List<GameObject> CollectTargets()
    {
        var results = new List<GameObject>();

        if (targetRoot == null)
            return results;

        foreach (MeshFilter filter in
                 targetRoot.GetComponentsInChildren<MeshFilter>(true))
        {
            GameObject obj = filter.gameObject;

            if (filter.sharedMesh == null)
                continue;

            if (!IsSupported(obj.name))
                continue;

            if (obj.GetComponent<Collider>() != null)
                continue;

            results.Add(obj);
        }

        return results;
    }

    private void Preview()
    {
        List<GameObject> targets = CollectTargets();

        int boxCount = 0;
        int meshCount = 0;

        foreach (GameObject obj in targets)
        {
            if (UseMeshCollider(obj.name))
                meshCount++;
            else
                boxCount++;
        }

        EditorUtility.DisplayDialog(
            "Dungeon Collider Preview",
            $"Total: {targets.Count}\n" +
            $"Box Collider: {boxCount}\n" +
            $"Mesh Collider: {meshCount}\n\n" +
            "No objects were modified.",
            "OK");
    }

    private void Generate()
    {
        List<GameObject> targets = CollectTargets();

        int generated = 0;

        Undo.SetCurrentGroupName(
            "Generate NEXELYTH Dungeon Colliders");

        int undoGroup = Undo.GetCurrentGroup();

        foreach (GameObject obj in targets)
        {
            if (UseMeshCollider(obj.name))
            {
                MeshCollider collider =
                    Undo.AddComponent<MeshCollider>(obj);

                collider.sharedMesh =
                    obj.GetComponent<MeshFilter>().sharedMesh;

                collider.convex = false;
                collider.isTrigger = false;
            }
            else
            {
                BoxCollider collider =
                    Undo.AddComponent<BoxCollider>(obj);

                Mesh mesh =
                    obj.GetComponent<MeshFilter>().sharedMesh;

                collider.center = mesh.bounds.center;
                collider.size = mesh.bounds.size;
                collider.isTrigger = false;
            }

            generated++;
        }

        Undo.CollapseUndoOperations(undoGroup);

        Debug.Log(
            $"[NEXELYTH] Generated {generated} colliders.");

        EditorUtility.DisplayDialog(
            "Dungeon Collider Generator",
            $"Successfully generated {generated} colliders.",
            "OK");
    }
}
#endif
