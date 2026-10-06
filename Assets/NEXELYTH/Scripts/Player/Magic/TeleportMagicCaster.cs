using UnityEngine;
using UnityEngine.InputSystem;

public class TeleportMagicCaster : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform headTransform;
    [SerializeField] private GameObject magicCirclePrefab;

    [Header("Input")]
    [SerializeField] private InputActionReference triggerAction;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnDistance = 1.5f;

    private void OnEnable()
    {
        if (triggerAction != null)
        {
            triggerAction.action.Enable();
            triggerAction.action.performed += OnTriggerPerformed;
        }
    }

    private void OnDisable()
    {
        if (triggerAction != null)
        {
            triggerAction.action.performed -= OnTriggerPerformed;
            triggerAction.action.Disable();
        }
    }

    private void OnTriggerPerformed(InputAction.CallbackContext context)
    {
        Cast();
    }

    public void Cast()
    {
        Debug.Log("[TeleportMagicCaster] Cast triggered.");

        SpawnMagicCircle();
    }

    private void SpawnMagicCircle()
    {
        if (headTransform == null)
        {
            Debug.LogWarning("[TeleportMagicCaster] Head Transform is missing.");
            return;
        }

        if (magicCirclePrefab == null)
        {
            Debug.LogWarning("[TeleportMagicCaster] Magic Circle Prefab is missing.");
            return;
        }

        // 取玩家「現在看向的方向」
        Vector3 forward = headTransform.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.forward;

        forward.Normalize();

        // 以玩家目前頭部位置為基準，在前方生成
        Vector3 targetPosition =
            headTransform.position +
            forward * spawnDistance;

        Vector3 rayOrigin =
            targetPosition +
            Vector3.up * 2f;

        if (Physics.Raycast(
            rayOrigin,
            Vector3.down,
            out RaycastHit hit,
            5f,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore))
        {
            Vector3 spawnPosition =
                hit.point +
                hit.normal * 0.01f;

            Quaternion spawnRotation =
                Quaternion.FromToRotation(
                    Vector3.up,
                    hit.normal);

            Instantiate(
                magicCirclePrefab,
                spawnPosition,
                spawnRotation
            );
        }
        else
        {
            Debug.LogWarning(
                "[TeleportMagicCaster] Ground not found.");
        }
    }
}