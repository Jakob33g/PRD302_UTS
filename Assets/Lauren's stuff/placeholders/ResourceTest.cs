using UnityEngine;
using TMPro;

public class ResourceTest : MonoBehaviour
{
    public int oreAmount = 0;
    public TextMeshProUGUI oreText;

    void Start()
    {
        UpdateUI();
    }
    public void AddResource(ItemSO item, int amount)
    {
        if (item == null || amount <= 0) return;
        oreAmount += amount;
        UpdateUI();
    }

    public bool Has(ItemSO item, int amount)
    {
        if (item == null || amount <= 0)
        {
            return false;
        }
        return oreAmount >= amount;
    }

        public bool Remove(ItemSO item, int amount)
    {
        if (!Has(item, amount)) return false;

        oreAmount -= amount;
        if (oreAmount < 0) oreAmount = 0;

        UpdateUI();
        return true;
    }
    
    void UpdateUI()
    {
        if (oreText != null)
            oreText.text = $"# of ore: {oreAmount}";
    }
}
