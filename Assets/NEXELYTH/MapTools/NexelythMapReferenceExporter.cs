
using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class NexelythMapReferenceExporter
{
    private const int ExportResolution = 2048;

    [Serializable]
    private class PointData
    {
        public string name;
        public string hierarchyPath;
        public Vector3 position;
    }

    [Serializable]
    private class BoundsData
    {
        public string name;
        public string type;
        public Vector3 center;
        public Vector3 size;
    }

    [Serializable]
    private class ExportData
    {
        public string sceneName;
        public string imageFile;
        public int imageWidth;
        public int imageHeight;

        public float minX;
        public float maxX;
        public float minZ;
        public float maxZ;

        public List<PointData> landmarks = new();
        public List<BoundsData> terrainBounds = new();
    }

    [Serializable]
    private class RegionalBoundsData
    {
        public string sceneName;
        public string imageFile;
        public int imageWidth;
        public int imageHeight;

        // Actual image coverage in Unity world coordinates.
        public float minX;
        public float maxX;
        public float minZ;
        public float maxZ;

        // Original regional collider bounds.
        public Vector3 center;
        public Vector3 size;
    }

    [MenuItem("NEXELYTH/Map Tools/Export Scene Reference")]
    public static void Export()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogError(
                "[NEXELYTH] Stop Play Mode before exporting."
            );
            return;
        }

        Scene scene = SceneManager.GetActiveScene();

        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogError(
                "[NEXELYTH] No valid active scene."
            );
            return;
        }

        try
        {
            var data = new ExportData
            {
                sceneName = scene.name,
                imageWidth = ExportResolution,
                imageHeight = ExportResolution
            };

            Bounds combined = new Bounds();
            bool hasBounds = false;

            CollectSceneData(
                scene,
                data,
                ref combined,
                ref hasBounds
            );

            if (!hasBounds)
            {
                Debug.LogError(
                    "[NEXELYTH] No terrain or mountain bounds found."
                );
                return;
            }

            // Add 5% margin around the complete world footprint.
            float padding = Mathf.Max(
                combined.size.x,
                combined.size.z
            ) * 0.05f;

            float minX = combined.min.x - padding;
            float maxX = combined.max.x + padding;
            float minZ = combined.min.z - padding;
            float maxZ = combined.max.z + padding;

            // Square image must represent square world coverage.
            SetSquareCoverage(
                minX,
                maxX,
                minZ,
                maxZ,
                out data.minX,
                out data.maxX,
                out data.minZ,
                out data.maxZ
            );

            string projectRoot =
                Directory.GetParent(Application.dataPath).FullName;

            string folder = Path.Combine(
                projectRoot,
                "MapReferences",
                scene.name
            );

            Directory.CreateDirectory(folder);

            // One unique identifier shared by this export batch.
            string exportId =
                DateTime.Now.ToString("yyyyMMdd_HHmmss_fff")
                + "_"
                + Guid.NewGuid().ToString("N").Substring(0, 8);

            // World map.
            data.imageFile =
                $"Forest_TopDown_{exportId}.png";

            string worldImagePath = Path.Combine(
                folder,
                data.imageFile
            );

            string worldJsonPath = Path.Combine(
                folder,
                $"Forest_Reference_{exportId}.json"
            );

            ExportTopDown(
                data.minX,
                data.maxX,
                data.minZ,
                data.maxZ,
                data.imageWidth,
                data.imageHeight,
                combined.max.y,
                worldImagePath
            );

            File.WriteAllText(
                worldJsonPath,
                JsonUtility.ToJson(data, true)
            );

            // Regional map.
            bool regionalExported = ExportRegionalMap(
                scene,
                combined,
                folder,
                exportId
            );

            Debug.Log(
                "[NEXELYTH] World map exported successfully.\n" +
                $"Folder: {folder}\n" +
                $"World image: {data.imageFile}\n" +
                $"World JSON: {Path.GetFileName(worldJsonPath)}\n" +
                $"Regional map: {(regionalExported ? "Exported" : "Skipped")}"
            );
        }
        catch (Exception ex)
        {
            Debug.LogError(
                "[NEXELYTH] Map export failed:\n" + ex
            );
        }
    }

    private static void CollectSceneData(
        Scene scene,
        ExportData data,
        ref Bounds combined,
        ref bool hasBounds)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            // Collect Unity Terrain.
            foreach (Terrain terrain in
                     root.GetComponentsInChildren<Terrain>(true))
            {
                if (terrain.terrainData == null)
                    continue;

                // Transform the terrain's local bounds to world AABB.
                Bounds localBounds = new Bounds(
                    terrain.terrainData.size * 0.5f,
                    terrain.terrainData.size
                );

                Bounds worldBounds = TransformBounds(
                    terrain.transform,
                    localBounds
                );

                AddBounds(
                    data,
                    worldBounds,
                    terrain.name,
                    "Terrain",
                    ref combined,
                    ref hasBounds
                );
            }

            // Collect mountains, cliffs and rocks.
            foreach (MeshRenderer renderer in
                     root.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (!renderer.enabled ||
                    !renderer.gameObject.activeInHierarchy)
                {
                    continue;
                }

                string path =
                    GetPath(renderer.transform).ToLowerInvariant();

                if (!path.Contains("mountain") &&
                    !path.Contains("cliff") &&
                    !path.Contains("rock"))
                {
                    continue;
                }

                AddBounds(
                    data,
                    renderer.bounds,
                    renderer.name,
                    "Mesh",
                    ref combined,
                    ref hasBounds
                );
            }

            // Collect explicit landmarks.
            foreach (Transform t in
                     root.GetComponentsInChildren<Transform>(true))
            {
                if (!t.name.StartsWith(
                        "MAP_",
                        StringComparison.Ordinal))
                {
                    continue;
                }

                // RegionalBounds is an area, not a landmark.
                if (t.name == "MAP_RegionalBounds")
                    continue;

                data.landmarks.Add(new PointData
                {
                    name = t.name,
                    hierarchyPath = GetPath(t),
                    position = t.position
                });
            }
        }
    }

    private static bool ExportRegionalMap(
        Scene scene,
        Bounds combined,
        string folder,
        string exportId)
    {
        Transform regionalTransform = null;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Transform t in
                     root.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "MAP_RegionalBounds")
                {
                    regionalTransform = t;
                    break;
                }
            }

            if (regionalTransform != null)
                break;
        }

        if (regionalTransform == null)
        {
            Debug.LogWarning(
                "[NEXELYTH] MAP_RegionalBounds not found. " +
                "Regional export skipped."
            );
            return false;
        }

        BoxCollider box =
            regionalTransform.GetComponent<BoxCollider>();

        if (box == null)
        {
            Debug.LogWarning(
                "[NEXELYTH] MAP_RegionalBounds has no BoxCollider. " +
                "Regional export skipped."
            );
            return false;
        }

        // Compute world AABB from local BoxCollider geometry.
        // Works even if the collider component is disabled.
        Bounds regionalBounds = TransformBounds(
            regionalTransform,
            new Bounds(box.center, box.size)
        );

        if (regionalBounds.size.x <= 0f ||
            regionalBounds.size.z <= 0f)
        {
            Debug.LogWarning(
                "[NEXELYTH] Regional bounds are invalid. " +
                "Regional export skipped."
            );
            return false;
        }

        SetSquareCoverage(
            regionalBounds.min.x,
            regionalBounds.max.x,
            regionalBounds.min.z,
            regionalBounds.max.z,
            out float minX,
            out float maxX,
            out float minZ,
            out float maxZ
        );

        string imageName =
            $"Forest_Regional_TopDown_{exportId}.png";

        string jsonName =
            $"Forest_RegionalBounds_{exportId}.json";

        var regionalData = new RegionalBoundsData
        {
            sceneName = scene.name,
            imageFile = imageName,
            imageWidth = ExportResolution,
            imageHeight = ExportResolution,

            minX = minX,
            maxX = maxX,
            minZ = minZ,
            maxZ = maxZ,

            center = regionalBounds.center,
            size = regionalBounds.size
        };

        // The image and JSON share the same world coverage.
        ExportTopDown(
            regionalData.minX,
            regionalData.maxX,
            regionalData.minZ,
            regionalData.maxZ,
            regionalData.imageWidth,
            regionalData.imageHeight,
            combined.max.y,
            Path.Combine(folder, imageName)
        );

        File.WriteAllText(
            Path.Combine(folder, jsonName),
            JsonUtility.ToJson(regionalData, true)
        );

        Debug.Log(
            "[NEXELYTH] Regional map exported.\n" +
            $"Image: {imageName}\n" +
            $"JSON: {jsonName}\n" +
            $"World X: {minX:F2} to {maxX:F2}\n" +
            $"World Z: {minZ:F2} to {maxZ:F2}"
        );

        return true;
    }

    private static void AddBounds(
        ExportData data,
        Bounds bounds,
        string name,
        string type,
        ref Bounds combined,
        ref bool hasBounds)
    {
        data.terrainBounds.Add(new BoundsData
        {
            name = name,
            type = type,
            center = bounds.center,
            size = bounds.size
        });

        if (!hasBounds)
        {
            combined = bounds;
            hasBounds = true;
        }
        else
        {
            combined.Encapsulate(bounds);
        }
    }

    private static void SetSquareCoverage(
        float minX,
        float maxX,
        float minZ,
        float maxZ,
        out float squareMinX,
        out float squareMaxX,
        out float squareMinZ,
        out float squareMaxZ)
    {
        float centerX = (minX + maxX) * 0.5f;
        float centerZ = (minZ + maxZ) * 0.5f;

        float span = Mathf.Max(
            maxX - minX,
            maxZ - minZ
        );

        float halfSpan = span * 0.5f;

        squareMinX = centerX - halfSpan;
        squareMaxX = centerX + halfSpan;

        squareMinZ = centerZ - halfSpan;
        squareMaxZ = centerZ + halfSpan;
    }

    private static Bounds TransformBounds(
        Transform transform,
        Bounds localBounds)
    {
        Vector3 center = localBounds.center;
        Vector3 extents = localBounds.extents;

        Vector3 worldMin = new Vector3(
            float.PositiveInfinity,
            float.PositiveInfinity,
            float.PositiveInfinity
        );

        Vector3 worldMax = new Vector3(
            float.NegativeInfinity,
            float.NegativeInfinity,
            float.NegativeInfinity
        );

        for (int x = -1; x <= 1; x += 2)
        {
            for (int y = -1; y <= 1; y += 2)
            {
                for (int z = -1; z <= 1; z += 2)
                {
                    Vector3 localCorner = center +
                        Vector3.Scale(
                            new Vector3(x, y, z),
                            extents
                        );

                    Vector3 worldCorner =
                        transform.TransformPoint(localCorner);

                    worldMin = Vector3.Min(
                        worldMin,
                        worldCorner
                    );

                    worldMax = Vector3.Max(
                        worldMax,
                        worldCorner
                    );
                }
            }
        }

        Bounds result = new Bounds();
        result.SetMinMax(worldMin, worldMax);

        return result;
    }

    private static string GetPath(Transform transform)
    {
        string path = transform.name;

        while (transform.parent != null)
        {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }

        return path;
    }

    private static void ExportTopDown(
        float minX,
        float maxX,
        float minZ,
        float maxZ,
        int imageWidth,
        int imageHeight,
        float highestY,
        string outputPath)
    {
        GameObject cameraObject = null;
        RenderTexture renderTexture = null;
        Texture2D image = null;

        RenderTexture previousActive =
            RenderTexture.active;

        try
        {
            cameraObject = new GameObject(
                "__NexelythMapExportCamera"
            );

            // Avoid accidentally saving this temporary object.
            cameraObject.hideFlags = HideFlags.HideAndDontSave;

            Camera camera =
                cameraObject.AddComponent<Camera>();

            float width = maxX - minX;
            float depth = maxZ - minZ;

            float span = Mathf.Max(width, depth);

            if (span <= 0f)
            {
                throw new InvalidOperationException(
                    "Invalid map coverage."
                );
            }

            camera.orthographic = true;
            camera.orthographicSize = span * 0.5f;
            camera.aspect = 1f;

            camera.transform.position = new Vector3(
                (minX + maxX) * 0.5f,
                highestY + 1000f,
                (minZ + maxZ) * 0.5f
            );

            // +Z appears at the top of the exported image.
            camera.transform.rotation =
                Quaternion.Euler(90f, 0f, 0f);

            camera.clearFlags =
                CameraClearFlags.SolidColor;

            camera.backgroundColor = Color.black;

            camera.nearClipPlane = 0.1f;

            // Ensure the camera can reach the lowest geometry.
            camera.farClipPlane = Mathf.Max(
                5000f,
                1000f + Mathf.Abs(highestY) + 5000f
            );

            // Prevent export camera from rendering to the editor.
            camera.enabled = false;

            renderTexture = new RenderTexture(
                imageWidth,
                imageHeight,
                24,
                RenderTextureFormat.ARGB32
            );

            renderTexture.Create();

            camera.targetTexture = renderTexture;
            camera.Render();

            RenderTexture.active = renderTexture;

            image = new Texture2D(
                imageWidth,
                imageHeight,
                TextureFormat.RGB24,
                false
            );

            image.ReadPixels(
                new Rect(
                    0,
                    0,
                    imageWidth,
                    imageHeight
                ),
                0,
                0
            );

            image.Apply();

            byte[] pngBytes = image.EncodeToPNG();

            File.WriteAllBytes(
                outputPath,
                pngBytes
            );

            Debug.Log(
                $"[NEXELYTH] PNG exported: {outputPath}"
            );
        }
        finally
        {
            if (cameraObject != null)
            {
                Camera camera =
                    cameraObject.GetComponent<Camera>();

                if (camera != null)
                    camera.targetTexture = null;
            }

            RenderTexture.active = previousActive;

            if (image != null)
            {
                UnityEngine.Object.DestroyImmediate(image);
            }

            if (renderTexture != null)
            {
                renderTexture.Release();

                UnityEngine.Object.DestroyImmediate(
                    renderTexture
                );
            }

            if (cameraObject != null)
            {
                UnityEngine.Object.DestroyImmediate(
                    cameraObject
                );
            }
        }
    }
}
