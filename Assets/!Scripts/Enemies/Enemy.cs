using UnityEngine;

public class Enemy : MonoBehaviour
{
    public EnemySO Data { get; private set; }

    // --- Targeting ---
    Transform[] potentialTargets;   // provided by spawner (player + others)
    Transform currentTarget;

    [Header("Targeting")]
    [Tooltip("How often to reconsider the nearest target (seconds).")]
    public float retargetInterval = 0.25f;

    [Tooltip("How much closer (meters) a new target must be before switching (prevents jitter).")]
    public float switchBias = 0.5f;

    float lastRetarget;

    // --- Ownership / Rewards ---
    EnemySpawner spawner;           // who spawned me (for pooling & XP sink)

    // --- Health ---
    int currentHP;

    [Header("Visual")]
    public Transform visualRoot;    // optional child to scale

    // SIGNATURE UNCHANGED
    public void Init(EnemySO so, Transform[] followTargets, EnemySpawner owner)
    {
        Data = so;
        spawner = owner;
        potentialTargets = followTargets;

        currentHP = Data.maxHealth;

        // Scale visual
        var v = visualRoot != null ? visualRoot : transform;
        v.localScale = Vector3.one * Mathf.Max(0.01f, Data.scale);

        // pick an initial target immediately
        PickNearest(force: true);

        gameObject.SetActive(true);
    }

    void Update()
    {
        if (Data == null) return;

        // periodic retarget
        if (Time.time - lastRetarget >= retargetInterval)
            PickNearest(force: false);

        if (currentTarget == null) return;

        // Move toward current target on XZ plane
        Vector3 to = currentTarget.position - transform.position;
        to.y = 0f;
        float dist = to.magnitude;

        if (dist > Data.stopDistance)
        {
            Vector3 dir = (dist > 0.0001f) ? to / dist : Vector3.zero;
            transform.position += dir * Data.moveSpeed * Time.deltaTime;

            if (dir.sqrMagnitude > 0.000001f)
            {
                Quaternion look = Quaternion.LookRotation(new Vector3(dir.x, 0f, dir.z));
                transform.rotation = look;
            }
        }
    }

    void PickNearest(bool force)
    {
        lastRetarget = Time.time;

        Transform nearest = null;
        float nearestSqr = float.PositiveInfinity;

        if (potentialTargets != null)
        {
            for (int i = 0; i < potentialTargets.Length; i++)
            {
                var t = potentialTargets[i];
                if (!IsValidTarget(t)) continue;

                Vector3 d = t.position - transform.position;
                d.y = 0f;
                float sqr = d.sqrMagnitude;

                if (sqr < nearestSqr)
                {
                    nearestSqr = sqr;
                    nearest = t;
                }
            }
        }

        if (force || currentTarget == null)
        {
            currentTarget = nearest;
            return;
        }

        if (nearest == null)
        {
            currentTarget = null;
            return;
        }

        // Only switch if the new target is meaningfully closer (switchBias)
        float biasSqr = switchBias * switchBias;
        Vector3 currD = currentTarget.position - transform.position;
        currD.y = 0f;
        float currSqr = currD.sqrMagnitude;

        if (nearest != currentTarget && nearestSqr + biasSqr < currSqr)
            currentTarget = nearest;
    }

    bool IsValidTarget(Transform t)
    {
        return t != null && t.gameObject.activeInHierarchy;
    }

    // Public so weapons can call this later
    public void TakeDamage(int dmg)
    {
        currentHP -= Mathf.Max(0, dmg);
        if (currentHP <= 0) Die();
    }

    void Die()
    {
        // Award XP through spawner (only if PlayerXP is wired)
        if (spawner != null && spawner.playerXP != null && spawner.xpPerKill > 0)
        {
            spawner.playerXP.GainXP(spawner.xpPerKill);
        }

        // Return to pool
        if (spawner) spawner.Despawn(this);
        else gameObject.SetActive(false);
    }
}