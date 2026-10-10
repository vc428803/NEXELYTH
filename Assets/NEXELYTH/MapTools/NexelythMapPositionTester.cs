
using UnityEngine;
using NEXELYTH.MapTools;

public class NexelythMapPositionTester : MonoBehaviour
{
    [Header("Player Tracking")]
    [SerializeField]
    private Transform playerTransform;

    [Header("Regional Map JSON")]
    [SerializeField]
    private TextAsset regionalBoundsJson;

    [Header("Debug")]
    [SerializeField]
    private bool logPosition = false;

    private NexelythMapBoundsData bounds;

    private void Start()
    {
        if (regionalBoundsJson == null)
        {
            Debug.LogError(
                "[NEXELYTH] Regional bounds JSON missing."
            );
            enabled = false;
            return;
        }

        if (playerTransform == null)
        {
            var xrOrigin =
                FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();

            if (xrOrigin != null)
            {
                playerTransform = xrOrigin.transform;

                Debug.Log(
                    "[NEXELYTH] XR Origin found automatically: " +
                    playerTransform.name
                );
            }
            else
            {
                Debug.LogError(
                    "[NEXELYTH] XR Origin not found. " +
                    "Make sure NexelythBootstrap is loaded."
                );

                enabled = false;
                return;
            }
        }

        bounds = JsonUtility.FromJson<NexelythMapBoundsData>(
            regionalBoundsJson.text
        );

        Debug.Log(
            "[NEXELYTH] Regional map coordinates initialized."
        );
    }

    private void Update()
    {
        Vector3 worldPosition = playerTransform.position;

        Vector2 uv =
            NexelythMapCoordinateConverter.WorldToUV(
                worldPosition,
                bounds
            );

        if (!logPosition)
            return;

        Vector2 pixel =
            NexelythMapCoordinateConverter.UVToPixel(
                uv,
                bounds
            );

        Debug.Log(
            $"[NEXELYTH MAP] World: {worldPosition} " +
            $"UV: {uv} Pixel: {pixel} " +
            $"Inside: {NexelythMapCoordinateConverter.IsInsideMap(uv)}"
        );
    }
}
