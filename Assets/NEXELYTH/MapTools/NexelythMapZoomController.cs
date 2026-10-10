
using UnityEngine;
using UnityEngine.InputSystem;

public class NexelythMapZoomController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform mapAnchor;
    [SerializeField] private GameObject worldMap;
    [SerializeField] private Transform playerHead;

    [Header("Zoom Settings")]
    [SerializeField] private float minDistance = 0.45f;
    [SerializeField] private float maxDistance = 2.0f;
    [SerializeField] private float zoomStep = 0.25f;
    [SerializeField] private float smoothSpeed = 12f;

    private float targetDistance;
    private float currentDistance;
    private Vector3 zoomDirection;
    private Vector3 anchorOrigin;
    private bool wasOpen;

    private void Start()
    {
        if (playerHead == null && Camera.main != null)
            playerHead = Camera.main.transform;

        if (mapAnchor == null ||
            worldMap == null ||
            playerHead == null)
        {
            Debug.LogError(
                "[NEXELYTH MAP] Zoom references missing."
            );

            enabled = false;
        }
    }

    private void LateUpdate()
    {
        if (!worldMap.activeInHierarchy)
        {
            wasOpen = false;
            return;
        }

        // Every time the map opens, capture its new position.
        if (!wasOpen)
        {
            anchorOrigin = playerHead.position;

            Vector3 offset =
                mapAnchor.position - anchorOrigin;

            currentDistance = offset.magnitude;

            if (currentDistance < 0.01f)
            {
                currentDistance = 1.2f;
                zoomDirection = playerHead.forward;
            }
            else
            {
                zoomDirection = offset.normalized;
            }

            targetDistance = Mathf.Clamp(
                currentDistance,
                minDistance,
                maxDistance
            );

            wasOpen = true;

            Debug.Log(
                "[NEXELYTH MAP] Zoom initialized."
            );
        }

        Mouse mouse = Mouse.current;

        if (mouse != null)
        {
            float scroll = mouse.scroll.ReadValue().y;

            if (Mathf.Abs(scroll) > 0.01f)
            {
                targetDistance -=
                    (scroll / 120f) * zoomStep;

                targetDistance = Mathf.Clamp(
                    targetDistance,
                    minDistance,
                    maxDistance
                );

                Debug.Log(
                    "[NEXELYTH MAP] Zoom target: " +
                    targetDistance.ToString("F2")
                );
            }
        }

        currentDistance = Mathf.MoveTowards(
            currentDistance,
            targetDistance,
            smoothSpeed * Time.deltaTime
        );

        mapAnchor.position =
            anchorOrigin +
            zoomDirection * currentDistance;
    }
}
