using UnityEngine;

public class InvDebugAdder : MonoBehaviour
{
    public Inventory inv;
    public ItemSO woodItem;
    public ItemSO stoneItem;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && inv && woodItem)
            inv.AddItem(woodItem, 5);

        if (Input.GetKeyDown(KeyCode.Alpha2) && inv && stoneItem)
            inv.AddItem(stoneItem, 3);
    }
}