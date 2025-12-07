using UnityEngine;
using UnityEngine.UI;
/*
/// <summary>
/// Helper script to diagnose and fix shop setup issues.
/// Add this to your SkillTreePanel GameObject and it will help set up the shop.
/// </summary>
[DefaultExecutionOrder(-50)]
public class ShopSetupHelper : MonoBehaviour
{
    [Header("Auto-Setup")]
    [Tooltip("If true, automatically creates shop container if missing")]
    public bool autoCreateShopContainer = true;
    
    [Tooltip("If true, automatically adds PurchasableItemsUI component if missing")]
    public bool autoAddShopComponent = true;
    
    [Tooltip("If true, logs setup steps")]
    public bool logSetup = true;

    void Start()
    {
        SetupShop();
    }

    [ContextMenu("Setup Shop")]
    public void SetupShop()
    {
        bool fixedAnything = false;

        // Find SkillTreePanel
        GameObject skillTreePanel = gameObject;
        if (!gameObject.name.Contains("SkillTree") && !gameObject.name.Contains("Panel"))
        {
            // Try to find it
            GameObject found = GameObject.Find("SkillTreePanel");
            if (found != null) skillTreePanel = found;
        }

        // Check if PurchasableItemsUI component exists
        PurchasableItemsUI shopUI = skillTreePanel.GetComponent<PurchasableItemsUI>();
        if (shopUI == null && autoAddShopComponent)
        {
            shopUI = skillTreePanel.AddComponent<PurchasableItemsUI>();
            if (logSetup) Debug.Log("[ShopSetupHelper] ✓ Added PurchasableItemsUI component");
            fixedAnything = true;
        }

        // Check if shop container exists
        Transform shopContainer = null;
        if (skillTreePanel != null)
        {
            shopContainer = skillTreePanel.transform.Find("ShopContainer");
            if (shopContainer == null) shopContainer = skillTreePanel.transform.Find("Shop");
            
            if (shopContainer == null && autoCreateShopContainer)
            {
                // Create shop container
                GameObject shopGO = new GameObject("ShopContainer");
                shopGO.transform.SetParent(skillTreePanel.transform, false);
                
                // Add layout group for automatic layout
                VerticalLayoutGroup layout = shopGO.AddComponent<VerticalLayoutGroup>();
                layout.spacing = 10f;
                layout.padding = new RectOffset(10, 10, 10, 10);
                layout.childControlWidth = true;
                layout.childControlHeight = false;
                layout.childForceExpandWidth = true;
                layout.childForceExpandHeight = false;
                
                // Add content size fitter
                ContentSizeFitter fitter = shopGO.AddComponent<ContentSizeFitter>();
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                
                shopContainer = shopGO.transform;
                if (logSetup) Debug.Log("[ShopSetupHelper] ✓ Created ShopContainer GameObject");
                fixedAnything = true;
            }
        }

        // Assign shop container to PurchasableItemsUI
        if (shopUI != null && shopContainer != null)
        {
            if (shopUI.shopContainer == null)
            {
                shopUI.shopContainer = shopContainer;
                if (logSetup) Debug.Log("[ShopSetupHelper] ✓ Assigned ShopContainer to PurchasableItemsUI");
                fixedAnything = true;
            }
        }

        // Check if shop item prefab is assigned
        if (shopUI != null && shopUI.shopItemPrefab == null)
        {
            // Try to find skill button prefab to reuse
            SkillTreeUI skillTreeUI = FindAnyObjectByType<SkillTreeUI>();
            if (skillTreeUI != null && skillTreeUI.skillButtonPrefab != null)
            {
                shopUI.shopItemPrefab = skillTreeUI.skillButtonPrefab;
                if (logSetup) Debug.Log("[ShopSetupHelper] ✓ Assigned skillButtonPrefab as shopItemPrefab");
                fixedAnything = true;
            }
            else
            {
                if (logSetup) Debug.LogWarning("[ShopSetupHelper] ⚠ shopItemPrefab not assigned! Please assign a button prefab in PurchasableItemsUI component.");
            }
        }

        // Check if purchasable towers are assigned
        if (shopUI != null && (shopUI.purchasableTowers == null || shopUI.purchasableTowers.Length == 0))
        {
            if (logSetup) Debug.LogWarning("[ShopSetupHelper] ⚠ purchasableTowers array is empty! Add TowerSO assets to PurchasableItemsUI.purchasableTowers array in Inspector.");
        }

        if (fixedAnything && logSetup)
        {
            Debug.Log("[ShopSetupHelper] ✓ Shop setup complete! Check PurchasableItemsUI component to add TowerSO assets.");
        }
        else if (!fixedAnything && logSetup)
        {
            Debug.Log("[ShopSetupHelper] ✓ Shop is already set up correctly!");
        }

        // Force rebuild shop UI
        if (shopUI != null)
        {
            // Use reflection to call BuildShopUI if it's private
            var method = shopUI.GetType().GetMethod("BuildShopUI", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (method != null)
            {
                method.Invoke(shopUI, null);
                if (logSetup) Debug.Log("[ShopSetupHelper] ✓ Rebuilt shop UI");
            }
            else
            {
                // Try public method
                var publicMethod = shopUI.GetType().GetMethod("RebuildShopUI", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (publicMethod != null)
                {
                    publicMethod.Invoke(shopUI, null);
                    if (logSetup) Debug.Log("[ShopSetupHelper] ✓ Rebuilt shop UI (public method)");
                }
            }
        }
    }

    [ContextMenu("Check Shop Setup")]
    public void CheckShopSetup()
    {
        Debug.Log("[ShopSetupHelper] ===== Checking Shop Setup =====");
        
        PurchasableItemsUI shopUI = GetComponent<PurchasableItemsUI>();
        if (shopUI == null)
        {
            Debug.LogError("[ShopSetupHelper] ✗ PurchasableItemsUI component not found!");
            Debug.LogError("[ShopSetupHelper] Fix: Add PurchasableItemsUI component to SkillTreePanel");
        }
        else
        {
            Debug.Log("[ShopSetupHelper] ✓ PurchasableItemsUI component found");
            
            if (shopUI.shopContainer == null)
                Debug.LogError("[ShopSetupHelper] ✗ ShopContainer not assigned!");
            else
                Debug.Log($"[ShopSetupHelper] ✓ ShopContainer: {shopUI.shopContainer.name}");
            
            if (shopUI.shopItemPrefab == null)
                Debug.LogError("[ShopSetupHelper] ✗ ShopItemPrefab not assigned!");
            else
                Debug.Log($"[ShopSetupHelper] ✓ ShopItemPrefab: {shopUI.shopItemPrefab.name}");
            
            if (shopUI.purchasableTowers == null || shopUI.purchasableTowers.Length == 0)
                Debug.LogWarning("[ShopSetupHelper] ⚠ PurchasableTowers array is empty! Add TowerSO assets.");
            else
                Debug.Log($"[ShopSetupHelper] ✓ PurchasableTowers: {shopUI.purchasableTowers.Length} towers");
        }
        
        Transform shopContainer = transform.Find("ShopContainer");
        if (shopContainer == null) shopContainer = transform.Find("Shop");
        if (shopContainer == null)
            Debug.LogError("[ShopSetupHelper] ✗ ShopContainer GameObject not found as child!");
        else
            Debug.Log($"[ShopSetupHelper] ✓ ShopContainer GameObject found: {shopContainer.name}");
        
        Debug.Log("[ShopSetupHelper] ===== Check Complete =====");
    }
}

*/
