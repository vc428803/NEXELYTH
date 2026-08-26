using System;

//NexelythMemoSaveData

//變成：

//NexelythMemoSaveFileData
//├─ Slot 0 SaveData
//├─ Slot 1 SaveData
//└─ Slot 2 SaveData

[Serializable]
public class NexelythMemoSaveFileData
{
    public NexelythMemoSaveData[] slots;

    public NexelythMemoSaveFileData(
        int slotCount
    )
    {
        // 修改原因：依照 Memo 系統目前的 Slot 數量建立存檔容器，
        // 讓 Save File 可以一次保存完整的一組 Memo Slot。
        slots =
            new NexelythMemoSaveData[slotCount];

        for (int i = 0; i < slotCount; i++)
        {
            slots[i] =
                new NexelythMemoSaveData();
        }
    }
}