using System;
using System.Collections.Generic;
using UnityEngine;

public class SkillTree : MonoBehaviour
{
    [Header("Connect These to Your Player")]
    public PlayerXP playerXP;
    public Health playerHealth;
    public PlayerController playerController;
    public MineManager mineManager;
    public Inventory inventory;

    [Header("All Skills")]
    [Tooltip("Put all your skill assets here - drag them from the Project window")]
    public SkillSO[] allSkills;

    private Dictionary<string, int> unlockedSkills = new Dictionary<string, int>();

    // Cached modifiers (calculated once, reused)
    private float cachedHealthModifier;
    private float cachedMoveSpeedModifier;
    private float cachedDefenseModifier;
    private float cachedXPBonusModifier;
    private float cachedTowerDamageModifier;
    private float cachedTowerRangeModifier;
    private float cachedTowerFireRateModifier;
    private bool modifiersDirty = true;

    public event Action<SkillSO, int> onSkillUnlocked;
    public event Action onSkillsChanged;
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
        float healthFlat = 0f, healthPercent = 0f;
        float speedFlat = 0f, speedPercent = 0f;
        float defensePercent = 0f;
        float xpPercent = 0f;
        float towerDamagePercent = 0f;
        float towerRangePercent = 0f;
        float towerFireRatePercent = 0f;

        foreach (var kvp in unlockedSkills)
        {
            var skill = FindSkill(kvp.Key);
            if (skill == null) continue;

            int rank = kvp.Value;
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

        float baseHealth = playerHealth != null ? playerHealth.baseMaxHealth : 100f;
        cachedHealthModifier = healthFlat + (baseHealth * healthPercent);

        float baseSpeed = playerController != null ? playerController.baseMoveSpeed : 6f;
        cachedMoveSpeedModifier = speedFlat + (baseSpeed * speedPercent);

        cachedDefenseModifier = Mathf.Clamp01(defensePercent);
        cachedXPBonusModifier = 1f + xpPercent;
        cachedTowerDamageModifier = towerDamagePercent;
        cachedTowerRangeModifier = 1f + towerRangePercent;
        cachedTowerFireRateModifier = 1f + towerFireRatePercent;

        modifiersDirty = false;
    }

    void Awake()
    {
        if (!playerXP) playerXP = GetComponent<PlayerXP>();
        if (!playerHealth) playerHealth = GetComponent<Health>();
        if (!playerController) playerController = GetComponent<PlayerController>();
        if (!mineManager) mineManager = GetComponent<MineManager>();
        if (!inventory) inventory = GetComponent<Inventory>();
        
        if (playerHealth != null)
            playerHealth.onDeath.AddListener(OnPlayerDeath);
    }
    
    void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.onDeath.RemoveListener(OnPlayerDeath);
    }

    [Header("Reset Settings")]
    [Tooltip("If true, skills reset every time you press Play. If false, skills save between sessions.")]
    public bool resetSkillsOnStart = true;

    void Start()
    {
        modifiersDirty = true;
        
        if (resetSkillsOnStart)
            ResetAllSkills();
        else
            LoadSkills();
        
        ApplyAllModifiers();
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

        // Give mines to inventory if this is the mine skill
        if (skill.skillType == SkillType.Mine && mineManager != null && inventory != null)
        {
            // Give 5 mines when skill is first unlocked
            if (unlockedSkills[id] == 1)
            {
                // Try to find the mine item if not assigned
                if (mineManager.mineItem == null)
                {
                    // Try to load it from Resources or find it
                    var mineItem = Resources.Load<ItemSO>("Mine");
                    if (mineItem == null)
                    {
                        // Try to find it by name
                        var allItems = Resources.FindObjectsOfTypeAll<ItemSO>();
                        foreach (var item in allItems)
                        {
                            if (item != null && (item.name == "Mine" || item.itemName == "Land Mine"))
                            {
                                mineManager.mineItem = item;
                                Debug.Log($"[SkillTree] Found mine item: {item.name}");
                                break;
                            }
                        }
                    }
                    else
                    {
                        mineManager.mineItem = mineItem;
                    }
                }
                
                if (mineManager.mineItem != null)
                {
                    inventory.AddItem(mineManager.mineItem, 5);
                    Debug.Log($"[SkillTree] Added 5 mines to inventory!");
                }
                else
                {
                    Debug.LogWarning("[SkillTree] Could not find mine item! Please assign it manually in MineManager component.");
                }
            }
        }

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
        if (!hasFocus && !resetSkillsOnStart) SaveSkills();
    }
    
    // Reset all unlocked skills (useful for testing or when player dies)
    public void ResetAllSkills()
    {
        unlockedSkills.Clear();
        modifiersDirty = true;
        ApplyAllModifiers();
        
        // Clear all mines when skills reset
        if (mineManager != null)
            mineManager.ClearAllMines();
        
        onSkillsChanged?.Invoke();
        
        // Clear saved skills from PlayerPrefs
        int count = PlayerPrefs.GetInt("SkillCount", 0);
        for (int i = 0; i < count; i++)
        {
            PlayerPrefs.DeleteKey($"Skill_{i}_ID");
            PlayerPrefs.DeleteKey($"Skill_{i}_Rank");
        }
        PlayerPrefs.DeleteKey("SkillCount");
        PlayerPrefs.Save();
        
        Debug.Log("[SkillTree] All skills reset!");
    }
    
    // Called automatically when player dies (connected to Health.onDeath event)
    void OnPlayerDeath()
    {
        if (resetSkillsOnStart)
        {
            ResetAllSkills();
            Debug.Log("[SkillTree] Player died - skills reset!");
        }
    }
}

