using System;

//Runtime → Save

//NexelythMemoSlot
//        ↓
//CopyFrom(...)
//        ↓
//NexelythMemoSaveData

//以及：

//Save → Runtime

//NexelythMemoSaveData
//        ↓
//ApplyTo(...)
//        ↓
//NexelythMemoSlot

[Serializable]
public class NexelythMemoSaveData
{
    public bool isSaved;
    public string mapId;
    public int x;
    public int y;

    public NexelythMemoSaveData()
    {
        // 修改原因：提供空白建構子，
        // 方便未來 JSON 反序列化時建立 SaveData 物件。
        Clear();
    }

    public NexelythMemoSaveData(
        NexelythMemoSlot slot
    )
    {
        // 修改原因：建立 SaveData 時直接從 Runtime Memo Slot 匯出資料，
        // 形成 NexelythMemoSlot → NexelythMemoSaveData 的存檔轉換入口。
        CopyFrom(
            slot
        );
    }

    public void CopyFrom(
        NexelythMemoSlot slot
    )
    {
        // 修改原因：集中處理 Runtime Slot → SaveData 的轉換，
        // 避免未來不同 Save 系統重複撰寫相同資料複製邏輯。
        if (slot == null ||
            !slot.IsSaved ||
            slot.Location == null)
        {
            Clear();
            return;
        }

        isSaved = true;
        mapId = slot.Location.MapId;
        x = slot.Location.X;
        y = slot.Location.Y;
    }

    public void ApplyTo(
        NexelythMemoSlot slot
    )
    {
        // 修改原因：提供 SaveData → Runtime Slot 的反向轉換，
        // 讓遊戲讀檔後可以完整還原 Memo Slot 狀態。
        if (slot == null)
        {
            return;
        }

        if (!isSaved)
        {
            slot.Clear();
            return;
        }

        // 修改原因：只有 isSaved 為 true 時，
        // MapId 與 X/Y 才被視為有效的世界位置資料。
        slot.Save(
            new NexelythWorldLocation(
                mapId,
                x,
                y
            )
        );
    }

    public void Clear()
    {
        // 修改原因：未保存的 Slot 統一清空其他位置欄位，
        // 避免舊資料殘留造成 Debug、JSON 或後續判斷上的混淆。
        isSaved = false;
        mapId = string.Empty;
        x = 0;
        y = 0;
    }
}