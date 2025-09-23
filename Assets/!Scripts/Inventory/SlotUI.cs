using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotUI : MonoBehaviour
{
    InventoryUI ui;
    public bool isHotbar;
    public int index;

    [Header("UI References")]
    public Image icon;                 // child "Icon" image
    public TextMeshProUGUI countText;  // child "Count" text

    public InventorySlot bound { get; private set; }

    public void Init(InventoryUI ui, bool isHotbar, int index)
    {
        this.ui = ui;
        this.isHotbar = isHotbar;
        this.index = index;

        if (icon == null)      icon      = transform.Find("Icon")?.GetComponent<Image>();
        if (countText == null) countText = transform.Find("Count")?.GetComponent<TextMeshProUGUI>();

        var btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                ui.OnSlotLeftClick(this);

                // Optional: shift quick-move when NOT carrying
                bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                if (shift && !ui.IsCarrying) ui.ClickSlot(isHotbar, index, true);
            });
        }

        // Make sure our children don't block raycasts (so clicks outside slots work)
        if (icon != null)      icon.raycastTarget = false;
        if (countText != null) countText.raycastTarget = false;
    }

    public void Bind(InventorySlot slot)
    {
        bound = slot;

        if (slot == null || slot.IsEmpty)
        {
            if (icon != null) { icon.enabled = false; icon.sprite = null; }
            if (countText != null) countText.text = "";
            return;
        }

        if (icon != null)
        {
            bool hasSprite = (slot.item != null && slot.item.icon != null);
            icon.enabled = hasSprite;
            icon.sprite  = hasSprite ? slot.item.icon : null;
        }

        if (countText != null)
        {
            bool showCount = (slot.item != null && slot.item.stackable && slot.count > 1);
            countText.text = showCount ? slot.count.ToString() : "";
        }
    }
}