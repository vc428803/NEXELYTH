using TMPro;
using UnityEngine;

public class NexelythMemoUIController : MonoBehaviour
{
    [SerializeField]
    private Transform playerCamera;

    [SerializeField]
    private float distanceFromPlayer = 0.8f;

    [SerializeField]
    private bool followPlayer = false;

    [SerializeField]
    private GameObject[] memoSlotButtons;

    [SerializeField]
    private TMP_Text[] memoSlotLabels;

    private int lastAvailableSlotCount = -1;

    private void Start()
    {
        // 修改原因：Memo UI 啟動時先依照玩家目前的 MEMO 技能等級，
        // 更新實際可以使用的 Slot Button 數量。
        RefreshAvailableSlotButtons();

        // 修改原因：Memo UI 啟動時同步顯示目前已保存的 Memo 地點，
        // 讓玩家可以直接辨識每一個傳送位置。
        RefreshMemoSlotLabels();
    }

    private void Update()
    {
        if (NexelythMemoLocationService.Instance == null)
        {
            return;
        }

        int availableSlotCount =
            NexelythMemoLocationService.Instance
                .AvailableSlotCount;

        // 修改原因：只有玩家的 Memo 可用 Slot 數量真的改變時，
        // 才重新整理 Button 顯示狀態。
        if (availableSlotCount !=
            lastAvailableSlotCount)
        {
            RefreshAvailableSlotButtons();
        }

        // 修改原因：目前 Memo Slot 可能在遊戲進行中被新增或 Rolling 覆蓋，
        // 因此同步檢查顯示文字，讓 UI 能立即反映最新存點。
        RefreshMemoSlotLabels();
    }

    private void LateUpdate()
    {
        if (!followPlayer)
        {
            return;
        }

        FollowPlayer();
    }

    private void FollowPlayer()
    {
        if (playerCamera == null)
        {
            // 修改原因：Memo UI 必須以玩家目前 Camera
            // 作為位置與朝向計算基準。
            return;
        }

        // 修改原因：Memo UI 開啟期間持續保持在玩家視線前方，
        // 讓玩家移動或轉向時都能繼續看到面板。
        Vector3 targetPosition =
            playerCamera.position +
            playerCamera.forward *
            distanceFromPlayer;

        transform.position =
            targetPosition;

        // 修改原因：Memo UI 必須持續面向玩家，
        // 避免玩家移動或轉身後看到面板側面或背面。
        Vector3 lookDirection =
            transform.position -
            playerCamera.position;

        transform.rotation =
            Quaternion.LookRotation(
                lookDirection
            );
    }

    private void RefreshAvailableSlotButtons()
    {
        if (NexelythMemoLocationService.Instance == null)
        {
            return;
        }

        int availableSlotCount =
            NexelythMemoLocationService.Instance
                .AvailableSlotCount;

        // 修改原因：Memo UI 的 Slot Button 數量必須反映玩家目前
        // MEMO 技能等級，避免顯示玩家實際上不能使用的傳送位置。
        for (int i = 0;
             i < memoSlotButtons.Length;
             i++)
        {
            if (memoSlotButtons[i] == null)
            {
                continue;
            }

            memoSlotButtons[i].SetActive(
                i < availableSlotCount
            );
        }

        lastAvailableSlotCount =
            availableSlotCount;
    }

    private void RefreshMemoSlotLabels()
    {
        if (NexelythMemoLocationService.Instance == null)
        {
            return;
        }

        for (int i = 0;
             i < memoSlotLabels.Length;
             i++)
        {
            if (memoSlotLabels[i] == null)
            {
                continue;
            }

            NexelythMemoSlot memoSlot =
                NexelythMemoLocationService.Instance
                    .GetMemoSlot(i);

            string displayText =
                "Empty";

            // 修改原因：已保存的 Memo Slot 顯示實際 MapId 與 X/Y，
            // 方便玩家確認 Rolling 覆蓋後的存點是否真的改變。
            if (memoSlot != null &&
                memoSlot.IsSaved &&
                memoSlot.Location != null)
            {
                displayText =
                    $"{memoSlot.Location.MapId} " +
                    $"({memoSlot.Location.X}, {memoSlot.Location.Y})";
            }

            // 修改原因：只有顯示內容真的改變時才更新 TMP 文字，
            // 避免每一幀重複寫入完全相同的 UI 內容。
            if (memoSlotLabels[i].text !=
                displayText)
            {
                memoSlotLabels[i].text =
                    displayText;
            }
        }
    }

    public void OpenMemoUI()
    {
        // 修改原因：開啟 Memo UI 時啟用玩家跟隨，
        // 並重新確認目前技能等級允許顯示多少個 Slot。
        followPlayer = true;

        RefreshAvailableSlotButtons();

        // 修改原因：每次開啟 Memo UI 時重新整理存點文字，
        // 確保玩家看到的是目前最新的 Memo 紀錄。
        RefreshMemoSlotLabels();

        gameObject.SetActive(true);
    }

    public void CloseMemoUI()
    {
        // 修改原因：關閉 Memo UI 時停止跟隨玩家，
        // 避免隱藏中的介面持續執行每幀更新。
        followPlayer = false;

        gameObject.SetActive(false);
    }
}