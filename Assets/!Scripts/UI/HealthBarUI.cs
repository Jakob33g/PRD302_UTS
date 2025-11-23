using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarUI : MonoBehaviour
{
    [Header("Health Display")]
    public Health health;
    public Image fillImage;
    public TextMeshProUGUI healthText;  // Shows "100 / 100"
    
    [Header("Speed Display")]
    public PlayerController playerController;
    public TextMeshProUGUI speedText;  // Shows "Speed: 6.0"

    void OnEnable()
    {
        if (health) health.onHealthChanged.AddListener(UpdateBar);
        InvokeRepeating(nameof(UpdateSpeed), 0f, 0.1f);
    }
    
    void OnDisable()
    {
        if (health) health.onHealthChanged.RemoveListener(UpdateBar);
        CancelInvoke(nameof(UpdateSpeed));
    }

    void Start()
    {
        if (!health) health = FindAnyObjectByType<Health>();
        if (!playerController) playerController = FindAnyObjectByType<PlayerController>();
        
        if (health) UpdateBar(health.currentHealth, health.maxHealth);
        UpdateSpeed();
    }

    void UpdateBar(float current, float max)
    {
        if (fillImage && max > 0f)
            fillImage.fillAmount = current / max;
        
        if (healthText)
            healthText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
    }
    
    void UpdateSpeed()
    {
        if (speedText && playerController != null)
            speedText.text = $"Speed: {playerController.moveSpeed:F1}";
    }
}