using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
/*
public class PurchasableItemsUI : MonoBehaviour
{
    [Header("Links")]
    public SkillTree skillTree;
    public Inventory inventory;
    
    [Header("UI References")]
    public Transform shopContainer;  // Container for shop items (towers, etc.) - place this in the same panel as skill tree
    public GameObject shopItemPrefab;  // Prefab for shop items (can reuse skillButtonPrefab or create new one)
    
    [Header("Purchasable Items")]
    [Tooltip("Towers that can be purchased directly with items (no skill points needed)")]
    public TowerSO[] purchasableTowers;
    
    [Header("Colors")]
    public Color canAffordColor = new Color(1f, 0.8f, 0.2f);   // Yellow when you can afford
    public Color cannotAffordColor = new Color(0.5f, 0.5f, 0.5f);    // Gray when you can't afford
    
    private Dictionary<TowerSO, ShopItemButton> shopButtons = new Dictionary<TowerSO, ShopItemButton>();
    
    void Awake()
    {
        if (!skillTree) skillTree = FindAnyObjectByType<SkillTree>();
        if (!inventory && skillTree != null) inventory = skillTree.inventory;
        if (!inventory) inventory = FindAnyObjectByType<Inventory>();
    }
    
    void Start()
    {
        // Try to auto-find shop container if not assigned
        if (shopContainer == null)
        {
            Debug.LogWarning("[PurchasableItemsUI] ShopContainer not assigned! Trying to find it automatically...");
            
            // Try to find it as a child of skill tree panel
            SkillTreeUI skillTreeUI = FindAnyObjectByType<SkillTreeUI>();
            if (skillTreeUI != null && skillTreeUI.skillTreePanel != null)
            {
                Transform found = skillTreeUI.skillTreePanel.transform.Find("ShopContainer");
                if (found == null) found = skillTreeUI.skillTreePanel.transform.Find("Shop");
                if (found != null)
                {
                    shopContainer = found;
                    Debug.Log($"[PurchasableItemsUI] ✓ Auto-found shop container: {shopContainer.name}");
                }
            }
            
            if (shopContainer == null)
            {
                Debug.LogError("[PurchasableItemsUI] ✗ Could not find shop container! Please create a GameObject named 'ShopContainer' as a child of SkillTreePanel and assign it in Inspector.");
            }
        }
        
        BuildShopUI();
        RefreshShopButtons();
        
        // Make sure shop container visibility matches the skill tree panel
        // Find SkillTreeUI to sync visibility
        SkillTreeUI skillTreeUI = FindAnyObjectByType<SkillTreeUI>();
        if (skillTreeUI != null && skillTreeUI.skillTreePanel != null)
        {
            // If skill tree panel is active, make sure shop container is also active
            // If skill tree panel is inactive, shop container should also be inactive
            if (shopContainer != null)
            {
                // Shop container should be a child of the skill tree panel, so it will show/hide with it
                // But we can also explicitly sync it here
                bool panelActive = skillTreeUI.skillTreePanel.activeInHierarchy;
                shopContainer.gameObject.SetActive(panelActive);
                
                // IMPORTANT: Make sure we're NOT disabling the skill container!
                // The shop and skills should both be visible together
                if (skillTreeUI.skillContainer != null && panelActive)
                {
                    skillTreeUI.skillContainer.gameObject.SetActive(true);
                    Debug.Log("[PurchasableItemsUI] Ensured skill container is also visible when shop is visible");
                }
            }
        }
    }
    
    void OnEnable()
    {
        // Listen for inventory changes to refresh shop buttons
        if (inventory != null)
        {
            inventory.onChanged += RefreshShopButtons;
        }
    }
    
    void OnDisable()
    {
        if (inventory != null)
        {
            inventory.onChanged -= RefreshShopButtons;
        }
    }
    
    void BuildShopUI()
    {
        // Build shop UI for purchasable items (towers, etc.)
        if (shopContainer == null)
        {
            Debug.LogWarning("[PurchasableItemsUI] ShopContainer is not assigned! Cannot build shop UI. Please assign a Transform (e.g., create 'ShopContainer' GameObject as child of SkillTreePanel).");
            return;
        }
        
        if (purchasableTowers == null || purchasableTowers.Length == 0)
        {
            Debug.LogWarning("[PurchasableItemsUI] PurchasableTowers array is empty! Add TowerSO assets to the PurchasableTowers array in Inspector.");
            return;
        }
        
        if (shopItemPrefab == null)
        {
            Debug.LogError("[PurchasableItemsUI] ShopItemPrefab is not assigned! Cannot create shop buttons. Please assign a prefab (can reuse skillButtonPrefab).");
            return;
        }
        
        // Clear old shop buttons
        foreach (Transform child in shopContainer)
            Destroy(child.gameObject);
        shopButtons.Clear();
        
        Debug.Log($"[PurchasableItemsUI] Building shop UI with {purchasableTowers.Length} purchasable towers...");
        
        // Create shop item for each tower
        int createdCount = 0;
        foreach (var towerSO in purchasableTowers)
        {
            if (towerSO == null) 
            {
                Debug.LogWarning("[PurchasableItemsUI] Found null TowerSO in purchasableTowers array! Skipping...");
                continue;
            }
            
            var go = Instantiate(shopItemPrefab, shopContainer);
            var shopBtn = go.GetComponent<ShopItemButton>();
            if (shopBtn == null)
                shopBtn = go.AddComponent<ShopItemButton>();
            
            shopBtn.Init(towerSO, this);
            shopButtons[towerSO] = shopBtn;
            createdCount++;
            
            Debug.Log($"[PurchasableItemsUI] ✓ Created shop button for {towerSO.name}");
        }
        
        Debug.Log($"[PurchasableItemsUI] ✓ Shop UI built successfully! Created {createdCount} shop buttons.");
    }
    
    public void RefreshShopButtons()
    {
        // Update all shop buttons to show if player can afford them
        foreach (var kvp in shopButtons)
        {
            var towerSO = kvp.Key;
            var button = kvp.Value;
            if (button != null)
                button.Refresh(skillTree);
        }
    }
    
    // Public method to rebuild shop UI (useful if setup changes)
    [ContextMenu("Rebuild Shop UI")]
    public void RebuildShopUI()
    {
        Debug.Log("[PurchasableItemsUI] Rebuilding shop UI...");
        BuildShopUI();
        RefreshShopButtons();
    }
    
    // Called when player clicks a shop item button
    public void OnShopItemClicked(TowerSO towerSO)
    {
        if (towerSO == null) return;
        
        Inventory inv = inventory != null ? inventory : (skillTree != null ? skillTree.inventory : null);
        if (inv == null)
        {
            Debug.LogError("[PurchasableItemsUI] No inventory found!");
            return;
        }
        
        // Check if player can afford it
        if (towerSO.costItem == null || towerSO.costAmount <= 0)
        {
            Debug.LogWarning($"[PurchasableItemsUI] Tower {towerSO.name} has no cost configured!");
            return;
        }
        
        if (!inv.Has(towerSO.costItem, towerSO.costAmount))
        {
            int has = GetItemCount(towerSO.costItem, inv);
            Debug.LogWarning($"[PurchasableItemsUI] Not enough {towerSO.costItem.itemName}! Need {towerSO.costAmount}, have {has}");
            return;
        }
        
        // Create or find the tower item
        ItemSO towerItem = CreateTowerItem(towerSO);
        if (towerItem == null)
        {
            Debug.LogError($"[PurchasableItemsUI] Failed to find/create tower item for {towerSO.name}!");
            Debug.LogError($"[PurchasableItemsUI] Make sure Item_Tower_Basic.asset exists in Assets/Prefabs/Items/ and has towerSO assigned!");
            return;
        }
        
        Debug.Log($"[PurchasableItemsUI] Using tower item: {towerItem.name} (itemName: {towerItem.itemName})");
        
        // Pay with items first
        bool removed = inv.Remove(towerSO.costItem, towerSO.costAmount);
        if (!removed)
        {
            Debug.LogError($"[PurchasableItemsUI] Failed to remove payment items! This shouldn't happen.");
            return;
        }
        
        Debug.Log($"[PurchasableItemsUI] Removed {towerSO.costAmount}x {towerSO.costItem.itemName} from inventory");
        
        // Add tower item to inventory
        Debug.Log($"[PurchasableItemsUI] Attempting to add {towerItem.itemName} to inventory...");
        int leftover = inv.AddItem(towerItem, 1);
        
        // Check inventory after adding
        bool hasTower = inv.Has(towerItem, 1);
        Debug.Log($"[PurchasableItemsUI] After AddItem - Leftover: {leftover}, Has tower in inventory: {hasTower}");
        
        if (leftover > 0)
        {
            Debug.LogWarning($"[PurchasableItemsUI] Inventory full! Could not add {leftover} tower(s)");
        }
        else if (!hasTower)
        {
            Debug.LogError($"[PurchasableItemsUI] Tower was not added to inventory! AddItem returned 0 but Has() returns false.");
        }
        else
        {
            Debug.Log($"[PurchasableItemsUI] ✓ Successfully purchased {towerItem.itemName} for {towerSO.costAmount}x {towerSO.costItem.itemName}!");
            Debug.Log($"[PurchasableItemsUI] ✓ Tower confirmed in inventory. Check your hotbar/backpack.");
        }
        
        // Refresh shop buttons to update affordability (so button stays enabled if you still have resources)
        RefreshShopButtons();
        
        // Also refresh inventory UI if it exists
        var inventoryUI = FindAnyObjectByType<InventoryUI>();
        if (inventoryUI != null)
        {
            inventoryUI.RefreshAll();
        }
        
        // Force inventory to notify changes (in case it didn't)
        if (inv != null)
        {
            inv.NotifyChanged();
        }
    }
    
    // Create or find tower item from TowerSO
    ItemSO CreateTowerItem(TowerSO towerSO)
    {
        if (towerSO == null) 
        {
            Debug.LogError("[PurchasableItemsUI] TowerSO is null!");
            return null;
        }
        
        Debug.Log($"[PurchasableItemsUI] Looking for tower item for {towerSO.name}");
        
        // Method 1: Try to find by towerSO reference match
        var allItems = Resources.FindObjectsOfTypeAll<ItemSO>();
        Debug.Log($"[PurchasableItemsUI] Found {allItems.Length} total ItemSO assets");
        
        foreach (var item in allItems)
        {
            if (item == null) continue;
            
            // Check if this item references the same TowerSO
            if (item.towerSO == towerSO)
            {
                Debug.Log($"[PurchasableItemsUI] ✓ Found matching tower item by TowerSO reference: {item.name}");
                return item;
            }
        }
        
        // Method 2: Try to load Item_Tower_Basic directly (for Tower_Weak)
        if (towerSO.name.Contains("Weak") || towerSO.name.Contains("Basic"))
        {
            var paths = new[] { 
                "Item_Tower_Basic",
                "Prefabs/Items/Item_Tower_Basic",
                "Items/Item_Tower_Basic"
            };
            
            foreach (var path in paths)
            {
                var towerItem = Resources.Load<ItemSO>(path);
                if (towerItem != null)
                {
                    Debug.Log($"[PurchasableItemsUI] ✓ Loaded Item_Tower_Basic from Resources path: {path}");
                    if (towerItem.towerSO == towerSO || towerItem.towerSO != null)
                    {
                        return towerItem;
                    }
                }
            }
        }
        
        // Method 3: Try to find by name pattern
        foreach (var item in allItems)
        {
            if (item == null || !item.isPlaceable || item.towerSO == null) continue;
            
            if ((towerSO.name.Contains("Weak") && item.name.Contains("Tower")) ||
                (towerSO.name.Contains("Basic") && item.name.Contains("Tower")))
            {
                Debug.Log($"[PurchasableItemsUI] ✓ Found tower item by name pattern: {item.name}");
                return item;
            }
        }
        
        // Method 4: Load by GUID (Item_Tower_Basic GUID: 8348a733dd0d471cb16337e5525f43f0)
        #if UNITY_EDITOR
        var assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath("8348a733dd0d471cb16337e5525f43f0");
        if (!string.IsNullOrEmpty(assetPath))
        {
            var towerItem = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemSO>(assetPath);
            if (towerItem != null)
            {
                Debug.Log($"[PurchasableItemsUI] ✓ Loaded Item_Tower_Basic by GUID from path: {assetPath}");
                if (towerItem.towerSO == towerSO || towerItem.towerSO != null)
                {
                    return towerItem;
                }
            }
        }
        
        // Method 5: Try direct path
        var directPath = "Assets/Prefabs/Items/Item_Tower_Basic.asset";
        if (UnityEditor.AssetDatabase.LoadAssetAtPath<ItemSO>(directPath) != null)
        {
            var towerItem = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemSO>(directPath);
            Debug.Log($"[PurchasableItemsUI] ✓ Loaded Item_Tower_Basic from direct path: {directPath}");
            return towerItem;
        }
        #endif
        
        // Last resort: Log error and return null
        Debug.LogError($"[PurchasableItemsUI] ✗ Could not find tower item asset for {towerSO.name}!");
        Debug.LogError($"[PurchasableItemsUI] Please make sure Item_Tower_Basic.asset exists and has towerSO assigned to Tower_Weak");
        return null;
    }
    
    int GetItemCount(ItemSO item, Inventory inv)
    {
        if (inv == null || item == null) return 0;
        int total = 0;
        foreach (var s in inv.hotbar)
            if (!s.IsEmpty && s.item == item) total += s.count;
        foreach (var s in inv.backpack)
            if (!s.IsEmpty && s.item == item) total += s.count;
        return total;
    }
} */