using System;
using UnityEngine;

public class PlayerXP : MonoBehaviour
{
    [Header("Player Level and Experience")]
    public int level = 1;
    public int currentXP = 0;

    [Tooltip("How much XP you need to go from level 1 to level 2. Each level needs more XP.")]
    public int baseXpToNext = 100;

    [Tooltip("How much harder each level gets. 1.25 means each level needs 25% more XP.")]
    public float growth = 1.25f;

    [Tooltip("How many skill points you get each time you level up")]
    public int skillPointsPerLevel = 1;

    public int unspentSkillPoints = 0;

    public int xpToNext
    {
        get
        {
            // Calculate how much XP is needed for the next level (gets harder each level)
            double req = baseXpToNext * Math.Pow(growth, Math.Max(0, level - 1));
            return Mathf.Max(1, Mathf.RoundToInt((float)req));
        }
    }

    // These events tell other scripts when XP or level changes
    public event Action<int, int, int> onXPChanged;   // Fires when XP changes (currentXP, xpToNext, level)
    public event Action<int> onLevelUp;               // Fires when you level up (new level)

    public void GainXP(int amount)
    {
        if (amount <= 0) return;

        Debug.Log($"[XP] Gaining {amount} XP");

        int remaining = amount;

        while (remaining > 0)
        {
            int need = xpToNext - currentXP;
            if (remaining >= need)
            {
                currentXP += need;
                remaining -= need;
                LevelUp();
            }
            else
            {
                currentXP += remaining;
                remaining = 0;
                FireXPChanged();
            }
        }
    }

    void LevelUp()
    {
        level++;
        currentXP = 0;
        unspentSkillPoints += skillPointsPerLevel;
        Debug.Log($"[XP] LEVEL UP → Lv.{level} (Unspent skill points: {unspentSkillPoints})");
        FireXPChanged();
        onLevelUp?.Invoke(level);
    }

    void FireXPChanged()
    {
        onXPChanged?.Invoke(currentXP, xpToNext, level);
    }

    public float GetFill01()
    {
        return xpToNext > 0 ? (float)currentXP / xpToNext : 0f;
    }

    [Header("Testing - Press 2 to Gain XP")]
    [Tooltip("How much XP you get when you press the 2 key (for testing the skill tree)")]
    public int testXPAmount = 50;

    void Update()
    {
        // Press the 2 key to gain XP and level up (useful for testing skills)
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            GainXP(testXPAmount);
            Debug.Log($"[XP] TEST: Gained {testXPAmount} XP (Press '2' to test leveling)");
        }
    }
}





