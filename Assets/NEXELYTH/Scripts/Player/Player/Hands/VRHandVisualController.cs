using UnityEngine;
using UnityEngine.InputSystem;

public class VRHandVisualController : MonoBehaviour
{
    [Header("Hand Root")]
    [SerializeField] private Transform handPoseRoot;

    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;

    [Header("Idle Pose")]
    [SerializeField]
    private Vector3 idlePosition =
        new Vector3(-0.1f, -0.2f, 0f);

    [SerializeField]
    private Vector3 idleRotation =
        new Vector3(-100f, -180f, 10f);

    [Header("Cast Pose")]
    [SerializeField]
    private Vector3 castPosition =
        new Vector3(0f, 0f, 0.05f);

    [SerializeField]
    private Vector3 castRotation =
        new Vector3(-10f, 0f, 0f);

    [Header("Walk Swing")]
    [SerializeField] private float swingSpeed = 6f;
    [SerializeField] private float swingSideAmount = 0.01f;
    [SerializeField] private float swingUpAmount = 0.01f;
    [SerializeField] private float swingForwardAmount = 0.04f;
    [SerializeField] private float swingRotationAmount = 6f;

    [Header("Blend")]
    [SerializeField] private float positionSpeed = 6f;
    [SerializeField] private float rotationSpeed = 6f;

    private void Update()
    {
        bool isCasting = IsCasting();
        bool isMoving = IsMoving();

        UpdateHandPose(isCasting, isMoving);
    }

    private bool IsCasting()
    {
        if (Keyboard.current == null)
            return false;

        return Keyboard.current.leftShiftKey.isPressed &&
               Keyboard.current.tKey.isPressed;
    }

    private bool IsMoving()
    {
        // 先讀 XR Move Action
        if (moveAction != null && moveAction.action != null)
        {
            Vector2 moveInput = moveAction.action.ReadValue<Vector2>();

            if (moveInput.sqrMagnitude > 0.01f)
                return true;
        }

        // 沒有 VR Controller 時，用 WASD 測試
        if (Keyboard.current != null)
        {
            return Keyboard.current.wKey.isPressed ||
                   Keyboard.current.aKey.isPressed ||
                   Keyboard.current.sKey.isPressed ||
                   Keyboard.current.dKey.isPressed;
        }

        return false;
    } 
    private void UpdateHandPose(bool isCasting, bool isMoving)
    {
        Vector3 targetPosition;
        Vector3 targetRotationEuler;

        if (isCasting)
        {
            // 施法時停止 Walking Swing
            targetPosition = castPosition;
            targetRotationEuler = castRotation;
        }
        else
        {
            targetPosition = idlePosition;
            targetRotationEuler = idleRotation;

            if (isMoving)
            {
                float swing =
                    Mathf.Sin(Time.time * swingSpeed);

                Vector3 swingOffset = new Vector3(
                    swing * swingSideAmount,
                    Mathf.Abs(swing) * swingUpAmount,
                    swing * swingForwardAmount
                );

                targetPosition += swingOffset;

                targetRotationEuler.z +=
                    swing * swingRotationAmount;
            }
        }

        Quaternion targetRotation =
            Quaternion.Euler(targetRotationEuler);

        handPoseRoot.localPosition =
            Vector3.Lerp(
                handPoseRoot.localPosition,
                targetPosition,
                Time.deltaTime * positionSpeed
            );

        handPoseRoot.localRotation =
            Quaternion.Slerp(
                handPoseRoot.localRotation,
                targetRotation,
                Time.deltaTime * rotationSpeed
            );
    }

    private void OnEnable()
    {
        if (moveAction != null && moveAction.action != null)
        {
            moveAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (moveAction != null && moveAction.action != null)
        {
            moveAction.action.Disable();
        }
    }
}