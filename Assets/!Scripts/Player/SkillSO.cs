using UnityEngine;

[CreateAssetMenu(menuName = "Game/Skill", fileName = "NewSkill")]
public class SkillSO : ScriptableObject
{
    [Header("Basic Info")]
    [Tooltip("A special ID number for this skill. The game makes this automatically - don't change it.")]
    public string skillID;
    public string skillName = "New Skill";
    [TextArea(2, 4)]
    public string description = "Skill description";
    public Sprite icon;

    void OnValidate()
    {
        // If the ID is empty, make one automatically
        if (string.IsNullOrEmpty(skillID))
        {
            skillID = System.Guid.NewGuid().ToString();
        }
    }

    [Header("What This Skill Does")]
    public SkillType skillType = SkillType.Health;
    
    [Header("How Much It Boosts Stats")]
    [Tooltip("Adds a fixed number. Example: +50 health")]
    public float flatBonus = 0f;
    
    [Tooltip("Adds a percentage. Example: 0.25 means +25%")]
    public float percentBonus = 0f;

    [Header("What You Need to Unlock")]
    [Tooltip("What level the player must be to unlock this")]
    public int requiredLevel = 1;
    
    [Tooltip("Other skills you must unlock first before this one")]
    public SkillSO[] prerequisites;

    [Header("How Much It Costs")]
    [Tooltip("How many skill points you need to unlock this")]
    public int skillPointCost = 1;

    [Header("How Many Times You Can Upgrade")]
    [Tooltip("How many times you can buy this skill. 1 means you can only buy it once.")]
    public int maxRank = 1;
}

public enum SkillType
{
    Health,           // Makes your health bar bigger
    MoveSpeed,        // Makes you move faster
    AttackDamage,     // Makes your attacks do more damage (if you can attack)
    AttackSpeed,      // Makes you attack faster (if you can attack)
    Defense,          // Makes you take less damage from enemies
    XPBonus,          // Makes you get more experience points from kills
    TowerDamage,      // Makes your towers do more damage
    TowerRange,       // Makes your towers shoot farther
    TowerFireRate,    // Makes your towers shoot faster
    ResourceGain,     // Makes you gather resources faster
    Mine,             // Unlocks ability to place mines that explode when enemies step on them
}

