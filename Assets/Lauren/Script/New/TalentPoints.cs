using UnityEngine;
using TMPro;

public class SkillPointsDisplay : MonoBehaviour
{
    public PlayerXP playerXP;
    public TextMeshProUGUI pointsText;

    void Awake()
    {
        if (playerXP == null)
        {
            playerXP = Object.FindFirstObjectByType<PlayerXP>();
        }
    }

    void OnEnable()
    {
        if (playerXP != null)
        {
            playerXP.onXPChanged += UpdateDisplay;
        }

        UpdateDisplay(0, 0, 0);
    }

    void OnDisable()
    {
        if (playerXP != null)
        {
            playerXP.onXPChanged -= UpdateDisplay;
        }
    }

    private void UpdateDisplay(int currentXP, int xpToNext, int level)
    {
        if (pointsText != null && playerXP != null)
        {
            pointsText.text = $"Available skill points: {playerXP.unspentSkillPoints}";
        }
    }
}
