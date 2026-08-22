using UnityEngine;

public class NexelythPortal : MonoBehaviour
{
    [SerializeField]
    private string targetSceneName;

    private void OnTriggerEnter(Collider other)
    {
        // 修改原因：目前以 XR Origin 上的 CharacterController 作為玩家本體，
        // 只有玩家進入 Portal Trigger 時才執行世界 Scene 切換。
        CharacterController characterController =
            other.GetComponent<CharacterController>();

        if (characterController == null)
            return;

        // 修改原因：所有 World Scene 切換統一交由 Bootstrap 內的
        // NexelythWorldSceneManager 管理，Portal 本身只負責指定目的地。
        if (NexelythWorldSceneManager.Instance == null)
        {
            Debug.LogError(
                "[NEXELYTH Portal] World Scene Manager not found."
            );
            return;
        }

        NexelythWorldSceneManager.Instance.ChangeWorldScene(
            targetSceneName
        );
    }
}