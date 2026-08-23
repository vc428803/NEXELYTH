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

        // 修改原因：所有世界傳送統一交由 Bootstrap 內的
        // NexelythWorldSceneManager 管理，Portal 本身只保存目的地資料。
        if (NexelythWorldSceneManager.Instance == null)
        {
            Debug.LogError(
                "[NEXELYTH Portal] World Scene Manager not found."
            );

            return;
        }

        // 修改原因：避免 Portal 尚未指定 World Location Asset 時執行傳送。
        if (targetLocation == null)
        {
            Debug.LogError(
                "[NEXELYTH Portal] Target location is not configured."
            );

            return;
        }

        // 修改原因：Portal 改為引用可共用的 World Location ScriptableObject，
        // 讓多個 Portal、Quest 或傳送功能可以共用同一個目的地設定。
        NexelythWorldSceneManager.Instance.ChangeWorldScene(
            targetLocation.MapId,
            targetLocation.Coordinate
        );
    }
}