//兩種目的地格式最後都走同一套傳送系統。
public interface IWorldTravelService
{
    // 修改原因：支援以 ScriptableObject 定義的固定世界地點，
    // 供 Portal、Fast Travel 等功能共用。
    void TravelTo(
        NexelythWorldLocationSO targetLocation
    );

    // 修改原因：支援 Runtime 產生的位置資料，
    // 讓 Memo、復活點或存檔位置可以直接使用玩家當下記錄的 Map + X/Y。
    void TravelTo(
        NexelythWorldLocation targetLocation
    );
}