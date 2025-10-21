using System;
using UnityEngine;

public class PlayerXP : MonoBehaviour
{
    [Header("Progression")]
    public int level = 1;
    public int currentXP = 0;

    [Tooltip("XP required for level 1->2. Subsequent levels grow by 'growth' multiplier.")]
    public int baseXpToNext = 100;

    [Tooltip("Multiplicative growth per level (e.g., 1.25 = +25% per level).")]
    public float growth = 1.25f;

    [Tooltip("Skill points awarded per level-up (for future skill tree).")]
    public int skillPointsPerLevel = 1;

    public int unspentSkillPoints = 0;

    public int xpToNext
    {
        get
        {
            double req = baseXpToNext * Math.Pow(growth, Math.Max(0, level - 1));
            return Mathf.Max(1, Mathf.RoundToInt((float)req));
        }
    }

    public event Action<int, int, int> onXPChanged;   // (currentXP, xpToNext, level)
    public event Action<int> onLevelUp;               // new level

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
}





