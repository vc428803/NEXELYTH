
using UnityEngine;
using UnityEngine.InputSystem;

public class NexelythSAOMenuController : MonoBehaviour
{
    [Header("SAO Menu")]
    [SerializeField]
    private GameObject menuCanvas;

    [Header("SAO Category Flyout")]
    [SerializeField]
    private GameObject categoryFlyout;

    private void Awake()
    {
        // 啟動時先隱藏分類清單。
        if (categoryFlyout != null)
        {
            categoryFlyout.SetActive(false);
        }

        // 啟動時隱藏整個玩家選單。
        if (menuCanvas != null)
        {
            menuCanvas.SetActive(false);
        }
    }



    private void Update()
    {
        if (Time.frameCount % 300 == 0)
        {
            Debug.Log("[NEXELYTH SAO] Update is running", this);
        }

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            return;
        }

        // M 鍵：開啟／關閉 SAO 選單
        if (keyboard.mKey.wasPressedThisFrame)
        {
            Debug.Log("[NEXELYTH SAO] M key detected!", this);
            ToggleMenu();
        }

        // F8 鍵：檢查滑鼠目前命中的 UI 物件
        if (keyboard.f8Key.wasPressedThisFrame)
        {
            Debug.Log("[NEXELYTH SAO] F8 UI Raycast Test", this);
            DebugUIRaycast();
        }
    }



    public void ToggleMenu()
    {
        if (menuCanvas == null)
        {
            Debug.LogWarning(
                "[NEXELYTH] SAO Menu Canvas 尚未指定。",
                this
            );
            return;
        }

        bool shouldOpen = !menuCanvas.activeSelf;

        // 每次開關主選單，都先收起第二層分類。
        if (categoryFlyout != null)
        {
            categoryFlyout.SetActive(false);
        }

        menuCanvas.SetActive(shouldOpen);
    }


    public void ToggleCategoryFlyout()
    {
        Debug.Log("[NEXELYTH SAO] Character button clicked!", this);

        if (categoryFlyout == null)
        {
            Debug.LogWarning(
                "[NEXELYTH] Category Flyout 尚未指定.",
                this
            );
            return;
        }

        categoryFlyout.SetActive(!categoryFlyout.activeSelf);
    }



    [ContextMenu("Debug UI Raycast At Mouse")]
    private void DebugUIRaycast()
    {
        var eventSystem = UnityEngine.EventSystems.EventSystem.current;

        if (eventSystem == null ||
            UnityEngine.InputSystem.Mouse.current == null)
        {
            Debug.LogWarning("[NEXELYTH SAO] EventSystem or Mouse missing");
            return;
        }

        var eventData = new UnityEngine.EventSystems.PointerEventData(eventSystem);
        eventData.position = UnityEngine.InputSystem.Mouse.current.position.ReadValue();

        var results = new System.Collections.Generic.List<
            UnityEngine.EventSystems.RaycastResult>();

        eventSystem.RaycastAll(eventData, results);

        Debug.Log($"[NEXELYTH SAO] Raycast hits: {results.Count}");

        foreach (var hit in results)
        {
            Debug.Log($"[NEXELYTH SAO] Hit: {hit.gameObject.name}");
        }
    }


}
