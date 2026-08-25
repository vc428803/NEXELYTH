using UnityEngine;

//NexelythMemoLocationService
//→ 管理「哪一格、何時存、何時傳送」

//NexelythMemoSlot
//→ 管理「這一格有沒有資料、裡面存什麼」

public class NexelythMemoLocationService : MonoBehaviour
{
    public static NexelythMemoLocationService Instance { get; private set; }

    private const int MemoSlotCount = 3;

    // 修改原因：Memo 改為固定 3 個 Slot，
    // 每個 Slot 由 NexelythMemoSlot 自己管理是否已保存及位置資料。
    private readonly NexelythMemoSlot[] memoSlots =
    {
        new NexelythMemoSlot(),
        new NexelythMemoSlot(),
        new NexelythMemoSlot()
    };

    public int SlotCount =>
        MemoSlotCount;

    private void Awake()
    {
        // 修改原因：確保 Bootstrap 中只有一個 Memo Location Service，
        // 讓 Memo、傳送技能與未來 VR UI 共用同一組 Memo Slot。
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public NexelythMemoSlot GetMemoSlot(
        int slotIndex
    )
    {
        // 修改原因：統一提供外部系統讀取指定 Memo Slot，
        // 未來 VR UI 可以透過 IsSaved 與 Location 顯示 Slot 狀態。
        if (!IsValidSlotIndex(slotIndex))
        {
            return null;
        }

        return memoSlots[slotIndex];
    }

    public void SaveCurrentLocation(
        int slotIndex
    )
    {
        // 修改原因：避免存取不存在的 Memo Slot。
        if (!IsValidSlotIndex(slotIndex))
        {
            return;
        }

        if (NexelythPlayerLocationService.Instance == null)
        {
            Debug.LogError(
                "[NEXELYTH Memo] Player Location Service not found."
            );

            return;
        }

        // 修改原因：Memo 必須保存玩家按下存點當下的實際位置，
        // 因此向 Player Location Service 即時取得目前 Map + X/Y。
        NexelythWorldLocation currentLocation =
            NexelythPlayerLocationService.Instance
                .GetCurrentWorldLocation();

        if (currentLocation == null)
        {
            Debug.LogError(
                "[NEXELYTH Memo] Current player location is not available."
            );

            return;
        }

        // 修改原因：建立獨立的 Runtime Location，
        // 避免玩家後續移動或傳送影響已保存的 Memo 位置。
        NexelythWorldLocation savedLocation =
            new NexelythWorldLocation(
                currentLocation.MapId,
                currentLocation.X,
                currentLocation.Y
            );

        // 修改原因：實際的 Slot 內容由 NexelythMemoSlot 自己管理，
        // Service 只負責協調「目前位置」與「指定 Slot」。
        memoSlots[slotIndex].Save(
            savedLocation
        );

        Debug.Log(
            $"[NEXELYTH Memo] " +
            $"Slot={slotIndex}, " +
            $"Saved Map={savedLocation.MapId}, " +
            $"Coordinate=({savedLocation.X}, {savedLocation.Y})"
        );
    }

    public void TravelToMemoLocation(
        int slotIndex
    )
    {
        // 修改原因：避免存取不存在的 Memo Slot。
        if (!IsValidSlotIndex(slotIndex))
        {
            return;
        }

        NexelythMemoSlot memoSlot =
            memoSlots[slotIndex];

        // 修改原因：透過 Slot 自己的 IsSaved 判斷是否已有存點，
        // 避免 Service 直接依賴 location == null 的內部實作。
        if (!memoSlot.IsSaved)
        {
            Debug.LogError(
                $"[NEXELYTH Memo] Slot {slotIndex} has not been saved yet."
            );

            return;
        }

        IWorldTravelService worldTravelService =
            NexelythWorldTravel.Service;

        if (worldTravelService == null)
        {
            Debug.LogError(
                "[NEXELYTH Memo] World travel service is not available."
            );

            return;
        }

        // 修改原因：Memo 仍透過統一的 IWorldTravelService 執行傳送，
        // 不直接依賴 NexelythWorldSceneManager。
        worldTravelService.TravelTo(
            memoSlot.Location
        );
    }

    public void ClearMemoLocation(
        int slotIndex
    )
    {
        // 修改原因：避免存取不存在的 Memo Slot。
        if (!IsValidSlotIndex(slotIndex))
        {
            return;
        }

        // 修改原因：清除行為交由 NexelythMemoSlot 自己處理，
        // 未來 VR UI 或存檔系統可以共用相同入口。
        memoSlots[slotIndex].Clear();

        Debug.Log(
            $"[NEXELYTH Memo] Slot {slotIndex} cleared."
        );
    }

    private bool IsValidSlotIndex(
        int slotIndex
    )
    {
        // 修改原因：集中處理 Memo Slot 範圍驗證，
        // 避免 Save、Travel、Clear 重複相同邏輯。
        if (slotIndex < 0 ||
            slotIndex >= MemoSlotCount)
        {
            Debug.LogError(
                $"[NEXELYTH Memo] Invalid slot index: {slotIndex}"
            );

            return false;
        }

        return true;
    }

    // 修改原因：保留原本無參數版本，
    // 讓既有程式仍可使用，預設操作 Slot 0。
    public void SaveCurrentLocation()
    {
        SaveCurrentLocation(0);
    }

    // 修改原因：保留原本無參數版本，
    // 讓既有程式仍可使用，預設傳送至 Slot 0。
    public void TravelToMemoLocation()
    {
        TravelToMemoLocation(0);
    }

    [ContextMenu("Test Save Slot 0")]
    private void TestSaveSlot0()
    {
        // 修改原因：正式 VR UI 完成前，
        // 透過 Inspector 驗證 Slot 0 存點功能。
        SaveCurrentLocation(0);
    }

    [ContextMenu("Test Save Slot 1")]
    private void TestSaveSlot1()
    {
        // 修改原因：正式 VR UI 完成前，
        // 透過 Inspector 驗證 Slot 1 存點功能。
        SaveCurrentLocation(1);
    }

    [ContextMenu("Test Save Slot 2")]
    private void TestSaveSlot2()
    {
        // 修改原因：正式 VR UI 完成前，
        // 透過 Inspector 驗證 Slot 2 存點功能。
        SaveCurrentLocation(2);
    }

    [ContextMenu("Test Travel To Slot 0")]
    private void TestTravelToSlot0()
    {
        // 修改原因：正式 VR UI 完成前，
        // 驗證 Slot 0 傳送功能。
        TravelToMemoLocation(0);
    }

    [ContextMenu("Test Travel To Slot 1")]
    private void TestTravelToSlot1()
    {
        // 修改原因：正式 VR UI 完成前，
        // 驗證 Slot 1 傳送功能。
        TravelToMemoLocation(1);
    }

    [ContextMenu("Test Travel To Slot 2")]
    private void TestTravelToSlot2()
    {
        // 修改原因：正式 VR UI 完成前，
        // 驗證 Slot 2 傳送功能。
        TravelToMemoLocation(2);
    }

    [ContextMenu("Test Clear Slot 0")]
    private void TestClearSlot0()
    {
        // 修改原因：正式 VR UI 完成前，
        // 驗證 Slot 0 清除功能。
        ClearMemoLocation(0);
    }

    private void OnDestroy()
    {
        // 修改原因：只有目前有效的 Memo Service 被銷毀時才清除 Instance。
        if (Instance == this)
        {
            Instance = null;
        }
    }
}