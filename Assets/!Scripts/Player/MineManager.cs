using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MineManager : MonoBehaviour
{
    [Header("Mine Settings")]
    public GameObject minePrefab;
    public int maxMines = 5;
    public float placementCooldown = 2f;
    public LayerMask groundMask = -1;
    public float placementRayDistance = 100f;

    [Header("Input")]
    public KeyCode placeMineKey = KeyCode.B;

    [Header("Links")]
    public SkillTree skillTree;
    public Inventory inventory;
    public ItemSO mineItem;
    public Camera cam;
    public Transform player;

    private List<GameObject> activeMines = new List<GameObject>();
    private float lastPlaceTime = 0f;
    private SkillSO mineSkill;

    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (!skillTree) skillTree = FindAnyObjectByType<SkillTree>();
        if (!inventory) inventory = FindAnyObjectByType<Inventory>();
        if (!player)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj) player = playerObj.transform;
        }

        Debug.Log($"[MineManager] Awake() - Camera: {(cam != null ? "found" : "null")}, SkillTree: {(skillTree != null ? "found" : "null")}, Inventory: {(inventory != null ? "found" : "null")}, Player: {(player != null ? "found" : "null")}");

        // Find the mine skill
        if (skillTree != null && skillTree.allSkills != null)
        {
            foreach (var skill in skillTree.allSkills)
            {
                if (skill != null && skill.skillType == SkillType.Mine)
                {
                    mineSkill = skill;
                    Debug.Log($"[MineManager] Found mine skill: {mineSkill.skillName}");
                    break;
                }
            }
        }
        
        if (mineSkill == null)
        {
            Debug.LogWarning("[MineManager] No mine skill found! Make sure you have a skill with SkillType.Mine in SkillTree.allSkills.");
        }
        
        if (mineItem == null)
        {
            Debug.LogWarning("[MineManager] MineItem is not assigned! You need to drag Item_Mine from Assets/Data/Items/ into the MineItem field in Inspector.");
        }
        
        if (minePrefab == null)
        {
            Debug.LogWarning("[MineManager] MinePrefab is not assigned! You need to create a mine prefab and assign it in Inspector.");
            Debug.LogWarning("[MineManager] To create a mine prefab:");
            Debug.LogWarning("[MineManager] 1. Create a GameObject (e.g., a small sphere or cube)");
            Debug.LogWarning("[MineManager] 2. Add a Collider component (set as Trigger)");
            Debug.LogWarning("[MineManager] 3. Add the Mine script component");
            Debug.LogWarning("[MineManager] 4. Set damage and explosion radius in Mine component");
            Debug.LogWarning("[MineManager] 5. Save as prefab and drag it into MineManager's MinePrefab field");
        }
        else
        {
            // Check if prefab has Mine component
            Mine prefabMine = minePrefab.GetComponent<Mine>();
            if (prefabMine == null)
            {
                Debug.LogError("[MineManager] MinePrefab is assigned but missing Mine script component! The prefab must have a Mine component.");
            }
            else
            {
                Debug.Log($"[MineManager] MinePrefab OK - has Mine component with damage: {prefabMine.damage}, radius: {prefabMine.explosionRadius}");
            }
        }
    }

    void Update()
    {
        // Check if mine skill is unlocked
        if (mineSkill == null)
        {
            // Try to find the mine skill again (in case it wasn't found in Awake)
            if (skillTree != null && skillTree.allSkills != null)
            {
                foreach (var skill in skillTree.allSkills)
                {
                    if (skill != null && skill.skillType == SkillType.Mine)
                    {
                        mineSkill = skill;
                        Debug.Log("[MineManager] Found mine skill!");
                        break;
                    }
                }
            }
            if (mineSkill == null)
            {
                return; // Still no mine skill found
            }
        }

        if (skillTree == null || !skillTree.IsSkillUnlocked(mineSkill))
        {
            return;
        }

        // Don't place mines if clicking on UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        // Check if player wants to place a mine
        if (Input.GetKeyDown(placeMineKey))
        {
            Debug.Log("[MineManager] B key pressed! Attempting to place mine...");
            TryPlaceMine();
        }
    }

    void TryPlaceMine()
    {
        Debug.Log("[MineManager] ===== TryPlaceMine() called =====");
        
        // Check cooldown
        if (Time.time - lastPlaceTime < placementCooldown)
        {
            Debug.Log($"[MineManager] Cooldown active. Wait {placementCooldown - (Time.time - lastPlaceTime):F1}s");
            return;
        }

        // Check if player has mines in inventory
        if (inventory != null && mineItem != null)
        {
            bool hasMines = inventory.Has(mineItem, 1);
            Debug.Log($"[MineManager] Inventory check - Has mines: {hasMines}, Inventory: {(inventory != null ? "found" : "null")}, MineItem: {(mineItem != null ? mineItem.name : "null")}");
            
            if (!hasMines)
            {
                Debug.LogWarning("[MineManager] No mines in inventory! Unlock the skill to get mines, or assign mineItem in Inspector.");
                return;
            }
        }
        else
        {
            Debug.LogWarning($"[MineManager] Inventory or mineItem not assigned! Inventory: {(inventory != null ? "found" : "null")}, MineItem: {(mineItem != null ? mineItem.name : "null")}");
            Debug.LogWarning("[MineManager] You need to assign 'mineItem' in the MineManager component!");
            return;
        }

        // Check if we've reached max mines
        CleanupDestroyedMines();
        if (activeMines.Count >= maxMines)
        {
            Debug.Log($"[MineManager] Max mines ({maxMines}) reached. Remove old mines first.");
            return;
        }

        // Raycast from camera to ground
        if (cam == null)
        {
            Debug.LogError("[MineManager] Camera is null! Trying to find Camera.main...");
            cam = Camera.main;
            if (cam == null)
            {
                Debug.LogError("[MineManager] Camera.main is also null!");
                return;
            }
        }

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        LayerMask mask = (groundMask.value == 0) ? Physics.DefaultRaycastLayers : groundMask;
        
        Debug.Log($"[MineManager] Raycasting from camera. Mouse position: {Input.mousePosition}, Ground mask: {mask.value}");

        if (Physics.Raycast(ray, out RaycastHit hit, placementRayDistance, mask))
        {
            Debug.Log($"[MineManager] Raycast hit: {hit.collider.name} at {hit.point}");
            
            // Check minimum distance from player
            if (player != null)
            {
                float distToPlayer = Vector3.Distance(hit.point, player.position);
                Debug.Log($"[MineManager] Distance to player: {distToPlayer:F2}m");
                if (distToPlayer < 2f)
                {
                    Debug.Log("[MineManager] Too close to player! Need to be at least 2m away.");
                    return;
                }
            }

            PlaceMine(hit.point);
        }
        else
        {
            Debug.LogWarning($"[MineManager] Could not find ground to place mine on! Raycast distance: {placementRayDistance}, Mask: {mask.value}");
            Debug.LogWarning("[MineManager] Make sure your ground has a collider and is on the correct layer!");
        }
    }

    void PlaceMine(Vector3 position)
    {
        if (minePrefab == null)
        {
            Debug.LogError("[MineManager] Mine prefab is not assigned! Please drag your mine prefab (with Mine script) into the MinePrefab field in Inspector.");
            return;
        }

        // Verify the prefab has the Mine component
        Mine mineComponent = minePrefab.GetComponent<Mine>();
        if (mineComponent == null)
        {
            Debug.LogError("[MineManager] Mine prefab is missing the Mine script component! The prefab must have a Mine component.");
            return;
        }

        // Remove mine from inventory if using inventory system
        if (inventory != null && mineItem != null)
        {
            if (!inventory.Remove(mineItem, 1))
            {
                Debug.LogError("[MineManager] Failed to remove mine from inventory!");
                return;
            }
        }

        // Instantiate the mine prefab
        GameObject mine = Instantiate(minePrefab, position, Quaternion.identity);
        
        // Verify the instantiated mine has the Mine component
        Mine instantiatedMine = mine.GetComponent<Mine>();
        if (instantiatedMine == null)
        {
            Debug.LogError("[MineManager] Instantiated mine is missing Mine component! This shouldn't happen.");
            Destroy(mine);
            return;
        }
        
        // Verify it has a collider (required by Mine script)
        Collider col = mine.GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogWarning("[MineManager] Mine prefab is missing a Collider! Adding a SphereCollider...");
            SphereCollider sphereCol = mine.AddComponent<SphereCollider>();
            sphereCol.isTrigger = true;
            sphereCol.radius = 0.5f;
        }
        else
        {
            // Make sure collider is a trigger
            col.isTrigger = true;
        }
        
        activeMines.Add(mine);
        lastPlaceTime = Time.time;

        Debug.Log($"[MineManager] Successfully placed mine at {position}. Total mines: {activeMines.Count}/{maxMines}");
        Debug.Log($"[MineManager] Mine component: {(instantiatedMine != null ? "OK" : "MISSING")}, Collider: {(mine.GetComponent<Collider>() != null ? "OK" : "MISSING")}");
    }

    void CleanupDestroyedMines()
    {
        activeMines.RemoveAll(m => m == null);
    }

    public void ClearAllMines()
    {
        foreach (GameObject mine in activeMines)
        {
            if (mine != null)
                Destroy(mine);
        }
        activeMines.Clear();
    }
}

