
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;

public class NexelythPortableMapController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform mapAnchor;
    [SerializeField] private GameObject worldMap;
    [SerializeField] private Transform playerHead;

    [Header("Placement")]
    [SerializeField] private float distance = 1.2f;
    [SerializeField] private float verticalOffset = -0.15f;

    [Header("Keyboard")]
    [SerializeField] private Key toggleKey = Key.M;

    private bool isOpen;

    private void Start()
    {
        if (playerHead == null)
        {
            XROrigin xrOrigin =
                FindFirstObjectByType<XROrigin>();

            if (xrOrigin != null && xrOrigin.Camera != null)
            {
                playerHead = xrOrigin.Camera.transform;
            }
        }

        if (mapAnchor == null ||
            worldMap == null ||
            playerHead == null)
        {
            Debug.LogError(
                "[NEXELYTH MAP] Missing references."
            );

            enabled = false;
            return;
        }

        worldMap.SetActive(false);
        isOpen = false;

        Debug.Log(
            "[NEXELYTH MAP] Portable map initialized."
        );
    }

    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current[toggleKey].wasPressedThisFrame)
        {
            ToggleMap();
        }
    }

    public void ToggleMap()
    {
        if (isOpen)
            CloseMap();
        else
            OpenMap();
    }

    public void OpenMap()
    {
        Vector3 forward = playerHead.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.forward;

        forward.Normalize();

        mapAnchor.position =
            playerHead.position +
            forward * distance +
            Vector3.up * verticalOffset;

        Quaternion facingRotation = Quaternion.LookRotation(
            forward,
            Vector3.up
        );

        mapAnchor.rotation =
            facingRotation * Quaternion.Euler(35f, 0f, 0f);

        worldMap.SetActive(true);
        isOpen = true;

        Debug.Log("[NEXELYTH MAP] Map opened.");
    }

    public void CloseMap()
    {
        worldMap.SetActive(false);
        isOpen = false;

        Debug.Log("[NEXELYTH MAP] Map closed.");
    }
}
