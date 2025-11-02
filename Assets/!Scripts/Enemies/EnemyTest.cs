using UnityEngine;

public class EnemyTest : MonoBehaviour
{
    public EnemySO Data { get; private set; }

    // --- Targeting ---
    Transform[] potentialTargets;
    Transform currentTarget;

    [Header("Targeting")]
    public float retargetInterval = 0.25f;
    public float switchBias = 0.5f;

    float lastRetarget;

    // --- Ownership / Rewards ---
    EnemySpawnerTest spawner;

    // --- Health ---
    int currentHP;

    [Header("Visual")]
    public Transform visualRoot;    // sprite root
    public Animator animator;       // assign Animator with mouth animation

    // --- Attack ---
    [Header("Attack")]
    public float attackDamage = 10f;
    public float attackRate = 1.0f;
    float attackCooldown;

        [Header("Debug")]
    public bool autoKill = true;        // enable/disable auto-kill
    public float autoKillTime = 6f;     // seconds
    float spawnTime;                    // track spawn time

    public void Init(EnemySO so, Transform[] followTargets, EnemySpawnerTest owner) //change for test
    {
        Data = so;
        spawner = owner;
        potentialTargets = followTargets;

        currentHP = Data.maxHealth;

        var v = visualRoot != null ? visualRoot : transform;
        v.localScale = Vector3.one * Mathf.Max(0.01f, Data.scale);

        if (animator != null) animator.Play("Idle");

        PickNearest(force: true);

        gameObject.SetActive(true);
        attackCooldown = 0f;

        spawnTime = Time.time; //TEST
    }

    void Update()
    {
        //TEST
        // --- Auto-kill debug ---
        if (autoKill && Time.time - spawnTime >= autoKillTime)
        {
            gameObject.SetActive(false);
            return; // stop further update logic
        }
        //TEST
        if (Data == null) return;

        if (Time.time - lastRetarget >= retargetInterval)
            PickNearest(force: false);

        if (currentTarget == null) return;

        // Move toward target on XZ plane
        Vector3 to = currentTarget.position - transform.position;
        to.y = 0f;
        float dist = to.magnitude;

        if (dist > Data.stopDistance)
        {
            Vector3 dir = (dist > 0.0001f) ? to / dist : Vector3.zero;
            transform.position += dir * Data.moveSpeed * Time.deltaTime;

            if (visualRoot != null) visualRoot.localRotation = Quaternion.identity;
        }
        else
        {
            TryAttackCurrentTarget();
        }

        if (attackCooldown > 0f) attackCooldown -= Time.deltaTime;
    }

    void TryAttackCurrentTarget()
    {
        if (attackCooldown > 0f || currentTarget == null) return;

        var hp = currentTarget.GetComponentInParent<Health>();
        if (hp != null) hp.TakeDamage(attackDamage);

        attackCooldown = 1f / Mathf.Max(0.01f, attackRate);
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

        float biasSqr = switchBias * switchBias;
        Vector3 currD = currentTarget.position - transform.position;
        currD.y = 0f;
        float currSqr = currD.sqrMagnitude;

        if (nearest != currentTarget && nearestSqr + biasSqr < currSqr)
            currentTarget = nearest;
    }

    bool IsValidTarget(Transform t) => t != null && t.gameObject.activeInHierarchy;

    public void TakeDamage(int dmg)
    {
        currentHP -= Mathf.Max(0, dmg);
        if (currentHP <= 0) Die();
    }

    void Die()
    {
        if (spawner != null && spawner.playerXP != null && spawner.xpPerKill > 0)
            spawner.playerXP.GainXP(spawner.xpPerKill);

    }
}
