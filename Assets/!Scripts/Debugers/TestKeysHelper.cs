using UnityEngine;
/*
/// <summary>
/// Helper script to ensure test keys (Z, X, C) work properly.
/// Add this to any GameObject in your scene (or the Player) to enable test keys.
/// </summary>
public class TestKeysHelper : MonoBehaviour
{
    [Header("Auto-Find Components")]
    public PlayerXP playerXP;
    public Inventory inventory;
    public InvDebugAdder invDebugAdder;

    [Header("Test Keys")]
    [Tooltip("Press Z to gain XP")]
    public KeyCode xpKey = KeyCode.Z;
    [Tooltip("Press X to add wood")]
    public KeyCode woodKey = KeyCode.X;
    [Tooltip("Press C to add stone")]
    public KeyCode stoneKey = KeyCode.C;
    [Tooltip("Press V to add mine")]
    public KeyCode mineKey = KeyCode.V;

    [Header("XP Test")]
    public int testXPAmount = 50;

    [Header("Item Test Amounts")]
    public int woodAmount = 5;
    public int stoneAmount = 3;
    public int mineAmount = 1;

    void Awake()
    {
        // Ensure this component is always enabled
        this.enabled = true;
        this.gameObject.SetActive(true);
    }

    void Start()
    {
        // Auto-find components if not assigned
        if (!playerXP) playerXP = FindAnyObjectByType<PlayerXP>();
        if (!inventory) inventory = FindAnyObjectByType<Inventory>();
        if (!invDebugAdder) invDebugAdder = FindAnyObjectByType<InvDebugAdder>();

        // Ensure InvDebugAdder is enabled if it exists
        if (invDebugAdder != null)
        {
            invDebugAdder.enabled = true;
            invDebugAdder.gameObject.SetActive(true);
            Debug.Log("[TestKeysHelper] Found and enabled InvDebugAdder");
        }
        else
        {
            Debug.LogWarning("[TestKeysHelper] InvDebugAdder not found! Creating one...");
            GameObject helper = new GameObject("InvDebugAdder_Helper");
            invDebugAdder = helper.AddComponent<InvDebugAdder>();
        }

        // Ensure PlayerXP is enabled
        if (playerXP != null)
        {
            playerXP.enabled = true;
            playerXP.gameObject.SetActive(true);
            Debug.Log("[TestKeysHelper] Found PlayerXP component");
        }
        else
        {
            Debug.LogError("[TestKeysHelper] PlayerXP not found! Test keys may not work.");
        }

        Debug.Log("[TestKeysHelper] Test keys enabled:");
        Debug.Log($"  - {xpKey}: Gain {testXPAmount} XP");
        Debug.Log($"  - {woodKey}: Add {woodAmount}x Wood");
        Debug.Log($"  - {stoneKey}: Add {stoneAmount}x Stone");
        Debug.Log($"  - {mineKey}: Add {mineAmount}x Mine");
    }

    void Update()
    {
        // Ensure components are still enabled
        if (playerXP != null && !playerXP.enabled)
        {
            playerXP.enabled = true;
            Debug.LogWarning("[TestKeysHelper] Re-enabled PlayerXP component!");
        }

        if (invDebugAdder != null && !invDebugAdder.enabled)
        {
            invDebugAdder.enabled = true;
            Debug.LogWarning("[TestKeysHelper] Re-enabled InvDebugAdder component!");
        }

        // XP test key (Z)
        if (Input.GetKeyDown(xpKey))
        {
            if (playerXP != null)
            {
                playerXP.GainXP(testXPAmount);
                Debug.Log($"[TestKeysHelper] TEST: Gained {testXPAmount} XP (Level: {playerXP.level}, SP: {playerXP.unspentSkillPoints})");
            }
            else
            {
                Debug.LogError("[TestKeysHelper] Cannot add XP - PlayerXP not found!");
            }
        }

        // Let InvDebugAdder handle X, C, V keys if it exists
        // But also provide fallback here
        if (invDebugAdder == null || !invDebugAdder.enabled)
        {
            if (Input.GetKeyDown(woodKey) && inventory != null)
            {
                var woodItem = FindItemByName("Wood");
                if (woodItem != null)
                {
                    inventory.AddItem(woodItem, woodAmount);
                    Debug.Log($"[TestKeysHelper] Added {woodAmount}x {woodItem.itemName}");
                }
                else
                {
                    Debug.LogWarning("[TestKeysHelper] Wood item not found!");
                }
            }

            if (Input.GetKeyDown(stoneKey) && inventory != null)
            {
                var stoneItem = FindItemByName("Stone");
                if (stoneItem != null)
                {
                    inventory.AddItem(stoneItem, stoneAmount);
                    Debug.Log($"[TestKeysHelper] Added {stoneAmount}x {stoneItem.itemName}");
                }
                else
                {
                    Debug.LogWarning("[TestKeysHelper] Stone item not found!");
                }
            }

            if (Input.GetKeyDown(mineKey) && inventory != null)
            {
                var mineItem = FindItemByName("Mine") ?? FindItemByName("Land Mine");
                if (mineItem != null)
                {
                    inventory.AddItem(mineItem, mineAmount);
                    Debug.Log($"[TestKeysHelper] Added {mineAmount}x {mineItem.itemName}");
                }
                else
                {
                    Debug.LogWarning("[TestKeysHelper] Mine item not found!");
                }
            }
        }
    }

    ItemSO FindItemByName(string name)
    {
        var allItems = Resources.FindObjectsOfTypeAll<ItemSO>();
        foreach (var item in allItems)
        {
            if (item != null && (item.name == name || item.itemName == name))
                return item;
        }
        return null;
    }
}
*/

