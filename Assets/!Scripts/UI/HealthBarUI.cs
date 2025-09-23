using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public Health health;     // we’ll link the Player's Health here
    public Image fillImage;   // we’ll link HealthBarFill here

    void OnEnable(){ if (health) health.onHealthChanged.AddListener(UpdateBar); }
    void OnDisable(){ if (health) health.onHealthChanged.RemoveListener(UpdateBar); }

    void UpdateBar(float current, float max)
    {
        if (fillImage && max > 0f) fillImage.fillAmount = current / max;
    }
}