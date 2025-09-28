using UnityEngine;
using System;

public class Inventory : MonoBehaviour
{
    [Header("Sizes")]
    public int hotbarSize = 9;
    public int backpackSize = 27;

    [Header("Data")]
    public InventorySlot[] hotbar;
    public InventorySlot[] backpack;

    // UI listeners subscribe to this
    public event Action onChanged;

    void Awake()
    {
        if (hotbar == null || hotbar.Length != hotbarSize) hotbar = NewArray(hotbarSize);
        if (backpack == null || backpack.Length != backpackSize) backpack = NewArray(backpackSize);
    }

    InventorySlot[] NewArray(int size)
    {
        var arr = new InventorySlot[size];
        for (int i = 0; i < size; i++) arr[i] = new InventorySlot();
        return arr;
    }

    // Call this from outside to notify UI safely
    public void NotifyChanged() => onChanged?.Invoke();

    // Add item anywhere (hotbar first, then backpack)
    public int AddItem(ItemSO item, int amount = 1)
    {
        // 1) stack into existing
        for (int i = 0; i < hotbar.Length && amount > 0; i++)
            if (!hotbar[i].IsEmpty && hotbar[i].CanStack(item))
                amount = hotbar[i].Add(item, amount);
        for (int i = 0; i < backpack.Length && amount > 0; i++)
            if (!backpack[i].IsEmpty && backpack[i].CanStack(item))
                amount = backpack[i].Add(item, amount);

        // 2) fill empty
        for (int i = 0; i < hotbar.Length && amount > 0; i++)
            if (hotbar[i].IsEmpty)
                amount = hotbar[i].Add(item, amount);
        for (int i = 0; i < backpack.Length && amount > 0; i++)
            if (backpack[i].IsEmpty)
                amount = backpack[i].Add(item, amount);

        NotifyChanged();
        return amount; // leftover if couldn't fit
    }

    public bool Has(ItemSO item, int amount)
    {
        int total = 0;
        foreach (var s in hotbar)    if (!s.IsEmpty && s.item == item) total += s.count;
        foreach (var s in backpack)  if (!s.IsEmpty && s.item == item) total += s.count;
        return total >= amount;
    }

    public bool Remove(ItemSO item, int amount)
    {
        if (!Has(item, amount)) return false;

        for (int i = 0; i < hotbar.Length && amount > 0; i++)
            if (!hotbar[i].IsEmpty && hotbar[i].item == item)
                amount -= hotbar[i].Remove(amount);
        for (int i = 0; i < backpack.Length && amount > 0; i++)
            if (!backpack[i].IsEmpty && backpack[i].item == item)
                amount -= backpack[i].Remove(amount);

        NotifyChanged();
        return true;
    }
}