
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

    public Vector3 CurrentWorldPosition { get; private set; }
    public Vector2 CurrentUV { get; private set; }
    public Vector2 CurrentPixel { get; private set; }
    public bool IsInsideMap { get; private set; }

    public bool IsInitialized { get; private set; }

    private void Start()
    {
        if (regionalBoundsJson == null)
        {
            Debug.LogError(
                "[NEXELYTH MAP] Regional bounds JSON missing."
            );

            enabled = false;
            return;
        }

        if (playerTransform == null)
        {
            var xrOrigin =
                FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();

            if (xrOrigin == null)
            {
                Debug.LogError(
                    "[NEXELYTH MAP] XR Origin not found."
                );

                enabled = false;
                return;
            }

            playerTransform = xrOrigin.Camera != null
                ? xrOrigin.Camera.transform
                : xrOrigin.transform;
        }

        bounds = JsonUtility.FromJson<NexelythMapBoundsData>(
            regionalBoundsJson.text
        );

        if (bounds == null)
        {
            Debug.LogError(
                "[NEXELYTH MAP] Invalid bounds JSON."
            );

            enabled = false;
            return;
        }

        IsInitialized = true;

        Debug.Log(
            "[NEXELYTH MAP] Regional coordinates initialized."
        );
    }

    private void Update()
    {
        if (!IsInitialized || playerTransform == null)
            return;

        CurrentWorldPosition = playerTransform.position;

        CurrentUV =
            NexelythMapCoordinateConverter.WorldToUV(
                CurrentWorldPosition,
                bounds
            );

        CurrentPixel =
            NexelythMapCoordinateConverter.UVToPixel(
                CurrentUV,
                bounds
            );

        IsInsideMap =
            NexelythMapCoordinateConverter.IsInsideMap(
                CurrentUV
            );

        if (logPosition)
        {
            Debug.Log(
                $"[NEXELYTH MAP] " +
                $"World: {CurrentWorldPosition} " +
                $"UV: {CurrentUV} " +
                $"Pixel: {CurrentPixel} " +
                $"Inside: {IsInsideMap}"
            );
        }
    }
}
