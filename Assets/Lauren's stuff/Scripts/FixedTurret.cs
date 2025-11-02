using UnityEngine;

public class FixedTurret : MonoBehaviour
{
    public TowerSO data;

    [Header("Scene Links")]
    public Transform head;
    public Transform muzzle;
    public LayerMask enemyMask;   // what is this for???

    [Header("Sprites")]
    public Sprite headUp;
    public Sprite headDown;
    public Sprite headLeft;
    public Sprite headRight;

    private SpriteRenderer headRenderer;
    public float switchThreshold = 1f;

    float fireCooldown;

    void Start()
    {
        if (head) headRenderer = head.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (data == null || data.projectilePrefab == null) return;

        EnemyTest target = FindNearestEnemy();
        if (target == null) return;

        Vector3 to = target.transform.position - transform.position;
        Vector2 dir = new Vector2(to.x, to.z);
        UpdateHeadDirection(dir);

        fireCooldown -= Time.deltaTime;
        if (fireCooldown <= 0f)
        {
            Fire(target);
            fireCooldown = 1f / Mathf.Max(0.01f, data.fireRate);
        }
    }

    void UpdateHeadDirection(Vector2 dir)
    {
        if (dir.sqrMagnitude < 0.0001f) return;
        dir.Normalize();

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y) + switchThreshold) // renderer changes by Lauren
        {
            if (dir.x > 0)
                headRenderer.sprite = headRight;
            else
                headRenderer.sprite = headLeft;
        }
        else
        {
            if (dir.y > 0)
                headRenderer.sprite = headUp;
            else
                headRenderer.sprite = headDown;
        }
    }

    EnemyTest FindNearestEnemy()
    {
            Vector3 pos = transform.position;
            float r = data.range;
            EnemyTest nearest = null;
            float bestSqr = float.PositiveInfinity;

            Collider[] hits = Physics.OverlapSphere(pos, r, enemyMask.value == 0 ? Physics.DefaultRaycastLayers : enemyMask);
        for (int i = 0; i < hits.Length; i++)
        {
            var e = hits[i].GetComponentInParent<EnemyTest>();
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

    void Fire(EnemyTest target)
    {
        if (!muzzle) muzzle = transform;

        var go = Instantiate(data.projectilePrefab, muzzle.position, Quaternion.identity);
        var p = go.GetComponent<Projectile>();
        if (!p) p = go.AddComponent<Projectile>();

        p.Init(target.transform, data.damage, data.bulletSpeed);
    }

    void OnDrawGizmosSelected()
    {
        if (data == null) return;
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, data.range);
    }
}
