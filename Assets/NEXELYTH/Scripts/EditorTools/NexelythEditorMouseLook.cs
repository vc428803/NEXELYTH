
using UnityEngine;
using UnityEngine.InputSystem;

public class NexelythEditorMouseLook : MonoBehaviour
{
#if UNITY_EDITOR
    [Header("References")]
    [SerializeField] private Transform cameraOffset;

    [Header("Mouse Look")]
    [SerializeField] private float sensitivity = 0.12f;
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;

    private float yaw;
    private float pitch;

    private void Start()
    {
        if (cameraOffset == null)
        {
            Debug.LogError(
                "[NEXELYTH] Camera Offset is not assigned.");
            enabled = false;
            return;
        }

        yaw = transform.localEulerAngles.y;
        pitch = NormalizeAngle(cameraOffset.localEulerAngles.x);
    }

    private void LateUpdate()
    {
        Mouse mouse = Mouse.current;

        if (mouse == null ||
            !mouse.rightButton.isPressed)
        {
            return;
        }

        Vector2 delta = mouse.delta.ReadValue();

        yaw += delta.x * sensitivity;
        pitch -= delta.y * sensitivity;

        pitch = Mathf.Clamp(
            pitch,
            minPitch,
            maxPitch
        );

        transform.localRotation =
            Quaternion.Euler(0f, yaw, 0f);

        cameraOffset.localRotation =
            Quaternion.Euler(pitch, 0f, 0f);
    }

    private static float NormalizeAngle(float angle)
    {
        if (angle > 180f)
            angle -= 360f;

        return angle;
    }
#endif
}
