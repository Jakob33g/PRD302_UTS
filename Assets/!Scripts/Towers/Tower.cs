using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("Data")]
    public TowerSO data;

    [Header("Scene Links")]
    public Transform head;        // optional: rotates to face target
    public Transform muzzle;      // where bullets spawn
    public LayerMask enemyMask;   // put your Enemy colliders on this layer (optional)

    [Header("Aim")]
    public float turnSpeed = 360f;   // deg/sec

    float fireCooldown;

    void Reset()
    {
        // Auto-find children by name (optional)
        if (!muzzle) muzzle = transform.Find("Muzzle");
        if (!head) head = transform;
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
            fireCooldown = 1f / Mathf.Max(0.01f, data.fireRate);
        }
    }

    Enemy FindNearestEnemy()
    {
        Vector3 pos = transform.position;
        float r = data.range;
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

        p.Init(target.transform, data.damage, data.bulletSpeed);
    }

    void OnDrawGizmosSelected()
    {
        if (data == null) return;
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, data.range);
    }
}