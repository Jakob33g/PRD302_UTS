using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    [HideInInspector] public float currentHealth;

    [System.Serializable] public class HealthChangeEvent : UnityEvent<float, float>{}
    public HealthChangeEvent onHealthChanged;
    public UnityEvent onDeath;

    void Awake()
    {
        currentHealth = maxHealth;
        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(float dmg)
    {
        if (currentHealth <= 0f) return;
        currentHealth = Mathf.Max(0f, currentHealth - Mathf.Max(0f, dmg));
        onHealthChanged?.Invoke(currentHealth, maxHealth);
        if (currentHealth <= 0f) onDeath?.Invoke();
    }

    public void Heal(float amount)
    {
        if (currentHealth <= 0f) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + Mathf.Max(0f, amount));
        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}