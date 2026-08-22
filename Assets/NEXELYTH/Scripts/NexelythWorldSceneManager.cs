using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NexelythWorldSceneManager : MonoBehaviour
{
    public static NexelythWorldSceneManager Instance { get; private set; }

    [SerializeField]
    private string currentWorldSceneName = "Aurevane";

    private bool isTransitioning;

    private void Awake()
    {
        // 修改原因：確保整個 Bootstrap 只存在一個 World Scene Manager，
        // 讓所有 Portal 都可以透過同一個管理器切換世界 Scene。
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void ChangeWorldScene(string targetSceneName)
    {
        // 修改原因：避免玩家連續觸發 Portal 時重複執行 Scene 切換。
        if (isTransitioning)
            return;

        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            Debug.LogError(
                "[NEXELYTH World Scene Manager] Target scene name is empty."
            );
            return;
        }

        if (targetSceneName == currentWorldSceneName)
            return;

        StartCoroutine(
            ChangeWorldSceneRoutine(targetSceneName)
        );
    }

    private IEnumerator ChangeWorldSceneRoutine(
        string targetSceneName
    )
    {
        isTransitioning = true;

        // 修改原因：先載入目標世界 Scene，
        // Bootstrap 與 XR Origin 因為位於另一張 Scene，所以不會被卸載。
        AsyncOperation loadOperation =
            SceneManager.LoadSceneAsync(
                targetSceneName,
                LoadSceneMode.Additive
            );

        if (loadOperation == null)
        {
            Debug.LogError(
                $"[NEXELYTH World Scene Manager] Failed to start loading scene: {targetSceneName}"
            );

            isTransitioning = false;
            yield break;
        }

        yield return loadOperation;

        string previousWorldSceneName =
            currentWorldSceneName;

        currentWorldSceneName =
            targetSceneName;

        // 修改原因：確認新世界完成載入後，
        // 才卸載舊世界，避免切換期間出現完全沒有 World Scene 的狀態。
        if (!string.IsNullOrWhiteSpace(previousWorldSceneName))
        {
            Scene previousScene =
                SceneManager.GetSceneByName(
                    previousWorldSceneName
                );

            if (previousScene.IsValid() &&
                previousScene.isLoaded)
            {
                AsyncOperation unloadOperation =
                    SceneManager.UnloadSceneAsync(
                        previousWorldSceneName
                    );

                if (unloadOperation != null)
                    yield return unloadOperation;
            }
        }

        Debug.Log(
            $"[NEXELYTH World Scene Manager] " +
            $"World changed: {previousWorldSceneName} -> {targetSceneName}"
        );

        isTransitioning = false;
    }
}