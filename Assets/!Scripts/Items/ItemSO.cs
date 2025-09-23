using UnityEngine;

[CreateAssetMenu(fileName="Item_", menuName="Game/Item")]
public class ItemSO : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public bool stackable = true;
    public int maxStack = 64; // Minecraft vibe
}