using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("Tower Data")]
    public TowerSO data;

    [Header("Tower Parts")]
    public Transform head;        // The part that rotates to aim (optional)
    public Transform muzzle;      // Where bullets come out
    public LayerMask enemyMask;   // Which layer enemies are on (optional)

    [Header("Aiming")]
    public float turnSpeed = 360f;   // How fast the tower turns (degrees per second)

    [Header("Skill Bonuses")]
    [Tooltip("Automatically finds the player's SkillTree to get tower bonuses")]
    public SkillTree skillTree;

    // Save tower stats so we don't calculate them every frame
    private float cachedRange;
    private float cachedFireRate;
    private int cachedDamage;
    private bool modifiersCached = false;

    float fireCooldown;

    void Reset()
    {
        // Try to find tower parts automatically by name
        if (!muzzle) muzzle = transform.Find("Muzzle");
        if (!head) head = transform;
    }

    void Awake()
    {
        // Find the player's SkillTree automatically
        if (!skillTree)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player) skillTree = player.GetComponent<SkillTree>();
        }
    }

    void OnEnable()
    {
        // Listen for skill changes so we can update tower stats
        if (skillTree != null)
        {
            skillTree.onSkillsChanged += InvalidateCache;
        }
        InvalidateCache();
    }

    void OnDisable()
    {
        // Stop listening when tower is disabled
        if (skillTree != null)
        {
            skillTree.onSkillsChanged -= InvalidateCache;
        }
    }

    void InvalidateCache()
    {
        // Mark that we need to recalculate tower stats
        modifiersCached = false;
    }

    void Update()
    {
        if (data == null || data.projectilePrefab == null) return;

        // acquire target
        Enemy target = FindNearestEnemy();
        if (target == null) return;

        // aim (Y-only)
        Vector3 to = target.transform.position - (head ? head.position : transform.position);
        to.y = 0f;
        if (to.sqrMagnitude > 0.0001f)
        {
            Quaternion want = Quaternion.LookRotation(to);
            Transform pivot = head ? head : transform;
            pivot.rotation = Quaternion.RotateTowards(pivot.rotation, want, turnSpeed * Time.deltaTime);
        }

        // fire
        fireCooldown -= Time.deltaTime;
        if (fireCooldown <= 0f)
        {
            Fire(target);
            float effectiveFireRate = GetEffectiveFireRate();
            fireCooldown = 1f / Mathf.Max(0.01f, effectiveFireRate);
        }
    }

    Enemy FindNearestEnemy()
    {
        Vector3 pos = transform.position;
        float r = GetEffectiveRange();
        Enemy nearest = null;
        float bestSqr = float.PositiveInfinity;

        // Use physics overlap (works if enemies have colliders)
        Collider[] hits = Physics.OverlapSphere(pos, r, enemyMask.value == 0 ? Physics.DefaultRaycastLayers : enemyMask);
        for (int i = 0; i < hits.Length; i++)
        {
            var e = hits[i].GetComponentInParent<Enemy>();
            if (e == null || !e.gameObject.activeInHierarchy) continue;
            Vector3 d = e.transform.position - pos; d.y = 0f;
            float sq = d.sqrMagnitude;
            if (sq < bestSqr)
            {
                bestSqr = sq;
                nearest = e;
            }
        }

        // Fallback: if no layer set or nothing hit, do a cheap search in a radius by all colliders
        if (nearest == null)
        {
            Collider[] all = Physics.OverlapSphere(pos, r);
            for (int i = 0; i < all.Length; i++)
            {
                var e = all[i].GetComponentInParent<Enemy>();
                if (e == null || !e.gameObject.activeInHierarchy) continue;
                Vector3 d = e.transform.position - pos; d.y = 0f;
                float sq = d.sqrMagnitude;
                if (sq < bestSqr) { bestSqr = sq; nearest = e; }
            }
        }
        return nearest;
    }

    void Fire(Enemy target)
    {
        if (!muzzle) muzzle = transform;

        var go = Instantiate(data.projectilePrefab, muzzle.position, muzzle.rotation);
        var p = go.GetComponent<Projectile>();
        if (!p) p = go.AddComponent<Projectile>();

        int effectiveDamage = GetEffectiveDamage();
        p.Init(target.transform, effectiveDamage, data.bulletSpeed);
    }

    void CacheModifiers()
    {
        // Calculate tower stats with skill bonuses and save them
        if (modifiersCached || data == null) return;

        float baseRange = data.range;
        float baseRate = data.fireRate;
        int baseDamage = data.damage;

        if (skillTree != null)
        {
            // Apply skill bonuses to tower stats
            cachedRange = baseRange * skillTree.GetTowerRangeModifier();
            cachedFireRate = baseRate * skillTree.GetTowerFireRateModifier();
            float damageMod = skillTree.GetTowerDamageModifier();
            cachedDamage = Mathf.RoundToInt(baseDamage * (1f + damageMod));
        }
        else
        {
            // No skills, use base stats
            cachedRange = baseRange;
            cachedFireRate = baseRate;
            cachedDamage = baseDamage;
        }

        modifiersCached = true;
    }

    float GetEffectiveRange()
    {
        // Get tower range with skill bonuses applied
        if (!modifiersCached) CacheModifiers();
        return cachedRange;
    }

    float GetEffectiveFireRate()
    {
        // Get tower fire rate with skill bonuses applied
        if (!modifiersCached) CacheModifiers();
        return cachedFireRate;
    }

    int GetEffectiveDamage()
    {
        // Get tower damage with skill bonuses applied
        if (!modifiersCached) CacheModifiers();
        return cachedDamage;
    }

    void OnDrawGizmosSelected()
    {
        // Show tower range in editor when selected
        if (data == null) return;
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.35f);
        float range = Application.isPlaying ? GetEffectiveRange() : data.range;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}