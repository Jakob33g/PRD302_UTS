using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    public ItemSO item;
    public int count;

    public bool IsEmpty => item == null || count <= 0;
    public bool CanStack(ItemSO other) =>
        item == other && item != null && item.stackable && count < item.maxStack;

    public int Add(ItemSO toAdd, int amount)
    {
        if (IsEmpty)
        {
            item = toAdd;
            int take = toAdd.stackable ? Mathf.Min(amount, toAdd.maxStack) : 1;
            count = take;
            return amount - take;
        }
        if (CanStack(toAdd))
        {
            int space = item.maxStack - count;
            int put = Mathf.Min(space, amount);
            count += put;
            return amount - put;
        }
        return amount; // couldn't place here
    }

    public int Remove(int amount)
    {
        int removed = Mathf.Min(amount, count);
        count -= removed;
        if (count <= 0){ item = null; count = 0; }
        return removed;
    }
}