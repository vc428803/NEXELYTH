using UnityEngine;
using UnityEngine.SceneManagement;

public class NexelythBootstrapLoader : MonoBehaviour
{
    [SerializeField]
    // 修改原因：正式啟動流程預設載入 NEXELYTH 主城 Aurevane，
    // NexelythWorldTest 保留為純測試 Scene，不再作為正式世界入口。
    private string worldSceneName = "Aurevane";

    private void Start()
    {
        // 修改原因：Bootstrap Scene 只負責長駐 VR 系統，
        // 啟動後以 Additive 方式載入獨立的 World Scene，
        // 避免切換地圖時重新建立 XR Origin。
        if (!SceneManager.GetSceneByName(worldSceneName).isLoaded)
        {
            SceneManager.LoadSceneAsync(
                worldSceneName,
                LoadSceneMode.Additive
            );
        }
    }
}