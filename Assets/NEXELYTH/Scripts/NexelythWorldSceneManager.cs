using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.XR.CoreUtils;

public class NexelythWorldSceneManager : MonoBehaviour
{
    public static NexelythWorldSceneManager Instance { get; private set; }

    [SerializeField]
    private string currentWorldSceneName = "Aurevane";

    private bool isTransitioning;

    private void Awake()
    {
        // 修改原因：確保整個 Bootstrap 只存在一個 World Scene Manager，
        // 讓所有 Portal 與世界切換功能共用同一個管理器。
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        // 修改原因：BootstrapLoader 會先載入初始 World Scene，
        // 啟動後也要替目前已載入的世界建立統一 MapSystem。
        StartCoroutine(
            InitializeCurrentWorldSceneRoutine()
        );
    }

    private IEnumerator InitializeCurrentWorldSceneRoutine()
    {
        // 修改原因：等待 BootstrapLoader 完成初始 World Scene 的 Additive 載入。
        yield return null;

        Scene currentScene =
            SceneManager.GetSceneByName(
                currentWorldSceneName
            );

        while (!currentScene.IsValid() ||
               !currentScene.isLoaded)
        {
            yield return null;

            currentScene =
                SceneManager.GetSceneByName(
                    currentWorldSceneName
                );
        }

        EnsureMapCoordinateSystem(
            currentScene
        );
    }

    public void ChangeWorldScene(
        string targetSceneName,
        Vector2Int targetCoordinate
    )
    {
        // 修改原因：避免玩家連續觸發 Portal 時，
        // 同一時間執行多次 Scene 切換。
        if (isTransitioning)
            return;

        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            Debug.LogError(
                "[NEXELYTH World Scene Manager] Target scene name is empty."
            );
            return;
        }

        StartCoroutine(
            ChangeWorldSceneRoutine(
                targetSceneName,
                targetCoordinate
            )
        );
    }

    private IEnumerator ChangeWorldSceneRoutine(
        string targetSceneName,
        Vector2Int targetCoordinate
    )
    {
        isTransitioning = true;

        // 修改原因：先以 Additive 方式載入目標世界，
        // Bootstrap 與 XR Origin 位於另一張 Scene，因此會持續存在。
        AsyncOperation loadOperation =
            SceneManager.LoadSceneAsync(
                targetSceneName,
                LoadSceneMode.Additive
            );

        if (loadOperation == null)
        {
            Debug.LogError(
                $"[NEXELYTH World Scene Manager] Failed to start loading scene: {targetSceneName}"
            );

            isTransitioning = false;
            yield break;
        }

        yield return loadOperation;

        Scene targetScene =
            SceneManager.GetSceneByName(
                targetSceneName
            );

        if (!targetScene.IsValid() ||
            !targetScene.isLoaded)
        {
            Debug.LogError(
                $"[NEXELYTH World Scene Manager] Loaded scene is invalid: {targetSceneName}"
            );

            isTransitioning = false;
            yield break;
        }

        // 修改原因：目標世界載入完成後，
        // 自動取得或建立該 Scene 專屬的座標系。
        NexelythMapCoordinateSystem coordinateSystem =
            EnsureMapCoordinateSystem(
                targetScene
            );

        if (coordinateSystem == null)
        {
            Debug.LogError(
                $"[NEXELYTH World Scene Manager] Map Coordinate System not found: {targetSceneName}"
            );

            isTransitioning = false;
            yield break;
        }

        // 修改原因：將目標地圖 X/Y 轉換成 Unity 世界座標，
        // 讓 Portal、Memo、Fast Travel 都能共用 Map + Coordinate 傳送格式。
        Vector3 targetWorldPosition =
            coordinateSystem.MapCoordinateToWorld(
                targetCoordinate,
                0f
            );

        XROrigin xrOrigin =
            Object.FindFirstObjectByType<XROrigin>();

        if (xrOrigin == null)
        {
            Debug.LogError(
                "[NEXELYTH World Scene Manager] XR Origin not found."
            );

            isTransitioning = false;
            yield break;
        }

        // 修改原因：跨 Scene 傳送時保留同一個 XR Origin，
        // 只修改玩家在新世界中的位置，不重新建立 VR 玩家。
        xrOrigin.transform.position =
            targetWorldPosition;

        string previousWorldSceneName =
            currentWorldSceneName;

        currentWorldSceneName =
            targetSceneName;

        // 修改原因：確認新世界完成載入、座標系建立、
        // 且玩家已移動到目標位置後，再卸載舊世界。
        if (!string.IsNullOrWhiteSpace(
                previousWorldSceneName
            ) &&
            previousWorldSceneName != targetSceneName)
        {
            Scene previousScene =
                SceneManager.GetSceneByName(
                    previousWorldSceneName
                );

            if (previousScene.IsValid() &&
                previousScene.isLoaded)
            {
                AsyncOperation unloadOperation =
                    SceneManager.UnloadSceneAsync(
                        previousWorldSceneName
                    );

                if (unloadOperation != null)
                    yield return unloadOperation;
            }
        }

        Debug.Log(
            $"[NEXELYTH World Scene Manager] " +
            $"World changed: {previousWorldSceneName} -> {targetSceneName}, " +
            $"Coordinate=({targetCoordinate.x}, {targetCoordinate.y}), " +
            $"World={targetWorldPosition}"
        );

        isTransitioning = false;
    }

    private NexelythMapCoordinateSystem EnsureMapCoordinateSystem(
        Scene worldScene
    )
    {
        // 修改原因：先搜尋目標 World Scene 是否已經存在 MapSystem，
        // 避免重複建立多套座標系。
        GameObject[] rootObjects =
            worldScene.GetRootGameObjects();

        foreach (GameObject rootObject in rootObjects)
        {
            NexelythMapCoordinateSystem existingSystem =
                rootObject.GetComponentInChildren<
                    NexelythMapCoordinateSystem
                >(true);

            if (existingSystem != null)
            {
                return existingSystem;
            }
        }

        // 修改原因：如果 World Scene 尚未配置 MapSystem，
        // Runtime 自動建立一個，MapId 預設直接使用 Scene 名稱。
        GameObject mapSystemObject =
            new GameObject(
                "MapSystem"
            );

        SceneManager.MoveGameObjectToScene(
            mapSystemObject,
            worldScene
        );

        NexelythMapCoordinateSystem coordinateSystem =
            mapSystemObject.AddComponent<
                NexelythMapCoordinateSystem
            >();

        coordinateSystem.Initialize(
            worldScene.name,
            1f,
            Vector3.zero
        );

        Debug.Log(
            $"[NEXELYTH Map System] Auto-created for scene: {worldScene.name}"
        );

        return coordinateSystem;
    }
}