public interface IWorldTravelService
{
    // 修改原因：統一所有世界傳送功能的呼叫入口，
    // 讓 Portal 未來不需要直接依賴具體的 NexelythWorldSceneManager。
    void TravelTo(
        NexelythWorldLocationSO targetLocation
    );
}