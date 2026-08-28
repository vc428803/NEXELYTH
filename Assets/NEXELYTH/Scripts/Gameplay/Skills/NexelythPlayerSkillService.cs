using System.Collections.Generic;
using UnityEngine;

public class NexelythPlayerSkillService : MonoBehaviour
{
    public static NexelythPlayerSkillService Instance { get; private set; }

    private readonly Dictionary<string, NexelythPlayerSkillData> playerSkills =
        new Dictionary<string, NexelythPlayerSkillData>();

    private void Awake()
    {
        // 玩家技能資料在遊戲執行期間需要由單一 Service 管理，
        // 讓 Memo、戰鬥技能與未來 UI 共用同一份技能狀態。
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        //  只有目前有效的 Player Skill Service
        // 被銷毀時才清除 Singleton 參考。
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void SetSkill(
        string skillId,
        int level
    )
    {
        // 提供統一入口更新玩家技能資料，
        // 未來登入 API 取得 SkillId 與 Level 後可直接寫入這裡。
        playerSkills[skillId] =
            new NexelythPlayerSkillData(
                skillId,
                level
            );
    }

    public NexelythPlayerSkillData GetSkill(
        string skillId
    )
    {
        // 修改原因：讓 Gameplay 系統可以依 SkillId
        // 查詢玩家目前是否擁有技能及技能等級。
        if (playerSkills.TryGetValue(
                skillId,
                out NexelythPlayerSkillData skillData))
        {
            return skillData;
        }

        return null;
    }

    [ContextMenu("Test Set Memo Level 1")]
    private void TestSetMemoLevel1()
    {
        // 修改原因：正式登入 API 尚未接入前，
        // 用固定測試資料驗證 MEMO Lv1 的 Slot 規則。
        SetSkill("MEMO", 1);
    }

    [ContextMenu("Test Set Memo Level 2")]
    private void TestSetMemoLevel2()
    {
        // 修改原因：正式登入 API 尚未接入前，
        // 用固定測試資料驗證 MEMO Lv2 的 Slot 規則。
        SetSkill("MEMO", 2);
    }

    [ContextMenu("Test Set Memo Level 3")]
    private void TestSetMemoLevel3()
    {
        // 修改原因：正式登入 API 尚未接入前，
        // 用固定測試資料驗證 MEMO Lv3 的 Slot 規則。
        SetSkill("MEMO", 3);
    }

}