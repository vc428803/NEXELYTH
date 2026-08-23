using UnityEngine;

public class NexelythPortal : MonoBehaviour
{
    [SerializeField]
    private string targetSceneName;

    [SerializeField]
    private int targetX;

    [SerializeField]
    private int targetY;

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

        // 修改原因：Portal 不再只指定 Scene，
        // 同時保存目標地圖座標，作為未來 Memo、傳送技能與 Fast Travel 的共同格式。
        Vector2Int targetCoordinate =
            new Vector2Int(
                targetX,
                targetY
            );

        NexelythWorldSceneManager.Instance.ChangeWorldScene(
            targetSceneName,
            targetCoordinate
        );
    }
}