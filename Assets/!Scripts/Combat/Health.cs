using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

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
    private float displayedHP;
    public float hpAnimSpeed = 8f;

    // lauren
    public AudioClip hurtSFX;
    public Image hpBar;
    public Renderer visual;
    public float flashDuration = 0.2f; //flash red on damage so playuer sees
    public StateManager stateManager;
    private AudioSource audioSource;
    private Color originalColor;
    private float flashTimer = 0f;
    //lauren end

    void Awake()
    {
        // Start with full health
        maxHealth = baseMaxHealth;
        currentHealth = maxHealth;
        displayedHP = currentHealth;
        onHealthChanged?.Invoke(currentHealth, maxHealth);

        audioSource = GetComponent<AudioSource>();
        originalColor = visual.material.color;
    }

    public void SetMaxHealth(float newMax)
    {
        // When max health changes, keep the same percentage of health
        float ratio = maxHealth > 0 ? currentHealth / maxHealth : 1f;
        maxHealth = newMax;
        currentHealth = maxHealth * ratio;
        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    void Update()
    {
        if (flashTimer > 0f && visual)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0f)
            {
                visual.material.color = originalColor;
            }
        }

        displayedHP = Mathf.Lerp(displayedHP, currentHealth, Time.deltaTime * hpAnimSpeed);
        hpBar.fillAmount = displayedHP / maxHealth;
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

        audioSource.PlayOneShot(hurtSFX);
        visual.material.color = Color.red;
        flashTimer = flashDuration;
        
        
        // Die if health reaches zero
        if (currentHealth <= 0f) 
        {
            onDeath?.Invoke();
            stateManager.GameOver();
        }
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