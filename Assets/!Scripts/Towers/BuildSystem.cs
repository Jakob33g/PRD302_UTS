using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class BuildSystem : MonoBehaviour
{
    [Header("Links")]
    public Inventory inventory;          // drag your Player Inventory
    public Camera cam;                   // usually Camera.main
    public LayerMask groundMask;         // include Ground layer

    [Header("Buildable blueprints")]
    public List<TowerSO> towers = new(); // put TowerSO assets here (index 0,1,2...)

    [Header("Placement")]
    public float maxPlaceRay = 200f;
    public float minDistanceFromPlayer = 1.5f;
    public Material ghostOkMat;          // semi-transparent green
    public Material ghostBadMat;         // semi-transparent red

    // runtime state
    TowerSO currentBP;
    GameObject ghost;
    Renderer[] ghostRenderers;
    Material[] ghostOrigMats;
    bool canPlace;
    Transform player; // for min distance

    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (!inventory)
        {
            var inv = FindAnyObjectByType<Inventory>();
            if (inv) inventory = inv;
        }
        var pgo = GameObject.FindGameObjectWithTag("Player");
        if (pgo) player = pgo.transform;
    }

    void Update()
    {
        // Hotkeys: 1..9 select a tower
        int select = NumberKeyDown();
        if (select >= 0 && select < towers.Count)
        {
            TryEnterBuildMode(towers[select]);
        }

        if (currentBP != null)
        {
            UpdateGhostPose();
            if (LeftClickDown() && canPlace && !PointerOverUI())
            {
                Place();
            }
            if (RightClickDown() || EscapeDown())
            {
                CancelBuild();
            }
        }
    }

    // -------- BUILD MODE --------

    void TryEnterBuildMode(TowerSO bp)
    {
        if (bp == null || bp.prefab == null)
        {
            Debug.LogWarning("BuildSystem: Blueprint missing prefab.");
            return;
        }
        EnterBuildMode(bp);
    }

    void EnterBuildMode(TowerSO bp)
    {
        currentBP = bp;
        SpawnGhost();
        Debug.Log($"Build mode: {bp.name}. Left-click to place, Right-click/Esc to cancel.");
    }

    void CancelBuild()
    {
        currentBP = null;
        DestroyGhost();
    }

    void Place()
    {
        if (currentBP == null) return;

        // Check cost
        if (currentBP.costItem != null && currentBP.costAmount > 0)
        {
            if (!inventory.Has(currentBP.costItem, currentBP.costAmount))
            {
                Debug.Log("Not enough resources.");
                return;
            }
        }

        // ---- Instantiate the REAL tower ----
        Vector3 pos = ghost.transform.position;
        Quaternion rot = ghost.transform.rotation;

        var go = Instantiate(currentBP.prefab, pos, rot);

        // >>> CRITICAL: set the TowerSO on the placed Tower <<<
        var tower = go.GetComponent<Tower>();
        if (tower != null)
        {
            tower.data = currentBP;

            // Optional: helpful warning if projectile is missing on the SO
            if (tower.data.projectilePrefab == null)
                Debug.LogWarning($"BuildSystem: '{tower.name}' has TowerSO '{tower.data.name}' but no projectilePrefab set.");
        }
        else
        {
            Debug.LogWarning("BuildSystem: Placed prefab has no Tower component. Did you put Tower on the root?");
        }

        // Deduct cost
        if (currentBP.costItem != null && currentBP.costAmount > 0)
            inventory.Remove(currentBP.costItem, currentBP.costAmount);

        // Stay in build mode (place multiple) — comment out next line to stay in build mode
        // CancelBuild();
    }

    // -------- GHOST --------
    void SpawnGhost()
    {
        DestroyGhost();

        ghost = Instantiate(currentBP.prefab);
        // disable tower behaviour & colliders on ghost
        foreach (var t in ghost.GetComponentsInChildren<Tower>(true)) t.enabled = false;
        foreach (var c in ghost.GetComponentsInChildren<Collider>(true)) c.enabled = false;
        foreach (var r in ghost.GetComponentsInChildren<Rigidbody>(true)) r.isKinematic = true;

        ghostRenderers = ghost.GetComponentsInChildren<Renderer>(true);
        ghostOrigMats = new Material[ghostRenderers.Length];
        for (int i = 0; i < ghostRenderers.Length; i++)
        {
            // store original material and set ghost mat
            ghostOrigMats[i] = ghostRenderers[i].sharedMaterial;
            if (ghostOkMat) ghostRenderers[i].sharedMaterial = ghostOkMat;
        }
    }

    void UpdateGhostPose()
    {
        if (!ghost || !cam) return;

        Ray ray = cam.ScreenPointToRay(GetMousePosition());
        LayerMask mask = groundMask.value == 0 ? Physics.DefaultRaycastLayers : groundMask;
        float dist = maxPlaceRay <= 0 ? 500f : maxPlaceRay;

        if (Physics.Raycast(ray, out RaycastHit hit, dist, mask))
        {
            ghost.transform.position = hit.point + Vector3.up * 0.01f;

            // face fixed world yaw (Don’t Starve vibe)
            Vector3 fwd = new Vector3(cam.transform.forward.x, 0f, cam.transform.forward.z).normalized;
            if (fwd.sqrMagnitude < 0.001f) fwd = Vector3.forward;
            ghost.transform.rotation = Quaternion.LookRotation(fwd);

            bool farEnough = player ? (Vector3.Distance(hit.point, player.position) >= minDistanceFromPlayer) : true;
            SetGhostValid(farEnough);
            canPlace = farEnough;
        }
        else
        {
            SetGhostValid(false);
            canPlace = false;
        }
    }

    void SetGhostValid(bool ok)
    {
        if (ghostRenderers == null) return;
        for (int i = 0; i < ghostRenderers.Length; i++)
        {
            if (ghostOkMat && ok)        ghostRenderers[i].sharedMaterial = ghostOkMat;
            else if (ghostBadMat && !ok) ghostRenderers[i].sharedMaterial = ghostBadMat;
        }
    }

    void DestroyGhost()
    {
        if (ghost != null)
        {
            Destroy(ghost);
            ghost = null;
            ghostRenderers = null;
            ghostOrigMats = null;
        }
        canPlace = false;
    }

    // -------- UTIL: INPUT / UI --------
    bool PointerOverUI()
    {
        if (EventSystem.current == null) return false;
        return EventSystem.current.IsPointerOverGameObject();
    }

    int NumberKeyDown()
    {
        // returns 0..8 for keys 1..9, or -1 for none
        #if ENABLE_INPUT_SYSTEM
        if (Keyboard.current == null) return -1;
        if (Keyboard.current.digit1Key.wasPressedThisFrame) return 0;
        if (Keyboard.current.digit2Key.wasPressedThisFrame) return 1;
        if (Keyboard.current.digit3Key.wasPressedThisFrame) return 2;
        if (Keyboard.current.digit4Key.wasPressedThisFrame) return 3;
        if (Keyboard.current.digit5Key.wasPressedThisFrame) return 4;
        if (Keyboard.current.digit6Key.wasPressedThisFrame) return 5;
        if (Keyboard.current.digit7Key.wasPressedThisFrame) return 6;
        if (Keyboard.current.digit8Key.wasPressedThisFrame) return 7;
        if (Keyboard.current.digit9Key.wasPressedThisFrame) return 8;
        #else
        if (Input.GetKeyDown(KeyCode.Alpha1)) return 0;
        if (Input.GetKeyDown(KeyCode.Alpha2)) return 1;
        if (Input.GetKeyDown(KeyCode.Alpha3)) return 2;
        if (Input.GetKeyDown(KeyCode.Alpha4)) return 3;
        if (Input.GetKeyDown(KeyCode.Alpha5)) return 4;
        if (Input.GetKeyDown(KeyCode.Alpha6)) return 5;
        if (Input.GetKeyDown(KeyCode.Alpha7)) return 6;
        if (Input.GetKeyDown(KeyCode.Alpha8)) return 7;
        if (Input.GetKeyDown(KeyCode.Alpha9)) return 8;
        #endif
        return -1;
    }

    bool LeftClickDown()
    {
        #if ENABLE_INPUT_SYSTEM
        return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        #else
        return Input.GetMouseButtonDown(0);
        #endif
    }

    bool RightClickDown()
    {
        #if ENABLE_INPUT_SYSTEM
        return Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
        #else
        return Input.GetMouseButtonDown(1);
        #endif
    }

    bool EscapeDown()
    {
        #if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        #else
        return Input.GetKeyDown(KeyCode.Escape);
        #endif
    }

    Vector2 GetMousePosition()
    {
        #if ENABLE_INPUT_SYSTEM
        return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
        #else
        return Input.mousePosition;
        #endif
    }
}