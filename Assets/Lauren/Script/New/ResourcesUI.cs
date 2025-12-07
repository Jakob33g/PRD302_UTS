using UnityEngine;
using TMPro;

public class ResourceUI : MonoBehaviour
{
    public ItemSO gem;
    private int count = 0;
    private TextMeshProUGUI text;
    private Inventory inventory;

    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        inventory = FindAnyObjectByType<Inventory>();

        if (inventory != null)
        {
            inventory.onItemChanged += HandleChanged;
            int startingAmount = inventory.GetItemCount(gem);
            UpdateText(startingAmount);
        }
    }

    void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.onItemChanged -= HandleChanged;
        }
    }

    private void HandleChanged(ItemSO item, int newAmount)
    {
        if (item == gem)
            UpdateText(newAmount);
    }
    public void AddAmount(int amount)
    {
        count += amount;
        UpdateText(count);
    }

    private void UpdateText(int newCount)
    {
        count = newCount;
        if (text != null)
            text.text = $"{count}";
    }
}