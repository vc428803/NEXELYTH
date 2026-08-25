using System;

[Serializable]
public class NexelythMemoSlot
{
    private NexelythWorldLocation location;

    public bool IsSaved =>
        location != null;

    public NexelythWorldLocation Location =>
        location;

    public void Save(
        NexelythWorldLocation newLocation
    )
    {
        // 修改原因：由 Memo Slot 自己負責保存位置資料，
        // 避免 Memo Service 直接操作 Slot 內部狀態。
        location = newLocation;
    }

    public void Clear()
    {
        // 修改原因：統一提供清除 Memo Slot 的方法，
        // 方便未來 VR UI、覆蓋確認與 Save / Load 系統共用。
        location = null;
    }
}