using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [Header("Health Stats")]
    public float baseMaxHealth = 100f;  // Starting max health before skills
    
    [Header("Current Health")]
    [HideInInspector] public float maxHealth = 100f;  // Current max health (can be changed by skills)
    [HideInInspector] public float currentHealth;     // How much health you have right now

    // These events tell other scripts when health changes
    [System.Serializable] public class HealthChangeEvent : UnityEvent<float, float>{}
    public HealthChangeEvent onHealthChanged;  // Fires when health changes (current, max)
    public UnityEvent onDeath;                 // Fires when health reaches zero

    void Awake()
    {
        // Start with full health
        maxHealth = baseMaxHealth;
        currentHealth = maxHealth;
        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void SetMaxHealth(float newMax)
    {
        // When max health changes, keep the same percentage of health
        float ratio = maxHealth > 0 ? currentHealth / maxHealth : 1f;
        maxHealth = newMax;
        currentHealth = maxHealth * ratio;
        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    [Header("Defense - How Much Damage You Take")]
    [Tooltip("Damage multiplier. 1.0 = normal damage, 0.8 = 20% less damage. Set by SkillTree.")]
    public float defenseMultiplier = 1f;

    public void TakeDamage(float dmg)
    {
        // Don't take damage if already dead
        if (currentHealth <= 0f) return;
        
        // Apply defense reduction
        float actualDamage = dmg * defenseMultiplier;
        currentHealth = Mathf.Max(0f, currentHealth - Mathf.Max(0f, actualDamage));
        onHealthChanged?.Invoke(currentHealth, maxHealth);
        
        // Die if health reaches zero
        if (currentHealth <= 0f) onDeath?.Invoke();
    }

    public void SetDefenseMultiplier(float multiplier)
    {
        // Set how much damage you take (between 0 and 1)
        defenseMultiplier = Mathf.Clamp01(multiplier);
    }

    public void Heal(float amount)
    {
        if (currentHealth <= 0f) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + Mathf.Max(0f, amount));
        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}