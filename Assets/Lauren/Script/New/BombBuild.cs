using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class BombBuild : MonoBehaviour
{
    public Inventory inventory;
    public Camera cam;
    public LayerMask groundMask;
    public GameObject bombPrefab;
    public float maxPlaceRay = 50f;
    public float minDistanceFromPlayer = 1.5f;
    public Material ghostOkMat;
    public Material ghostBadMat;

    GameObject ghost;
    Renderer[] ghostRenderers;
    Transform player;
    bool canPlace;

    //lauren
    public AudioSource audioSource;
    public AudioClip placeSFX;
    public AudioClip failSFX;

    void Awake()
    {
        cam = Camera.main;
        inventory = FindAnyObjectByType<Inventory>();

        var pgo = GameObject.FindGameObjectWithTag("Player");
        if (pgo) 
        {
            player = pgo.transform;
        }
    }

    void Update()
    {
        if (NumberKeyDown() == 2) //0 is indexedf
        {
            TryEnterBuildMode();
        }

        if (ghost != null)
        {
            UpdateGhostPose();

            if (LeftClickDown() && canPlace && !PointerOverUI())
            {
                PlaceBomb();
            }

            if (RightClickDown() || EscapeDown())
            {
                DestroyGhost();
            }
        }
    }

    void TryEnterBuildMode()
    {
        SpawnGhost();
    }

    void SpawnGhost()
    {
        DestroyGhost();
        ghost = Instantiate(bombPrefab);

        // disable colliders & rigidbodies for ghost
        foreach (var c in ghost.GetComponentsInChildren<Collider>(true)) c.enabled = false;
        foreach (var r in ghost.GetComponentsInChildren<Rigidbody>(true)) r.isKinematic = true;

        // disable animator & particle effects so it doesn't play at start
        Animator anim = ghost.GetComponentInChildren<Animator>();
        if (anim != null) anim.enabled = false;

        ParticleSystem[] ps = ghost.GetComponentsInChildren<ParticleSystem>();
        foreach (var p in ps) p.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ghostRenderers = ghost.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < ghostRenderers.Length; i++)
            if (ghostOkMat) ghostRenderers[i].material = ghostOkMat;

        // set position above ground using half-height
        Collider bombCol = ghost.GetComponent<Collider>();
        float yOffset = bombCol != null ? bombCol.bounds.extents.y : 0.01f;
        ghost.transform.position = ghost.transform.position + Vector3.up * yOffset;
    }

    void UpdateGhostPose()
    {
        if (ghost == null || cam == null) return;

        Ray ray = cam.ScreenPointToRay(GetMousePosition());
        LayerMask mask = groundMask.value == 0 ? Physics.DefaultRaycastLayers : groundMask;
        float dist = maxPlaceRay <= 0 ? 500f : maxPlaceRay;

        if (Physics.Raycast(ray, out RaycastHit hit, dist, mask))
        {
            ghost.transform.position = hit.point + Vector3.up * 0.01f;

            Vector3 fwd = new Vector3(cam.transform.forward.x, 0f, cam.transform.forward.z).normalized;
            if (fwd.sqrMagnitude < 0.001f) fwd = Vector3.forward;
            ghost.transform.rotation = Quaternion.LookRotation(fwd);

            canPlace = player == null || Vector3.Distance(hit.point, player.position) >= minDistanceFromPlayer;
            SetGhostValid(canPlace);
        }
        else
        {
            SetGhostValid(false);
            canPlace = false;
        }
    }

    void SetGhostValid(bool ok)
    {
        if (ghost == null || ghostRenderers == null) return;

        for (int i = 0; i < ghostRenderers.Length; i++)
        {
            if (ghostRenderers[i] == null) continue;
            ghostRenderers[i].material = ok ? ghostOkMat : ghostBadMat;
        }
    }

    void DestroyGhost()
    {
        if (ghost != null) 
        {
            Destroy(ghost);
        }

        ghost = null;
        ghostRenderers = null;
        canPlace = false;
    }

    void PlaceBomb()
    {
        if (bombPrefab == null || ghost == null) return;

        // position above ground using half-height
        Collider bombCol = bombPrefab.GetComponent<Collider>();
        float yOffset = bombCol != null ? bombCol.bounds.extents.y : 0.01f;
        Vector3 spawnPos = ghost.transform.position + Vector3.up * yOffset;

        Instantiate(bombPrefab, spawnPos, ghost.transform.rotation);

        PlayPlaceSFX();
        DestroyGhost();
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

    bool PointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    void PlayPlaceSFX()
    {
        if (audioSource && placeSFX) audioSource.PlayOneShot(placeSFX);
    }

    void PlayFailSFX()
    {
        if (audioSource && failSFX) audioSource.PlayOneShot(failSFX);
    }
}
