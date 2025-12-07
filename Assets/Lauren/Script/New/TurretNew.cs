using UnityEngine;

public class TurretNew : MonoBehaviour
{
    [Header("Tower Data")]
    public TowerSO data;

    public SpriteRenderer spriteRenderer;
    public Sprite upSprite;
    public Sprite downSprite;
    public Sprite leftSprite;
    public Sprite rightSprite;

    public Transform anchorUp;
    public Transform anchorDown;
    public Transform anchorLeft;
    public Transform anchorRight;
    public SkillTree skillTree;
    private float cachedRange;
    private float cachedFireRate;
    private int cachedDamage;
    private bool modifiersCached = false;

    private float fireCooldown;
    public AudioSource audioSource;
    public AudioClip fireSFX;

    void Awake()
    {
        if (!skillTree)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player) skillTree = player.GetComponent<SkillTree>();
        }

        if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        if (skillTree != null)
            skillTree.onSkillsChanged += InvalidateCache;
        InvalidateCache();
    }

    void OnDisable()
    {
        if (skillTree != null)
            skillTree.onSkillsChanged -= InvalidateCache;
    }

    void InvalidateCache()
    {
        modifiersCached = false;
    }

    void Update()
    {
        if (data == null || data.projectilePrefab == null) return;

        Enemy target = FindNearestEnemy();
        if (target == null) return;

        Transform selectedAnchor = null;
        Vector3 dir = target.transform.position - transform.position;

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.z))
        {
            if (dir.x > 0)
            {
                spriteRenderer.sprite = rightSprite;
                selectedAnchor = anchorRight;
            }
            else
            {
                spriteRenderer.sprite = leftSprite;
                selectedAnchor = anchorLeft;
            }
        }
        else
        {
            if (dir.z > 0)
            {
                spriteRenderer.sprite = upSprite;
                selectedAnchor = anchorUp;
            }
            else
            {
                spriteRenderer.sprite = downSprite;
                selectedAnchor = anchorDown;
            }
        }

        // Fire
        fireCooldown -= Time.deltaTime;
        if (fireCooldown <= 0f && selectedAnchor != null)
        {
            Fire(selectedAnchor, target);
            float effectiveFireRate = GetEffectiveFireRate();
            fireCooldown = 1f / Mathf.Max(0.01f, effectiveFireRate);
        }
    }

    void Fire(Transform muzzle, Enemy target)
    {
        if (!muzzle) return;

        var go = Instantiate(data.projectilePrefab, muzzle.position, Quaternion.identity);
        var p = go.GetComponent<Projectile>();
        if (!p) p = go.AddComponent<Projectile>();

        int effectiveDamage = GetEffectiveDamage();
        p.Init(target.transform, effectiveDamage, data.bulletSpeed);
        audioSource.PlayOneShot(fireSFX);
    }

    Enemy FindNearestEnemy()
    {
        Vector3 pos = transform.position;
        float r = GetEffectiveRange();
        Enemy nearest = null;
        float bestSqr = float.PositiveInfinity;

        Collider[] hits = Physics.OverlapSphere(pos, r);
        foreach (var hit in hits)
        {
            var e = hit.GetComponentInParent<Enemy>();
            if (e == null || !e.gameObject.activeInHierarchy) continue;
            Vector3 d = e.transform.position - pos; d.y = 0f;
            float sq = d.sqrMagnitude;
            if (sq < bestSqr)
            {
                bestSqr = sq;
                nearest = e;
            }
        }
        return nearest;
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
