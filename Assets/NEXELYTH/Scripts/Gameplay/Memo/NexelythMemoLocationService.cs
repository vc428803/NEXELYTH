using UnityEngine;

// NexelythMemoLocationService
// → 管理「哪一格、何時存、何時傳送」
//
// NexelythMemoSlot
// → 管理「這一格有沒有資料、裡面存什麼」
//
// NexelythMemoSaveData
// → 管理「單一 Slot 如何轉成可序列化的存檔資料」
//
// NexelythMemoSaveFileData
// → 管理「整組 Memo Slots 的存檔資料」

//NexelythMemoLocationService
//│
//├─ Runtime Memo 操作
//│   ├─ Save
//│   ├─ Travel
//│   └─ Clear
//│
//├─ Save / Load Conversion
//│   ├─ CreateSaveData
//│   └─ ApplySaveData
//│
//└─ Development Test Tools

//MonoBehaviour 建構
//→ 不碰 Unity API

//Awake()
//→ Unity 已準備好
//→ new NexelythMemoSaveService()
//→ 讀 Application.persistentDataPath

public class NexelythMemoLocationService : MonoBehaviour
{
    private const int MemoSlotCount = 3;

    public static NexelythMemoLocationService Instance { get; private set; }

    // Memo 使用固定 3 個 Slot，
    // 每個 Slot 由 NexelythMemoSlot 自己管理是否已保存及位置資料。
    private readonly NexelythMemoSlot[] memoSlots =
    {
        new NexelythMemoSlot(),
        new NexelythMemoSlot(),
        new NexelythMemoSlot()
    };

    // 開發階段暫時持有 Memo Save Service，
    // 用來驗證 JSON Save / Load 流程是否能正確保存與還原多個 Memo Slot。
    private NexelythMemoSaveService memoSaveService;

    public int SlotCount =>
        MemoSlotCount;


    // =========================================================
    // Unity Lifecycle
    // =========================================================

    private void Awake()
    {
        // 確保 Bootstrap 中只有一個 Memo Location Service，
        // 讓 Memo、傳送技能、存檔系統與未來 VR UI 共用同一組 Memo Slot。
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        //  Application.persistentDataPath 不能在 MonoBehaviour
        // 的 constructor / 欄位初始化階段呼叫，因此改在 Awake 建立 Save Service。
        memoSaveService =
            new NexelythMemoSaveService();
    }

    private void OnDestroy()
    {
        // 修改原因：只有目前有效的 Memo Service 被銷毀時才清除 Instance。
        if (Instance == this)
        {
            Instance = null;
        }
    }


    // =========================================================
    // Public Memo API
    // =========================================================

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

        // 修改原因：實際 Slot 內容由 NexelythMemoSlot 自己管理，
        // Service 只負責協調玩家目前位置與指定 Slot。
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
        // 避免 Service 直接依賴 Location == null 的內部實作。
        if (!memoSlot.IsSaved)
        {
            // 修改原因：尚未設定 Memo Slot 屬於正常遊戲狀態，
            // 不應視為程式錯誤，也不應觸發 Unity 的 Error Pause。
            Debug.LogWarning(
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

        // 修改原因：Memo 透過統一的 IWorldTravelService 執行傳送，
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
        // 未來 VR UI、存檔系統與其他 Gameplay 可以共用相同入口。
        memoSlots[slotIndex].Clear();

        Debug.Log(
            $"[NEXELYTH Memo] Slot {slotIndex} cleared."
        );
    }


    // =========================================================
    // Save / Load Data Conversion
    // =========================================================

    public NexelythMemoSaveFileData CreateSaveData()
    {
        // 修改原因：將目前 Runtime 中所有 Memo Slot
        // 一次轉換成可序列化的 Save File Data，供之後 JSON 存檔使用。
        NexelythMemoSaveFileData saveFileData =
            new NexelythMemoSaveFileData(
                MemoSlotCount
            );

        for (int i = 0;
             i < MemoSlotCount;
             i++)
        {
            saveFileData.slots[i].CopyFrom(
                memoSlots[i]
            );
        }

        return saveFileData;
    }

    public void ApplySaveData(
        NexelythMemoSaveFileData saveFileData
    )
    {
        // 修改原因：把讀檔取得的 Memo Save Data
        // 還原回 Runtime Memo Slot，形成完整的 Save → Runtime 流程。
        if (saveFileData == null ||
            saveFileData.slots == null)
        {
            Debug.LogError(
                "[NEXELYTH Memo] Save file data is invalid."
            );

            return;
        }

        int restoreCount =
            Mathf.Min(
                MemoSlotCount,
                saveFileData.slots.Length
            );

        for (int i = 0;
             i < restoreCount;
             i++)
        {
            NexelythMemoSaveData slotSaveData =
                saveFileData.slots[i];

            if (slotSaveData == null)
            {
                // 修改原因：若存檔中某個 Slot 資料不存在，
                // Runtime 對應 Slot 應清空，避免保留舊資料。
                memoSlots[i].Clear();
                continue;
            }

            slotSaveData.ApplyTo(
                memoSlots[i]
            );
        }

        // 修改原因：如果舊版本存檔 Slot 數量少於目前版本，
        // 新增的 Runtime Slot 統一保持空白，避免殘留不正確狀態。
        for (int i = restoreCount;
             i < MemoSlotCount;
             i++)
        {
            memoSlots[i].Clear();
        }

        Debug.Log(
            $"[NEXELYTH Memo] Restored {restoreCount} memo slot(s) from save data."
        );
    }


    // =========================================================
    // Compatibility API
    // =========================================================

    public void SaveCurrentLocation()
    {
        // 修改原因：保留原本無參數版本，
        // 讓既有程式仍可使用，預設操作 Slot 0。
        SaveCurrentLocation(0);
    }

    public void TravelToMemoLocation()
    {
        // 修改原因：保留原本無參數版本，
        // 讓既有程式仍可使用，預設傳送至 Slot 0。
        TravelToMemoLocation(0);
    }


    // =========================================================
    // Private Helpers
    // =========================================================

    private bool IsValidSlotIndex(
        int slotIndex
    )
    {
        // 修改原因：集中處理 Memo Slot 範圍驗證，
        // 避免 Save、Travel、Clear 等方法重複相同邏輯。
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


    // =========================================================
    // Development Test Tools
    // =========================================================

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

    [ContextMenu("Test Save Memo File")]
    private void TestSaveMemoFile()
    {
        // 修改原因：正式 Save UI 完成前，
        // 透過 Inspector 驗證目前所有 Memo Slot 是否能寫入 JSON 存檔。
        memoSaveService.Save(
            this
        );
    }

    [ContextMenu("Test Load Memo File")]
    private void TestLoadMemoFile()
    {
        // 修改原因：正式 Load UI 完成前，
        // 透過 Inspector 驗證 JSON 存檔是否能還原所有 Memo Slot。
        memoSaveService.Load(
            this
        );
    }

    [ContextMenu("Test Clear All Memo Slots")]
    private void TestClearAllMemoSlots()
    {
        // 修改原因：提供 Save / Load 測試用的清空功能，
        // 方便確認資料確實是從硬碟讀回，而不是 Runtime 舊資料仍然存在。
        for (int i = 0;
             i < MemoSlotCount;
             i++)
        {
            memoSlots[i].Clear();
        }

        Debug.Log(
            "[NEXELYTH Memo] All memo slots cleared."
        );
    }
}