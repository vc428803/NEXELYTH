using UnityEngine;

public class NexelythMemoLocationService : MonoBehaviour
{
    public static NexelythMemoLocationService Instance { get; private set; }

    private NexelythWorldLocation memoLocation;

    public NexelythWorldLocation MemoLocation =>
        memoLocation;

    private void Awake()
    {
        // 修改原因：確保 Bootstrap 中只有一個 Memo Location Service，
        // 讓未來 Memo、傳送技能與 UI 共用同一份記錄位置。
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SaveCurrentLocation()
    {
        // 修改原因：Memo 直接從 Player Location Service 取得目前位置，
        // 避免 Memo 系統自己重新計算 Map 與 X/Y。
        if (NexelythPlayerLocationService.Instance == null)
        {
            Debug.LogError(
                "[NEXELYTH Memo] Player Location Service not found."
            );

            return;
        }

        // 修改原因：Memo 必須保存玩家按下存點當下的實際位置，
        // 因此改為即時計算 XR Origin 所在的 Map + X/Y，而不是使用上一次傳送留下的座標。
        NexelythWorldLocation currentLocation =
            NexelythPlayerLocationService.Instance.GetCurrentWorldLocation();

        if (currentLocation == null)
        {
            Debug.LogError(
                "[NEXELYTH Memo] Current player location is not available."
            );

            return;
        }

        // 修改原因：複製目前 Map + X/Y 作為 Memo 記錄，
        // 讓之後玩家位置改變時不會影響已保存的 Memo。
        memoLocation =
            new NexelythWorldLocation(
                currentLocation.MapId,
                currentLocation.X,
                currentLocation.Y
            );

        Debug.Log(
            $"[NEXELYTH Memo] Saved " +
            $"Map={memoLocation.MapId}, " +
            $"Coordinate=({memoLocation.X}, {memoLocation.Y})"
        );
    }

    [ContextMenu("Test Save Current Location")]
    private void TestSaveCurrentLocation()
    {
        // 修改原因：提供 Unity Inspector 的暫時測試入口，
        // 方便在尚未製作 Memo UI 前驗證目前位置是否能成功保存。
        SaveCurrentLocation();
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