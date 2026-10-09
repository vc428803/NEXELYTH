
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.XR.CoreUtils;

public class NexelythBootstrapLoader : MonoBehaviour
{
    [Header("World Settings")]
    [SerializeField]
    private string worldSceneName = "Aurevane";

    [Header("Player Spawn Settings")]
    [SerializeField]
    private string spawnPointName = "Aurevane_DefaultSpawn";

    private IEnumerator Start()
    {
        // 1. 載入 Aurevane，不重新建立 XR Origin
        Scene worldScene =
            SceneManager.GetSceneByName(worldSceneName);

        if (!worldScene.isLoaded)
        {
            AsyncOperation operation =
                SceneManager.LoadSceneAsync(
                    worldSceneName,
                    LoadSceneMode.Additive
                );

            if (operation == null)
            {
                Debug.LogError(
                    "[NEXELYTH] Failed to start loading world scene."
                );
                yield break;
            }

            yield return operation;
        }

        // 2. 確認世界場景載入成功
        worldScene =
            SceneManager.GetSceneByName(worldSceneName);

        if (!worldScene.IsValid() || !worldScene.isLoaded)
        {
            Debug.LogError(
                $"[NEXELYTH] Scene not loaded: {worldSceneName}"
            );
            yield break;
        }

        // 3. 找到 Aurevane 的出生點
        Transform spawnPoint = FindSpawnPoint(worldScene);

        if (spawnPoint == null)
        {
            Debug.LogError(
                $"[NEXELYTH] Spawn point not found: {spawnPointName}"
            );
            yield break;
        }

        // 4. 取得 Bootstrap 中現有的 XR Origin
        XROrigin xrOrigin =
            FindFirstObjectByType<XROrigin>();

        if (xrOrigin == null || xrOrigin.Camera == null)
        {
            Debug.LogError(
                "[NEXELYTH] XR Origin or Camera not found."
            );
            yield break;
        }

        // 5. 定位期間暫停 Character Controller 碰撞
        CharacterController controller =
            xrOrigin.GetComponent<CharacterController>();

        bool controllerWasEnabled =
            controller != null && controller.enabled;

        if (controller != null)
            controller.enabled = false;

        // 6. 調整玩家朝向
        // 使用 XR Camera 的目前水平朝向來補償頭部偏移
        float cameraYaw =
            xrOrigin.Camera.transform.eulerAngles.y;

        float targetYaw =
            spawnPoint.eulerAngles.y;

        xrOrigin.transform.Rotate(
            0f,
            targetYaw - cameraYaw,
            0f,
            Space.World
        );

        // 7. 將 XR Camera 水平位置對齊出生點，
        // 並保留目前的 XR Camera 相對高度
        float cameraHeight =
            xrOrigin.CameraInOriginSpaceHeight;

        Vector3 targetCameraPosition =
            spawnPoint.position +
            Vector3.up * cameraHeight;

        xrOrigin.MoveCameraToWorldLocation(
            targetCameraPosition
        );

        // 8. 恢復 Character Controller
        if (controller != null)
            controller.enabled = controllerWasEnabled;

        Debug.Log(
            $"[NEXELYTH] Player spawned in {worldSceneName} " +
            $"at {spawnPoint.position}"
        );
    }

    private Transform FindSpawnPoint(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform[] transforms =
                root.GetComponentsInChildren<Transform>(true);

            foreach (Transform candidate in transforms)
            {
                if (candidate.name == spawnPointName)
                    return candidate;
            }
        }

        return null;
    }
}
