using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.XR.CoreUtils;
//Aurevane → Aurevane
//Memo / 城內傳送
//→ 不 Load Scene
//→ 直接移動 XR Origin

//Aurevane → SylvarisFields
//跨地圖
//→ Load SylvarisFields
//→ 移動 XR Origin
//→ Unload Aurevane
public class NexelythWorldSceneManager :
    MonoBehaviour,
    IWorldTravelService
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

        // 修改原因：將目前的 World Scene Manager 註冊成世界傳送服務，
        // 讓 Portal 未來只依賴 IWorldTravelService，而不依賴具體 Manager。
        NexelythWorldTravel.Register(
            this
        );
    }
    private void Start()
    {
        // 修改原因：BootstrapLoader 會先載入初始 World Scene，
        // 啟動後也要替目前已載入的世界建立統一 MapSystem。
        StartCoroutine(
            InitializeCurrentWorldSceneRoutine()
        );
    }

    private void OnDestroy()
    {
        // 修改原因：Manager 被銷毀時解除世界傳送服務註冊，
        // 避免全域入口保留已失效的服務引用。
        NexelythWorldTravel.Unregister(
            this
        );

        // 修改原因：只有目前這個 Singleton 本身被銷毀時才清除 Instance，
        // 避免重複物件誤清除有效的 Manager。
        if (Instance == this)
        {
            Instance = null;
        }
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

        // 修改原因：取得初始 World Scene 的座標系，
        // 讓玩家尚未經過任何 Portal 前也能建立目前位置資料。
        NexelythMapCoordinateSystem coordinateSystem =
            EnsureMapCoordinateSystem(
                currentScene
            );

        // 修改原因：取得 Bootstrap 中持續存在的 XR Origin，
        // 用目前玩家的 Unity 世界位置換算初始 Map X/Y。
        XROrigin xrOrigin =
            Object.FindFirstObjectByType<XROrigin>();

        if (xrOrigin != null &&
            coordinateSystem != null &&
            NexelythPlayerLocationService.Instance != null)
        {
            Vector2Int currentCoordinate =
                coordinateSystem.WorldToMapCoordinate(
                    xrOrigin.transform.position
                );

            // 修改原因：遊戲一開始就記錄玩家目前所在的 Map + X/Y，
            // 避免 Memo、存檔或復活系統在第一次傳送前讀不到位置。
            NexelythPlayerLocationService.Instance.SetCurrentLocation(
                new NexelythWorldLocation(
                    currentScene.name,
                    currentCoordinate.x,
                    currentCoordinate.y
                )
            );

            // 修改原因：暫時輸出初始位置，
            // 驗證 Player Location Service 在第一次 Portal 傳送前已經有資料。
            Debug.Log(
                $"[NEXELYTH Player Location Service] " +
                $"Initial Map={currentScene.name}, " +
                $"Coordinate=({currentCoordinate.x}, {currentCoordinate.y})"
            );
        }
    }

    //中階寫法（透過介面）：IWorldTravelService worldTravelService = NexelythWorldSceneManager.Instance;
    //worldTravelService.TravelTo(targetLocation);
    public void TravelTo(
        NexelythWorldLocationSO targetLocation
    )
    {
        // 修改原因：提供統一的世界傳送介面，
        // 讓 Portal 與其他系統不需要直接依賴 Scene Manager 的具體傳送參數。
        if (targetLocation == null)
        {
            Debug.LogError(
                "[NEXELYTH World Scene Manager] Target location is null."
            );

            return;
        }

        ChangeWorldScene(
            targetLocation.MapId,
            targetLocation.Coordinate
        );
    }

    public void TravelTo(
    NexelythWorldLocation targetLocation
)
    {
        // 修改原因：支援 Runtime 產生的世界位置資料，
        // 讓 Memo、復活點與存檔位置可以直接共用同一套世界傳送流程。
        if (targetLocation == null)
        {
            Debug.LogError(
                "[NEXELYTH World Scene Manager] Runtime target location is null."
            );

            return;
        }

        ChangeWorldScene(
            targetLocation.MapId,
            targetLocation.Coordinate
        );
    }

    //初階寫法（直接呼叫） NexelythWorldSceneManager.Instance.ChangeWorldScene
    public void ChangeWorldScene(
        string targetSceneName,
        Vector2Int targetCoordinate
    )
    {
        // 避免玩家連續觸發 Portal 或 Memo 時，
        // 同一時間執行多次世界傳送。
        if (isTransitioning)
            return;

        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            Debug.LogError(
                "[NEXELYTH World Scene Manager] Target scene name is empty."
            );

            return;
        }

        // 如果目的地仍然位於目前 World Scene，
        // 不需要重新 Load / Unload Scene，只需移動玩家到指定 X/Y。
        if (targetSceneName == currentWorldSceneName)
        {
            MovePlayerWithinCurrentWorld(
                targetCoordinate
            );

            return;
        }

        //  只有跨 World Scene 傳送時，
        // 才執行完整的 Additive Load / Unload 流程。
        StartCoroutine(
            ChangeWorldSceneRoutine(
                targetSceneName,
                targetCoordinate
            )
        );
    }

    private void MovePlayerWithinCurrentWorld(
        Vector2Int targetCoordinate
    )
    {
        Scene currentScene =
            SceneManager.GetSceneByName(
                currentWorldSceneName
            );

        if (!currentScene.IsValid() ||
            !currentScene.isLoaded)
        {
            Debug.LogError(
                $"[NEXELYTH World Scene Manager] Current scene is invalid: {currentWorldSceneName}"
            );

            return;
        }

        //  同 Scene 傳送仍然使用該地圖既有的 Map Coordinate System，
        // 確保 Memo、Fast Travel 與 Portal 使用相同的 Map X/Y 規則。
        NexelythMapCoordinateSystem coordinateSystem =
            EnsureMapCoordinateSystem(
                currentScene
            );

        if (coordinateSystem == null)
        {
            Debug.LogError(
                $"[NEXELYTH World Scene Manager] Map Coordinate System not found: {currentWorldSceneName}"
            );

            return;
        }

        XROrigin xrOrigin =
            Object.FindFirstObjectByType<XROrigin>();

        if (xrOrigin == null)
        {
            Debug.LogError(
                "[NEXELYTH World Scene Manager] XR Origin not found."
            );

            return;
        }

        Vector3 targetWorldPosition =
            coordinateSystem.MapCoordinateToWorld(
                targetCoordinate,
                0f
            );

        //  同一張 World Scene 內傳送時直接移動長駐 XR Origin，
        // 避免重新載入整張地圖造成不必要的 Scene IO 與物件初始化。
        xrOrigin.transform.position =
            targetWorldPosition;

        //  同 Scene 傳送後也必須同步更新玩家目前 Map + X/Y，
        // 讓 Memo、Save、Respawn 等系統取得正確位置。
        if (NexelythPlayerLocationService.Instance != null)
        {
            NexelythPlayerLocationService.Instance.SetCurrentLocation(
                new NexelythWorldLocation(
                    currentWorldSceneName,
                    targetCoordinate.x,
                    targetCoordinate.y
                )
            );
        }

        Debug.Log(
            $"[NEXELYTH World Scene Manager] " +
            $"Moved within world: {currentWorldSceneName}, " +
            $"Coordinate=({targetCoordinate.x}, {targetCoordinate.y}), " +
            $"World={targetWorldPosition}"
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

        // 修改原因：傳送完成後同步更新玩家目前所在的 Map + X/Y，
        // 讓 Memo、復活點、Fast Travel 與存檔系統可以共用同一份位置資料。
        if (NexelythPlayerLocationService.Instance != null)
        {
            NexelythPlayerLocationService.Instance.SetCurrentLocation(
                new NexelythWorldLocation(
                    targetSceneName,
                    targetCoordinate.x,
                    targetCoordinate.y
                )
            );

            // 修改原因：暫時輸出玩家位置服務目前保存的 Map + X/Y，
            // 用來驗證跨 Scene 傳送後 CurrentLocation 是否同步更新成功。
            NexelythWorldLocation currentLocation =
                NexelythPlayerLocationService.Instance.CurrentLocation;

            if (currentLocation != null)
            {
                Debug.Log(
                    $"[NEXELYTH Player Location Service] " +
                    $"Map={currentLocation.MapId}, " +
                    $"Coordinate=({currentLocation.X}, {currentLocation.Y})"
                );
            }
        }

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