using UnityEngine;

public class NexelythPortal : MonoBehaviour
{
    [SerializeField]
    private NexelythWorldLocationSO targetLocation;

    private void OnTriggerEnter(Collider other)
    {
        // 修改原因：目前以 XR Origin 上的 CharacterController 作為玩家本體，
        // 只有玩家進入 Portal Trigger 時才執行世界 Scene 切換。
        CharacterController characterController =
            other.GetComponent<CharacterController>();

        if (characterController == null)
            return;


        // 修改原因：避免 Portal 尚未指定 World Location Asset 時執行傳送。
        if (targetLocation == null)
        {
            Debug.LogError(
                "[NEXELYTH Portal] Target location is not configured."
            );

            return;
        }

        // 修改原因：Portal 改為只依賴統一的世界傳送服務入口，
        // 不再直接依賴 NexelythWorldSceneManager 具體類別。
        //Portal 概念會變成
        //→ NexelythWorldTravel.Service
        //→ IWorldTravelService
        //→ NexelythWorldSceneManager
        //這一步做完之後，Portal 本身就完全不知道 NexelythWorldSceneManager 是誰了。
        IWorldTravelService worldTravelService =
            NexelythWorldTravel.Service;

        if (worldTravelService == null)
        {
            Debug.LogError(
                "[NEXELYTH Portal] World travel service is not available."
            );

            return;
        }

        worldTravelService.TravelTo(
            targetLocation
        );
    }
}