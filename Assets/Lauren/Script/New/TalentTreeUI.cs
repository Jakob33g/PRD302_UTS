using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TalentTreeUI : MonoBehaviour
{
    public SkillSO skill;
    public SkillTree skillTree;

    public TextMeshProUGUI levelText;
    public TextMeshProUGUI bonusText;
    public Button button;

    void Start()
    {
        if (button != null)
            button.onClick.AddListener(OnClick);

        UpdateUI();
    }

    void OnClick()
    {
        if (skillTree == null || skill == null) return;

        skillTree.TryUnlockSkill(skill);
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (skillTree == null || skill == null) return;

        int rank = skillTree.GetSkillRank(skill);

        levelText.text = $"Current Level: {rank}/{skill.maxRank}";

        string grant = "";
        if (skill.flatBonus != 0)
            grant += $"+{skill.flatBonus * rank} ";
        if (skill.percentBonus != 0)
            grant += $"+{skill.percentBonus * 100f * rank}%";

        bonusText.text = grant;
        
        button.interactable = skillTree.CanUnlockSkill(skill);
    }
}
