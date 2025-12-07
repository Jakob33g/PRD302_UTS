using UnityEngine;
using TMPro;

public class ResourceUI : MonoBehaviour
{
    public ItemSO gem;
    private int count = 0;
    private TextMeshProUGUI text;

    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        UpdateText();
    }

    public void AddAmount(int amount)
    {
        count += amount;
        UpdateText();
    }

    void UpdateText()
    {
        if (text && gem != null)
            text.text = $"{count}";
    }
}
