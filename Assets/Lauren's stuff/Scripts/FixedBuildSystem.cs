using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class FixedBuildSystem : MonoBehaviour
{
    [Header("Links")]
    public ResourceTest inv;
    public Camera cam;
    public LayerMask groundMask;
    public LayerMask obstacleMask; // theyre on 1 -Lauren

    [Header("Buildable blueprints")]
    public List<TowerSO> towers = new();

    [Header("Placement")]
    public float maxPlaceRay = 200f;
    public float minDistanceFromPlayer = 1.5f;
    public Material ghostOkMat;
    public Material ghostBadMat;

    TowerSO currentBP;
    GameObject ghost;
    Renderer[] ghostRenderers;
    Material[] ghostOrigMats;
    bool canPlace;
    Transform player;

    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (!inv)
            inv = FindAnyObjectByType<ResourceTest>();

        var pgo = GameObject.FindGameObjectWithTag("Player");
        if (pgo) player = pgo.transform;
    }

    void Update()
    {
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
    void TryEnterBuildMode(TowerSO bp)
    {
        if (bp == null || bp.prefab == null)
        {
            return;
        }
        EnterBuildMode(bp);
    }

    void EnterBuildMode(TowerSO bp)
    {
        currentBP = bp;
        SpawnGhost();
    }

    void CancelBuild()
    {
        currentBP = null;
        DestroyGhost();
    }

    void Place()
    {
        if (currentBP == null) return;

        if (currentBP.costItem != null && currentBP.costAmount > 0)
        {
            if (!inv.Has(currentBP.costItem, currentBP.costAmount))
            {
                Debug.Log("Not enough resources."); // why is this debug??
                return;
            }
        }

        Vector3 pos = ghost.transform.position;
        Quaternion rot = ghost.transform.rotation;
        var go = Instantiate(currentBP.prefab, pos, rot);

        var tower = go.GetComponent<Tower>();
        if (tower != null)
        {
            tower.data = currentBP;
        }

        if (currentBP.costItem != null && currentBP.costAmount > 0)
            inv.Remove(currentBP.costItem, currentBP.costAmount);

        CancelBuild();
    }

    void SpawnGhost()
    {
        DestroyGhost();

        ghost = Instantiate(currentBP.prefab);
        foreach (var t in ghost.GetComponentsInChildren<Tower>(true)) t.enabled = false;
        foreach (var c in ghost.GetComponentsInChildren<Collider>(true)) c.enabled = false;
        foreach (var r in ghost.GetComponentsInChildren<Rigidbody>(true)) r.isKinematic = true;

        ghostRenderers = ghost.GetComponentsInChildren<Renderer>(true);
        ghostOrigMats = new Material[ghostRenderers.Length];
        for (int i = 0; i < ghostRenderers.Length; i++)
        {
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
            //ghost.transform.position = hit.point + Vector3.up * 0.01f; //this seems to clip -Lauren
            float minY = 0.12f;
            Vector3 pos = hit.point;
            if (pos.y < minY) pos.y = minY;
            ghost.transform.position = pos;

            Vector3 fwd = new Vector3(cam.transform.forward.x, 0f, cam.transform.forward.z).normalized;
            if (fwd.sqrMagnitude < 0.001f) fwd = Vector3.forward;
            ghost.transform.rotation = Quaternion.LookRotation(fwd);

            bool farEnough = player ? (Vector3.Distance(hit.point, player.position) >= minDistanceFromPlayer) : true;
            bool noOverlap = !Physics.CheckBox(
                ghost.transform.position,
                ghost.GetComponentInChildren<Renderer>().bounds.extents * 0.9f,
                Quaternion.identity,
                obstacleMask
            );

            bool valid = farEnough && noOverlap;
            SetGhostValid(valid);
            canPlace = valid;
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
            if (ghostOkMat && ok) ghostRenderers[i].sharedMaterial = ghostOkMat;
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
    bool PointerOverUI()
    {
        if (EventSystem.current == null) return false;
        return EventSystem.current.IsPointerOverGameObject();
    }

    int NumberKeyDown()
    {
        #if ENABLE_INPUT_SYSTEM
        if (Keyboard.current == null) return -1;
        if (Keyboard.current.digit1Key.wasPressedThisFrame) return 0;
        if (Keyboard.current.digit2Key.wasPressedThisFrame) return 1;
        if (Keyboard.current.digit3Key.wasPressedThisFrame) return 2;
        #else
        if (Input.GetKeyDown(KeyCode.Alpha1)) return 0;
        if (Input.GetKeyDown(KeyCode.Alpha2)) return 1;
        if (Input.GetKeyDown(KeyCode.Alpha3)) return 2;
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
