using System.IO;
using UnityEngine;

//NexelythMemoLocationService
//→ 管 Memo Gameplay

//NexelythMemoSaveData / SaveFileData
//→ 管可序列化資料

//NexelythMemoSaveService
//→ 管 JSON + File IO

public class NexelythMemoSaveService
{
    private readonly string saveFilePath;

    public NexelythMemoSaveService()
    {
        // 修改原因：使用 Unity 的 persistentDataPath，
        // 讓不同平台都能取得適合寫入玩家存檔的位置。
        saveFilePath =
            Path.Combine(
                Application.persistentDataPath,
                "memo_save.json"
            );
    }

    public void Save(
        NexelythMemoLocationService memoService
    )
    {
        // 修改原因：Save Service 只負責檔案序列化與 IO，
        // Memo Gameplay 本身仍由 NexelythMemoLocationService 管理。
        if (memoService == null)
        {
            Debug.LogError(
                "[NEXELYTH Save] Memo service is null."
            );

            return;
        }

        NexelythMemoSaveFileData saveData =
            memoService.CreateSaveData();

        string json =
            JsonUtility.ToJson(
                saveData,
                true
            );

        File.WriteAllText(
            saveFilePath,
            json
        );

        Debug.Log(
            $"[NEXELYTH Save] Memo data saved: {saveFilePath}"
        );
    }

    public bool Load(
        NexelythMemoLocationService memoService
    )
    {
        // 修改原因：讀檔前先確認目標 Memo Service 與存檔是否存在，
        // 避免沒有存檔時產生例外。
        if (memoService == null)
        {
            Debug.LogError(
                "[NEXELYTH Save] Memo service is null."
            );

            return false;
        }

        if (!File.Exists(saveFilePath))
        {
            Debug.Log(
                "[NEXELYTH Save] Memo save file does not exist."
            );

            return false;
        }

        string json =
            File.ReadAllText(
                saveFilePath
            );

        NexelythMemoSaveFileData saveData =
            JsonUtility.FromJson<
                NexelythMemoSaveFileData
            >(json);

        if (saveData == null)
        {
            Debug.LogError(
                "[NEXELYTH Save] Failed to deserialize memo save data."
            );

            return false;
        }

        memoService.ApplySaveData(
            saveData
        );

        Debug.Log(
            $"[NEXELYTH Save] Memo data loaded: {saveFilePath}"
        );

        return true;
    }
}