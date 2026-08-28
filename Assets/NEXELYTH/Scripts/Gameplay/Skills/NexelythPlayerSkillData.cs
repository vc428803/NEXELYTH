using System;

[Serializable]
public class NexelythPlayerSkillData
{
    public string skillId;
    public int level;

    public NexelythPlayerSkillData(
        string skillId,
        int level
    )
    {
        //  建立玩家目前擁有技能的 Runtime 資料模型，
        // 未來可直接承接登入 API 回傳的 SkillId 與 Level。
        this.skillId = skillId;
        this.level = level;
    }
}