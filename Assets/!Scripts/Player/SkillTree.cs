using System;
using System.Collections.Generic;
using UnityEngine;

public class SkillTree : MonoBehaviour
{
    [Header("Connect These to Your Player")]
    public PlayerXP playerXP;
    public Health playerHealth;
    public PlayerController playerController;

    [Header("All Skills")]
    [Tooltip("Put all your skill assets here - drag them from the Project window")]
    public SkillSO[] allSkills;

    // Keeps track of which skills are unlocked and at what rank
    private Dictionary<string, int> unlockedSkills = new Dictionary<string, int>();

    // Saves the calculated bonuses so we don't have to calculate them every frame
    private float cachedHealthModifier;
    private float cachedMoveSpeedModifier;
    private float cachedDefenseModifier;
    private float cachedXPBonusModifier;
    private float cachedTowerDamageModifier;
    private float cachedTowerRangeModifier;
    private float cachedTowerFireRateModifier;
    private bool modifiersDirty = true;

    // Things that happen when skills change - other scripts can listen to these
    public event Action<SkillSO, int> onSkillUnlocked;  // Fires when you unlock a skill
    public event Action onSkillsChanged;                 // Fires when any skill changes

    // These functions give other scripts the bonus values from your unlocked skills
    public float GetHealthModifier()
    {
        if (modifiersDirty) RecalculateModifiers();
        return cachedHealthModifier;
    }

    public float GetMoveSpeedModifier()
    {
        if (modifiersDirty) RecalculateModifiers();
        return cachedMoveSpeedModifier;
    }

    public float GetDefenseModifier()
    {
        if (modifiersDirty) RecalculateModifiers();
        return cachedDefenseModifier;
    }

    public float GetXPBonusModifier()
    {
        if (modifiersDirty) RecalculateModifiers();
        return cachedXPBonusModifier;
    }

    public float GetTowerDamageModifier()
    {
        if (modifiersDirty) RecalculateModifiers();
        return cachedTowerDamageModifier;
    }

    public float GetTowerRangeModifier()
    {
        if (modifiersDirty) RecalculateModifiers();
        return cachedTowerRangeModifier;
    }

    public float GetTowerFireRateModifier()
    {
        if (modifiersDirty) RecalculateModifiers();
        return cachedTowerFireRateModifier;
    }

    void RecalculateModifiers()
    {
        // Start with zero for all bonuses
        float healthFlat = 0f, healthPercent = 0f;
        float speedFlat = 0f, speedPercent = 0f;
        float defensePercent = 0f;
        float xpPercent = 0f;
        float towerDamagePercent = 0f;
        float towerRangePercent = 0f;
        float towerFireRatePercent = 0f;

        // Go through all unlocked skills and add up their bonuses
        foreach (var kvp in unlockedSkills)
        {
            var skill = FindSkill(kvp.Key);
            if (skill == null) continue;

            int rank = kvp.Value;  // How many times you've bought this skill
            switch (skill.skillType)
            {
                case SkillType.Health:
                    healthFlat += skill.flatBonus * rank;
                    healthPercent += skill.percentBonus * rank;
                    break;
                case SkillType.MoveSpeed:
                    speedFlat += skill.flatBonus * rank;
                    speedPercent += skill.percentBonus * rank;
                    break;
                case SkillType.Defense:
                    defensePercent += skill.percentBonus * rank;
                    break;
                case SkillType.XPBonus:
                    xpPercent += skill.percentBonus * rank;
                    break;
                case SkillType.TowerDamage:
                    towerDamagePercent += skill.percentBonus * rank;
                    break;
                case SkillType.TowerRange:
                    towerRangePercent += skill.percentBonus * rank;
                    break;
                case SkillType.TowerFireRate:
                    towerFireRatePercent += skill.percentBonus * rank;
                    break;
            }
        }

        // Calculate the final bonus amounts and save them
        float baseHealth = playerHealth != null ? playerHealth.baseMaxHealth : 100f;
        cachedHealthModifier = healthFlat + (baseHealth * healthPercent);

        float baseSpeed = playerController != null ? playerController.baseMoveSpeed : 6f;
        cachedMoveSpeedModifier = speedFlat + (baseSpeed * speedPercent);

        cachedDefenseModifier = Mathf.Clamp01(defensePercent);
        cachedXPBonusModifier = 1f + xpPercent;
        cachedTowerDamageModifier = towerDamagePercent;
        cachedTowerRangeModifier = 1f + towerRangePercent;
        cachedTowerFireRateModifier = 1f + towerFireRatePercent;

        modifiersDirty = false;  // Mark as calculated
    }

    void Awake()
    {
        // Try to find these components on the same GameObject if they weren't assigned
        if (!playerXP)
            playerXP = GetComponent<PlayerXP>();
        if (!playerHealth)
            playerHealth = GetComponent<Health>();
        if (!playerController)
            playerController = GetComponent<PlayerController>();
    }

    void Start()
    {
        modifiersDirty = true;
        ApplyAllModifiers();
        
        // Try to load skills from last time you played
        LoadSkills();
    }

    public bool CanUnlockSkill(SkillSO skill)
    {
        if (skill == null) return false;
        if (playerXP == null) return false;

        // Check if player is high enough level
        if (playerXP.level < skill.requiredLevel) return false;

        // Check if player has enough skill points
        int currentRank = GetSkillRank(skill);
        if (currentRank >= skill.maxRank) return false;  // Already bought this skill max times
        if (playerXP.unspentSkillPoints < skill.skillPointCost) return false;

        // Check if player unlocked all the skills needed first
        if (skill.prerequisites != null)
        {
            foreach (var prereq in skill.prerequisites)
            {
                if (prereq == null) continue;
                if (GetSkillRank(prereq) < 1) return false;  // Need to unlock this prerequisite first
            }
        }

        return true;
    }

    public bool TryUnlockSkill(SkillSO skill)
    {
        // Check if player can unlock this skill
        if (!CanUnlockSkill(skill)) return false;

        // Take away the skill points
        playerXP.unspentSkillPoints -= skill.skillPointCost;

        // Increase the skill rank (how many times you've bought it)
        string id = string.IsNullOrEmpty(skill.skillID) ? skill.name : skill.skillID;
        if (!unlockedSkills.ContainsKey(id))
            unlockedSkills[id] = 0;
        unlockedSkills[id]++;

        // Mark that we need to recalculate bonuses
        modifiersDirty = true;

        // Update player stats with new bonuses
        ApplyAllModifiers();

        // Tell other scripts that a skill was unlocked
        onSkillUnlocked?.Invoke(skill, unlockedSkills[id]);
        onSkillsChanged?.Invoke();

        Debug.Log($"[SkillTree] Unlocked {skill.skillName} (Rank {unlockedSkills[id]}/{skill.maxRank})");
        return true;
    }

    public int GetSkillRank(SkillSO skill)
    {
        if (skill == null) return 0;
        string id = string.IsNullOrEmpty(skill.skillID) ? skill.name : skill.skillID;
        return unlockedSkills.TryGetValue(id, out int rank) ? rank : 0;
    }

    public bool IsSkillUnlocked(SkillSO skill)
    {
        return GetSkillRank(skill) > 0;
    }

    SkillSO FindSkill(string skillID)
    {
        // Look through all skills to find one with matching ID or name
        if (allSkills == null || string.IsNullOrEmpty(skillID)) return null;
        foreach (var s in allSkills)
        {
            if (s == null) continue;
            // Try to match by ID first, then by name if ID doesn't match
            if (!string.IsNullOrEmpty(s.skillID) && s.skillID == skillID)
                return s;
            if (s.name == skillID)
                return s;
        }
        return null;
    }

    void ApplyAllModifiers()
    {
        // Update player's max health with bonuses from skills
        if (playerHealth != null)
        {
            float modifier = GetHealthModifier();
            float newMax = playerHealth.baseMaxHealth + modifier;
            playerHealth.SetMaxHealth(newMax);

            // Update damage reduction from defense skills
            float defenseReduction = GetDefenseModifier();
            // If defense is 0.25, that means 25% less damage, so take 75% of damage
            playerHealth.SetDefenseMultiplier(1f - defenseReduction);
        }

        // Update player's movement speed with bonuses from skills
        if (playerController != null)
        {
            float modifier = GetMoveSpeedModifier();
            float newSpeed = playerController.baseMoveSpeed + modifier;
            playerController.SetMoveSpeed(newSpeed);
        }
    }

    // Save unlocked skills so they don't disappear when you close the game
    public void SaveSkills()
    {
        if (unlockedSkills.Count == 0) return;

        int index = 0;
        foreach (var kvp in unlockedSkills)
        {
            PlayerPrefs.SetString($"Skill_{index}_ID", kvp.Key);
            PlayerPrefs.SetInt($"Skill_{index}_Rank", kvp.Value);
            index++;
        }
        PlayerPrefs.SetInt("SkillCount", index);
        PlayerPrefs.Save();
        Debug.Log($"[SkillTree] Saved {index} skills");
    }

    // Load unlocked skills when game starts
    public void LoadSkills()
    {
        unlockedSkills.Clear();
        int count = PlayerPrefs.GetInt("SkillCount", 0);
        
        for (int i = 0; i < count; i++)
        {
            string id = PlayerPrefs.GetString($"Skill_{i}_ID", "");
            int rank = PlayerPrefs.GetInt($"Skill_{i}_Rank", 0);
            if (!string.IsNullOrEmpty(id) && rank > 0)
            {
                unlockedSkills[id] = rank;
            }
        }

        modifiersDirty = true;
        ApplyAllModifiers();
        onSkillsChanged?.Invoke();
        Debug.Log($"[SkillTree] Loaded {unlockedSkills.Count} skills");
    }

    // Auto-save when game is paused or loses focus
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) SaveSkills();
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus) SaveSkills();
    }
}

