using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("Links")]
    public Inventory inventory;          // Player Inventory
    public Transform hotbarRoot;         // Hotbar panel (grid)
    public Transform backpackRoot;       // Backpack panel (grid)
    public GameObject slotPrefab;        // SlotUI prefab
    public GameObject backpackPanelObj;  // Backpack panel object

    [Header("Drop to world")]
    public GameObject pickupPrefab;      // Prefab with WorldPickup + Trigger + Kinematic RB
    public LayerMask groundMask;         // Layer(s) your ground uses
    public float dropDistance = 200f;    // Raycast length from camera
    public float dropUpOffset = 0.05f;   // Small lift so it doesn’t clip into ground

    private SlotUI[] hotbarSlots;
    private SlotUI[] backpackSlots;
    private bool backpackOpen;

    // ---- CARRY STATE (data) ----
    private InventorySlot carry = new InventorySlot();   // empty means "not carrying"
    public bool IsCarrying => !carry.IsEmpty;

    // ---- RUNTIME GHOST (visual) ----
    private RectTransform ghostRoot;
    private Image ghostIcon;
    private TextMeshProUGUI ghostCount;

    void OnEnable()  { if (inventory != null) inventory.onChanged += RefreshAll; }
    void OnDisable() { if (inventory != null) inventory.onChanged -= RefreshAll; }

    void Start()
    {
        if (backpackPanelObj) backpackPanelObj.SetActive(false);
        BuildUI();
        RefreshAll();
    }

    void Update()
    {
        // Toggle backpack
        if (Input.GetKeyDown(KeyCode.I) && backpackPanelObj)
        {
            backpackOpen = !backpackOpen;
            backpackPanelObj.SetActive(backpackOpen);
        }

        // Move ghost with cursor
        if (ghostRoot != null)
            ghostRoot.position = Input.mousePosition;

        // If carrying and user clicks outside any slot -> drop to world
        if (IsCarrying && Input.GetMouseButtonDown(0) && !PointerOverAnySlot())
            TryDropCarriedToWorld();
    }

    // ----------------- BUILD & REFRESH -----------------

    void BuildUI()
    {
        if (inventory == null || hotbarRoot == null || backpackRoot == null || slotPrefab == null)
        {
            Debug.LogError("InventoryUI: Assign Inventory, HotbarRoot, BackpackRoot, SlotPrefab.");
            return;
        }

        foreach (Transform t in hotbarRoot) Destroy(t.gameObject);
        foreach (Transform t in backpackRoot) Destroy(t.gameObject);

        hotbarSlots = new SlotUI[inventory.hotbar.Length];
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            var go = Instantiate(slotPrefab, hotbarRoot);
            var slot = go.GetComponent<SlotUI>() ?? go.AddComponent<SlotUI>();
            slot.Init(this, true, i);
            hotbarSlots[i] = slot;
        }

        backpackSlots = new SlotUI[inventory.backpack.Length];
        for (int i = 0; i < backpackSlots.Length; i++)
        {
            var go = Instantiate(slotPrefab, backpackRoot);
            var slot = go.GetComponent<SlotUI>() ?? go.AddComponent<SlotUI>();
            slot.Init(this, false, i);
            backpackSlots[i] = slot;
        }
    }

    public void RefreshAll()
    {
        if (hotbarSlots != null)
            for (int i = 0; i < hotbarSlots.Length; i++)
                hotbarSlots[i].Bind(inventory.hotbar[i]);

        if (backpackSlots != null)
            for (int i = 0; i < backpackSlots.Length; i++)
                backpackSlots[i].Bind(inventory.backpack[i]);

        UpdateGhostVisual(); // keep ghost icon/count in sync if carrying
    }

    // ----------------- CLICK HANDLERS -----------------

    public void OnSlotLeftClick(SlotUI slotUI)
    {
        if (slotUI == null) return;
        var slot = slotUI.bound;
        if (slot == null) return;

        if (!IsCarrying)
        {
            // Pick up whole stack from this slot
            if (!slot.IsEmpty)
            {
                carry.item  = slot.item;
                carry.count = slot.count;
                slot.Remove(slot.count);
                inventory.NotifyChanged();
                CreateGhostVisual(carry);
            }
            return;
        }

        // Place into clicked slot
        int left = slot.Add(carry.item, carry.count);
        carry.count = left;
        if (carry.count <= 0)
        {
            carry.item = null;
            carry.count = 0;
            DestroyGhostVisual();
        }
        inventory.NotifyChanged();
    }

    // Shift-click quick move (when NOT carrying anything)
    public void ClickSlot(bool isHotbar, int index, bool shift)
    {
        if (IsCarrying) return;

        var from = isHotbar ? inventory.hotbar[index] : inventory.backpack[index];
        if (from.IsEmpty) return;

        var target = isHotbar ? inventory.backpack : inventory.hotbar;

        if (shift)
        {
            MoveEntireStack(from, target);
            RefreshAll();
            return;
        }

        MoveSome(from, target);
        RefreshAll();
    }

    // ----------------- MOVE HELPERS -----------------

    void MoveEntireStack(InventorySlot from, InventorySlot[] target)
    {
        int left = from.count;
        for (int i = 0; i < target.Length && left > 0; i++)
            if (!target[i].IsEmpty && target[i].CanStack(from.item))
                left = target[i].Add(from.item, left);
        for (int i = 0; i < target.Length && left > 0; i++)
            if (target[i].IsEmpty)
                left = target[i].Add(from.item, left);
        int moved = from.count - left;
        from.Remove(moved);
    }

    void MoveSome(InventorySlot from, InventorySlot[] target)
    {
        int amount = from.count;
        for (int i = 0; i < target.Length && amount > 0; i++)
            if (!target[i].IsEmpty && target[i].CanStack(from.item))
                amount = target[i].Add(from.item, amount);
        for (int i = 0; i < target.Length && amount > 0; i++)
            if (target[i].IsEmpty)
                amount = target[i].Add(from.item, amount);
        int moved = from.count - amount;
        from.Remove(moved);
    }

    // ----------------- UI RAYCAST -----------------

    bool PointerOverAnySlot()
    {
        if (EventSystem.current == null) return false;

        var ped = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(ped, results);

        for (int i = 0; i < results.Count; i++)
            if (results[i].gameObject.GetComponentInParent<SlotUI>() != null)
                return true;

        return false;
    }

    // ----------------- DROP TO WORLD -----------------

    void TryDropCarriedToWorld()
    {
        if (!IsCarrying)
            return;

        if (pickupPrefab == null)
        {
            Debug.LogWarning("InventoryUI: pickupPrefab not set.");
            return;
        }

        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("InventoryUI: No Camera.main for drop raycast.");
            return;
        }

        // 1) Try ray from mouse to ground
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        LayerMask mask = (groundMask.value == 0) ? Physics.DefaultRaycastLayers : groundMask;
        float dist = (dropDistance <= 0) ? 500f : dropDistance;

        Vector3 dropPoint;
        if (Physics.Raycast(ray, out RaycastHit hit, dist, mask))
        {
            dropPoint = hit.point;
        }
        else
        {
            // 2) If we missed, project a point 2m in front of the camera,
            // then raycast DOWN to find the floor
            Vector3 probe = cam.transform.position + cam.transform.forward * 2f + Vector3.up * 5f;
            if (Physics.Raycast(probe, Vector3.down, out hit, 50f, mask))
                dropPoint = hit.point;
            else
                // 3) Last resort: just use that forward point
                dropPoint = cam.transform.position + cam.transform.forward * 2f;
        }

        // Spawn pickup
        var go = Instantiate(pickupPrefab, dropPoint + Vector3.up * dropUpOffset, Quaternion.identity);
        var wp = go.GetComponent<WorldPickup>();
        if (wp != null)
        {
            wp.item = carry.item;
            wp.amount = carry.count;
        }
        else
        {
            Debug.LogError("InventoryUI: pickupPrefab has no WorldPickup component!");
        }

        // Clear carry so you can pick/drop again immediately
        carry.item = null;
        carry.count = 0;
        DestroyGhostVisual();
        inventory.NotifyChanged();
    }

    // ----------------- GHOST VISUAL -----------------

    void CreateGhostVisual(InventorySlot s)
    {
        DestroyGhostVisual(); // just in case

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("InventoryUI: No Canvas found for ghost visual.");
            return;
        }

        // Root
        GameObject root = new GameObject("CarryGhost", typeof(RectTransform));
        root.transform.SetParent(canvas.transform, false);
        ghostRoot = root.GetComponent<RectTransform>();
        ghostRoot.pivot = new Vector2(0.5f, 0.5f);
        ghostRoot.sizeDelta = new Vector2(64, 64);
        ghostRoot.SetAsLastSibling();

        // Icon
        GameObject iconGO = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconGO.transform.SetParent(ghostRoot, false);
        ghostIcon = iconGO.GetComponent<Image>();
        ghostIcon.raycastTarget = false;
        ghostIcon.preserveAspect = true;

        // Count
        GameObject countGO = new GameObject("Count", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        countGO.transform.SetParent(ghostRoot, false);
        ghostCount = countGO.GetComponent<TextMeshProUGUI>();
        ghostCount.raycastTarget = false;
        ghostCount.alignment = TextAlignmentOptions.BottomRight;
        ghostCount.fontSize = 24;
        ghostCount.enableAutoSizing = true;
        ghostCount.rectTransform.anchorMin = new Vector2(0, 0);
        ghostCount.rectTransform.anchorMax = new Vector2(1, 1);
        ghostCount.rectTransform.offsetMin = new Vector2(4, 2);
        ghostCount.rectTransform.offsetMax = new Vector2(-4, -2);

        UpdateGhostVisual();
        ghostRoot.position = Input.mousePosition;
    }

    void UpdateGhostVisual()
    {
        if (ghostRoot == null || ghostIcon == null || ghostCount == null) return;

        Sprite sprite = (carry.item != null) ? carry.item.icon : null;
        ghostIcon.sprite = sprite;
        ghostIcon.color  = (sprite != null) ? Color.white : new Color(1, 1, 1, 0);

        if (carry.item != null && carry.item.stackable && carry.count > 1)
            ghostCount.text = carry.count.ToString();
        else
            ghostCount.text = "";
    }

    void DestroyGhostVisual()
    {
        if (ghostRoot != null)
        {
            Destroy(ghostRoot.gameObject);
            ghostRoot = null;
            ghostIcon = null;
            ghostCount = null;
        }
    }
}