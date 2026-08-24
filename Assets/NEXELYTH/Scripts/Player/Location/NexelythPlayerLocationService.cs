using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.SceneManagement;

//在的架構其實明確偏單機／單 Client：
//NexelythPlayerLocationService.Instance
//→ 只有一份

//xrOrigin
//→ 只快取一個

//currentLocation
//→ 只保存一位玩家的位置

// 單一資料來源（Single Source of Truth）：玩家目前的地圖與座標只存一份。
// 供多系統共用：存檔系統可以讀 CurrentLocation 來記錄玩家位置；
// 復活系統可以讀它來決定重生點；Memo / Quest 等系統也可以共用。
// 生命週期安全：透過 Awake / OnDestroy 確保單例的建立與清除正確。
public class NexelythPlayerLocationService : MonoBehaviour
{
    public static NexelythPlayerLocationService Instance { get; private set; }

    private NexelythWorldLocation currentLocation;

    // 修改原因：XR Origin 位於長駐 Bootstrap，
    // 因此取得一次後直接快取，避免每次查詢玩家位置都重新搜尋 Hierarchy。
    private XROrigin xrOrigin;

    public NexelythWorldLocation CurrentLocation =>
        currentLocation;

    private void Awake()
    {
        // 修改原因：確保 Bootstrap 中只有一個玩家位置服務，
        // 讓 Memo、存檔、復活點等系統共用同一份目前位置資料。
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // 修改原因：XR Origin 在 Bootstrap 中會長駐，
        // 啟動時先取得並快取引用，避免日後每次查詢位置都重新搜尋場景。
        xrOrigin =
            Object.FindFirstObjectByType<XROrigin>();

        if (xrOrigin == null)
        {
            Debug.LogError(
                "[NEXELYTH Player Location] XR Origin not found during initialization."
            );
        }
    }

    public void SetCurrentLocation(
        NexelythWorldLocation location
    )
    {
        // 修改原因：集中更新玩家目前所在的 Map + X/Y，
        // 避免未來不同系統各自保存一份位置資料造成不同步。
        currentLocation = location;
    }

    public NexelythWorldLocation GetCurrentWorldLocation()
    {
        // 修改原因：如果初始化時 XR Origin 尚未準備完成，
        // 第一次真正需要玩家位置時再嘗試取得一次，之後仍然使用快取引用。
        if (xrOrigin == null)
        {
            xrOrigin =
                Object.FindFirstObjectByType<XROrigin>();
        }

        if (xrOrigin == null)
        {
            Debug.LogError(
                "[NEXELYTH Player Location] XR Origin not found."
            );

            return currentLocation;
        }

        // 修改原因：目前 MapId 由 Player Location Service 統一保存，
        // 若尚未初始化位置，無法判斷應使用哪張 World Scene 的座標系。
        if (currentLocation == null ||
            string.IsNullOrWhiteSpace(currentLocation.MapId))
        {
            Debug.LogError(
                "[NEXELYTH Player Location] Current map is not available."
            );

            return currentLocation;
        }

        Scene currentScene =
            SceneManager.GetSceneByName(
                currentLocation.MapId
            );

        if (!currentScene.IsValid() ||
            !currentScene.isLoaded)
        {
            Debug.LogError(
                $"[NEXELYTH Player Location] Current scene is not loaded: {currentLocation.MapId}"
            );

            return currentLocation;
        }

        NexelythMapCoordinateSystem coordinateSystem = null;

        GameObject[] rootObjects =
            currentScene.GetRootGameObjects();

        foreach (GameObject rootObject in rootObjects)
        {
            coordinateSystem =
                rootObject.GetComponentInChildren<
                    NexelythMapCoordinateSystem
                >(true);

            if (coordinateSystem != null)
                break;
        }

        if (coordinateSystem == null)
        {
            Debug.LogError(
                $"[NEXELYTH Player Location] Map Coordinate System not found: {currentLocation.MapId}"
            );

            return currentLocation;
        }

        Vector2Int currentCoordinate =
            coordinateSystem.WorldToMapCoordinate(
                xrOrigin.transform.position
            );

        // 修改原因：將即時計算出的 Map + X/Y 回寫成目前位置，
        // 讓 Memo、Save、Respawn 等系統取得一致的最新資料。
        currentLocation =
            new NexelythWorldLocation(
                currentLocation.MapId,
                currentCoordinate.x,
                currentCoordinate.y
            );

        return currentLocation;
    }

    private void OnDestroy()
    {
        // 修改原因：只有目前有效的服務被銷毀時才清除 Instance。
        if (Instance == this)
        {
            Instance = null;
        }
    }
}