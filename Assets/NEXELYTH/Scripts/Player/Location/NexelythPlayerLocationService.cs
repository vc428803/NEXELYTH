using UnityEngine;


//單一資料來源（Single Source of Truth）：玩家目前的地圖與座標只存一份。
//供多系統共用：存檔系統可以讀 CurrentLocation 來記錄玩家位置；復活系統可以讀它來決定重生點；備忘錄/任務系統可能用它來判斷觸發條件等。
//生命週期安全：透過 Awake/OnDestroy 確保單例的建立與清除都正確，不會有殘留的錯誤引用。
public class NexelythPlayerLocationService : MonoBehaviour
{
    public static NexelythPlayerLocationService Instance { get; private set; }

    private NexelythWorldLocation currentLocation;

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
    }

    public void SetCurrentLocation(
        NexelythWorldLocation location
    )
    {
        // 修改原因：集中更新玩家目前所在的 Map + X/Y，
        // 避免未來不同系統各自保存一份位置資料造成不同步。
        currentLocation = location;
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