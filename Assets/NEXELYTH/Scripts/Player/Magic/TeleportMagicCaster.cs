using UnityEngine;
using UnityEngine.InputSystem;

public class TeleportMagicCaster : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerRoot;
    [SerializeField] private GameObject magicCirclePrefab;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnDistance = 1.5f;

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        bool shiftPressed =
            Keyboard.current.leftShiftKey.isPressed ||
            Keyboard.current.rightShiftKey.isPressed;

        bool tPressed =
            Keyboard.current.tKey.wasPressedThisFrame;

        if (shiftPressed && tPressed)
        {
            Cast();
        }
    }

    public void Cast()
    {
        Debug.Log("[TeleportMagicCaster] Cast triggered.");

        SpawnMagicCircle();
    }

    private void SpawnMagicCircle()
    {
        if (playerRoot == null)
        {
            Debug.LogWarning("[TeleportMagicCaster] Player Root is missing.");
            return;
        }

        if (magicCirclePrefab == null)
        {
            Debug.LogWarning("[TeleportMagicCaster] Magic Circle Prefab is missing.");
            return;
        }

        Vector3 forward = playerRoot.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.forward;

        forward.Normalize();

        Vector3 targetPosition =
            playerRoot.position +
            forward * spawnDistance;

        // 從目標位置上方往下打 Raycast 找地面
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
            // 稍微離地，避免 Z-Fighting
            Vector3 spawnPosition =
                hit.point +
                hit.normal * 0.01f;

            // 讓法陣貼合地面角度
            Quaternion spawnRotation =
                Quaternion.FromToRotation(
                    Vector3.up,
                    hit.normal);

            Instantiate(
                magicCirclePrefab,
                spawnPosition,
                spawnRotation
            );

            Debug.Log(
                $"[TeleportMagicCaster] Magic circle spawned on ground at {spawnPosition}.");
        }
        else
        {
            Debug.LogWarning(
                "[TeleportMagicCaster] Ground not found.");
        }
    }

}