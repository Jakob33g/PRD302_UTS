using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class XPUI : MonoBehaviour
{
    public PlayerXP xp;
    public Image fill;
    public TextMeshProUGUI levelText;

    void OnEnable()
    {
        if (xp) xp.onXPChanged += OnXP;
        UpdateAll();
    }
    void OnDisable()
    {
        if (xp) xp.onXPChanged -= OnXP;
    }

    void UpdateAll()
    {
        if (!xp || !fill) return;
        fill.fillAmount = xp.GetFill01();
        if (levelText) levelText.text = $"Lv. {xp.level}";
    }

    void OnXP(int current, int toNext, int level)
    {
        UpdateAll();
    }
}