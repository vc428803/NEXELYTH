
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class NexelythSAOMenuController : MonoBehaviour
{
    [Header("SAO Menu")]
    [SerializeField] private GameObject menuCanvas;

    [Header("SAO Category Flyout")]
    [SerializeField] private GameObject categoryFlyout;

    [Header("Additional Category Flyouts")]
    [SerializeField] private GameObject socialFlyout;
    [SerializeField] private GameObject chatFlyout;

    [Header("Category Visuals")]
    [SerializeField]
    private NexelythSAOCategoryVisual characterVisual;

    [SerializeField]
    private NexelythSAOCategoryVisual socialVisual;


    [Header("Main Icon Sorting")]
    [SerializeField] private RectTransform characterButton;
    [SerializeField] private RectTransform socialButton;


    [SerializeField]
    private NexelythSAOCategoryVisual chatVisual;

    private GameObject activeFlyout;

    private void Awake()
    {
        CloseAllFlyouts();

        if (menuCanvas != null)
            menuCanvas.SetActive(false);
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        // M：開啟或關閉玩家選單
        if (keyboard.mKey.wasPressedThisFrame)
            ToggleMenu();

        // F8：測試 UI Raycast
        if (keyboard.f8Key.wasPressedThisFrame)
            DebugUIRaycast();
    }

    public void ToggleMenu()
    {
        if (menuCanvas == null)
        {
            Debug.LogWarning(
                "[NEXELYTH SAO] Menu Canvas missing.",
                this
            );
            return;
        }

        bool shouldOpen = !menuCanvas.activeSelf;

        CloseAllFlyouts();
        menuCanvas.SetActive(shouldOpen);
    }

    // Character 主選單
    // 保留原本 Button OnClick 綁定
    public void ToggleCategoryFlyout()
    {
        ToggleFlyout(categoryFlyout);
    }

    // Social 主選單
    public void ToggleSocialFlyout()
    {
        ToggleFlyout(socialFlyout);
    }

    // Chat 主選單
    public void ToggleChatFlyout()
    {
        ToggleFlyout(chatFlyout);
    }

    private void ToggleFlyout(GameObject target)
    {
        if (target == null)
        {
            Debug.LogWarning(
                "[NEXELYTH SAO] Flyout not assigned.",
                this
            );
            return;
        }

        // 再次點擊目前選中的分類 → 收合
        if (activeFlyout == target && target.activeSelf)
        {
            CloseAllFlyouts();

            Debug.Log(
                "[NEXELYTH SAO] Category collapsed.",
                this
            );

            return;
        }

        // 切換分類前，關閉其他選單並清除高亮
        CloseAllFlyouts();


        target.SetActive(true);
        activeFlyout = target;

        MoveSelectedIconToTop(target);
        UpdateCategoryVisuals(target);


        Debug.Log(
            $"[NEXELYTH SAO] Opened: {target.name}",
            this
        );
    }

    private void UpdateCategoryVisuals(GameObject selectedFlyout)
    {
        if (characterVisual != null)
        {
            characterVisual.SetSelected(
                selectedFlyout == categoryFlyout
            );
        }

        if (socialVisual != null)
        {
            socialVisual.SetSelected(
                selectedFlyout == socialFlyout
            );
        }

        if (chatVisual != null)
        {
            chatVisual.SetSelected(
                selectedFlyout == chatFlyout
            );
        }
    }

    private void CloseAllFlyouts()
    {
        if (categoryFlyout != null)
            categoryFlyout.SetActive(false);

        if (socialFlyout != null)
            socialFlyout.SetActive(false);

        if (chatFlyout != null)
            chatFlyout.SetActive(false);

        activeFlyout = null;

        // 全部恢復未選取狀態
        UpdateCategoryVisuals(null);
    }

    [ContextMenu("Debug UI Raycast At Mouse")]
    private void DebugUIRaycast()
    {
        EventSystem eventSystem = EventSystem.current;

        if (eventSystem == null || Mouse.current == null)
        {
            Debug.LogWarning(
                "[NEXELYTH SAO] EventSystem or Mouse missing.",
                this
            );
            return;
        }

        var eventData = new PointerEventData(eventSystem);

        eventData.position =
            Mouse.current.position.ReadValue();

        var results = new List<RaycastResult>();

        eventSystem.RaycastAll(eventData, results);

        Debug.Log(
            $"[NEXELYTH SAO] Raycast hits: {results.Count}",
            this
        );

        foreach (var hit in results)
        {
            Debug.Log(
                $"[NEXELYTH SAO] Hit: {hit.gameObject.name}",
                this
            );
        }
    }


    private void MoveSelectedIconToTop(GameObject selectedFlyout)
    {
        RectTransform selectedButton = null;

        if (selectedFlyout == categoryFlyout)
            selectedButton = characterButton;
        else if (selectedFlyout == socialFlyout)
            selectedButton = socialButton;

        if (selectedButton == null)
            return;

        selectedButton.SetAsFirstSibling();

        Debug.Log(
            $"[NEXELYTH SAO] Moved to top: {selectedButton.name}",
            this
        );
    }

}
