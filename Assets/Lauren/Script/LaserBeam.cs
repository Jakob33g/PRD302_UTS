using UnityEngine;

public class LaserBeam : MonoBehaviour
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
    public LineRenderer laserRenderer;
    //laser arc
    public int segments = 12;
    public float arcStrength = 0.4f;
    public float zigzag = 0.3f;
    public float noiseSpeed = 20f;
    public float damagePerSecond = 10f;
    public SkillTree skillTree;
    private float cachedRange;
    private float cachedFireRate;
    private int cachedDamage;
    private bool modifiersCached = false;
    public AudioSource audioSource;
    public AudioClip fireSFX;
    private float fireCooldown;
    private Enemy target;
    private Transform currentAnchor;

    void Awake()
    {
        if (!skillTree)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player) skillTree = player.GetComponent<SkillTree>();
        }

        if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
        if (laserRenderer) laserRenderer.enabled = false;
    }

    void OnEnable()
    {
        if (skillTree != null)
        {
            skillTree.onSkillsChanged += InvalidateCache;
        }

        InvalidateCache();
    }

    void OnDisable()
    {
        if (skillTree != null)
        {
            skillTree.onSkillsChanged -= InvalidateCache;
        }
    }

    void InvalidateCache()
    {
        modifiersCached = false;
    }

    void Update()
    {
        if (data == null) return;

        target = FindNearestEnemy();

        if (target == null)
        {
            if (laserRenderer) laserRenderer.enabled = false;
            return;
        }

        Vector3 dir = target.transform.position - transform.position;

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.z))
        {
            if (dir.x > 0)
            {
                spriteRenderer.sprite = rightSprite;
                currentAnchor = anchorRight;
            }
            else
            {
                spriteRenderer.sprite = leftSprite;
                currentAnchor = anchorLeft;
            }
        }
        else
        {
            if (dir.z > 0)
            {
                spriteRenderer.sprite = upSprite;
                currentAnchor = anchorUp;
            }
            else
            {
                spriteRenderer.sprite = downSprite;
                currentAnchor = anchorDown;
            }
        }

        fireCooldown -= Time.deltaTime;
        if (fireCooldown <= 0f && currentAnchor != null)
        {
            audioSource.PlayOneShot(fireSFX);
            fireCooldown = 1f / Mathf.Max(0.01f, GetEffectiveFireRate());

            if (laserRenderer)
            {
                laserRenderer.enabled = true;
                DrawLaser(currentAnchor.position, target.transform.position);
                target.TakeDamage(GetEffectiveDamage());
                Invoke("HideLaser", 0.1f);
            }
        }
    }

    void DrawLaser(Vector3 start, Vector3 end)
    {
        if (!laserRenderer) return;

        laserRenderer.positionCount = segments + 1;

        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            Vector3 pos = Vector3.Lerp(start, end, t);

            if (i != 0 && i != segments)
            {
                Vector3 direction = (end - start);
                Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized;

                float curve = Mathf.Sin(t * Mathf.PI) * arcStrength;
                float noise = (Random.value - 0.5f) * zigzag;

                pos += perpendicular * (curve + noise);
            }

            laserRenderer.SetPosition(i, pos);
        }
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
        if (modifiersCached || data == null) return;

        float baseRange = data.range;
        float baseRate = data.fireRate;
        int baseDamage = data.damage;

        if (skillTree != null)
        {
            cachedRange = baseRange * skillTree.GetTowerRangeModifier();
            cachedFireRate = baseRate * skillTree.GetTowerFireRateModifier();
            float damageMod = skillTree.GetTowerDamageModifier();
            cachedDamage = Mathf.RoundToInt(baseDamage * (1f + damageMod));
        }
        else
        {
            cachedRange = baseRange;
            cachedFireRate = baseRate;
            cachedDamage = baseDamage;
        }

        modifiersCached = true;
    }

    void HideLaser()
    {
        if (laserRenderer) laserRenderer.enabled = false;
    }

    float GetEffectiveRange()
    {
        if (!modifiersCached) CacheModifiers();
        return cachedRange;
    }

    float GetEffectiveFireRate()
    {
        if (!modifiersCached) CacheModifiers();
        return cachedFireRate;
    }

    int GetEffectiveDamage()
    {
        if (!modifiersCached) CacheModifiers();
        return cachedDamage;
    }

    void OnDrawGizmosSelected()
    {
        if (data == null) return;
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.35f);
        float range = Application.isPlaying ? GetEffectiveRange() : data.range;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}