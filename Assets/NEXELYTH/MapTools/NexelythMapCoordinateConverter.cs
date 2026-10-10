
using UnityEngine;

namespace NEXELYTH.MapTools
{
    [System.Serializable]
    public class NexelythMapBoundsData
    {
        public string sceneName;
        public string imageFile;

        public int imageWidth;
        public int imageHeight;

        public float minX;
        public float maxX;
        public float minZ;
        public float maxZ;

        public Vector3 center;
        public Vector3 size;
    }

    public static class NexelythMapCoordinateConverter
    {
        // Unity World Position -> normalized map UV.
        // U: left 0 -> right 1
        // V: bottom 0 -> top 1
        public static Vector2 WorldToUV(
            Vector3 worldPosition,
            NexelythMapBoundsData bounds)
        {
            if (bounds == null)
            {
                Debug.LogError(
                    "[NEXELYTH] Map bounds data is null."
                );
                return Vector2.zero;
            }

            float width = bounds.maxX - bounds.minX;
            float height = bounds.maxZ - bounds.minZ;

            if (width <= 0f || height <= 0f)
            {
                Debug.LogError(
                    "[NEXELYTH] Invalid map bounds."
                );
                return Vector2.zero;
            }

            float u = (worldPosition.x - bounds.minX) / width;
            float v = (worldPosition.z - bounds.minZ) / height;

            return new Vector2(u, v);
        }

        // Normalized UV -> image pixel coordinate.
        // Pixel Y is measured from the image top.
        public static Vector2 UVToPixel(
            Vector2 uv,
            NexelythMapBoundsData bounds)
        {
            if (bounds == null ||
                bounds.imageWidth <= 0 ||
                bounds.imageHeight <= 0)
            {
                Debug.LogError(
                    "[NEXELYTH] Invalid image dimensions."
                );
                return Vector2.zero;
            }

            return new Vector2(
                uv.x * bounds.imageWidth,
                (1f - uv.y) * bounds.imageHeight
            );
        }

        public static bool IsInsideMap(Vector2 uv)
        {
            return uv.x >= 0f &&
                   uv.x <= 1f &&
                   uv.y >= 0f &&
                   uv.y <= 1f;
        }
    }
}
