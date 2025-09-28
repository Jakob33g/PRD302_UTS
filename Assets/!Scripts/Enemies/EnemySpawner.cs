using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Targets")]
    public Transform player;                 // main target
    [Tooltip("Other things enemies can attack (e.g., Base, Turret).")]
    public Transform[] additionalTargets;    // drag any number of extras here

    [Header("Enemy Types")]
    public EnemySO[] enemyTypes;             // ScriptableObject types to spawn

    [Header("Spawn Ring")]
    public LayerMask groundMask;             // include your Ground layer
    public float minRadius = 18f;
    public float maxRadius = 24f;
    public float spawnInterval = 1.5f;
    public int   maxAlive     = 20;

    [Header("Y placement")]
    public float spawnCastHeight = 20f;      // ray down from above
    public float spawnUpOffset   = 0.05f;

    [Header("Rewards / XP")]
    [Tooltip("XP given to the player for each enemy killed (kept here to avoid changing EnemySO).")]
    public int xpPerKill = 5;
    [Tooltip("Who receives XP. If not assigned, we'll try to read PlayerXP from 'player'.")]
    public PlayerXP playerXP;

    // --- internals ---
    readonly List<Enemy> alive = new();
    readonly Dictionary<EnemySO, Queue<Enemy>> pool = new();
    float timer;

    void Awake()
    {
        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }

        // Auto-wire PlayerXP if possible (safe if PlayerXP not present)
        if (!playerXP && player != null)
            playerXP = player.GetComponent<PlayerXP>();

        if (enemyTypes != null)
        {
            foreach (var so in enemyTypes)
            {
                if (so != null && !pool.ContainsKey(so))
                    pool[so] = new Queue<Enemy>();
            }
        }
    }

    void Update()
    {
        if ((player == null && (additionalTargets == null || additionalTargets.Length == 0)) ||
            enemyTypes == null || enemyTypes.Length == 0)
            return;

        timer -= Time.deltaTime;
        if (timer <= 0f && alive.Count < maxAlive)
        {
            SpawnOne();
            timer = spawnInterval;
        }
    }

    void SpawnOne()
    {
        var so = PickByWeight();
        if (so == null || so.prefab == null)
        {
            Debug.LogWarning("EnemySpawner: EnemySO missing prefab.");
            return;
        }

        Vector3 pos = PickSpawnPosition();
        Quaternion rot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        Enemy enemy = GetFromPool(so, pos, rot);
        // NOTE: Enemy.Init signature unchanged
        enemy.Init(so, BuildTargetsArray(), this);
        alive.Add(enemy);
    }

    Transform[] BuildTargetsArray()
    {
        // Build a compact array: [player?, extras...], skipping nulls
        int extraCount = 0;
        if (additionalTargets != null)
        {
            for (int i = 0; i < additionalTargets.Length; i++)
                if (additionalTargets[i] != null) extraCount++;
        }

        int count = (player != null ? 1 : 0) + extraCount;
        var arr = new Transform[count];

        int idx = 0;
        if (player != null) arr[idx++] = player;

        if (additionalTargets != null)
        {
            for (int i = 0; i < additionalTargets.Length; i++)
                if (additionalTargets[i] != null)
                    arr[idx++] = additionalTargets[i];
        }

        return arr;
    }

    Vector3 PickSpawnPosition()
    {
        // Pick a ring point around the player if available, else around the first extra
        Vector3 center = Vector3.zero;
        Transform around = player;
        if (around == null && additionalTargets != null)
        {
            for (int i = 0; i < additionalTargets.Length && around == null; i++)
                if (additionalTargets[i] != null) around = additionalTargets[i];
        }
        if (around != null) center = around.position;

        float angle = Random.Range(0f, Mathf.PI * 2f);
        float radius = Random.Range(minRadius, maxRadius);
        Vector3 flat = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
        Vector3 start = center + flat + Vector3.up * spawnCastHeight;

        // Raycast down to ground
        LayerMask mask = groundMask.value == 0 ? Physics.DefaultRaycastLayers : groundMask;
        if (Physics.Raycast(start, Vector3.down, out RaycastHit hit, spawnCastHeight * 2f, mask))
            return hit.point + Vector3.up * spawnUpOffset;

        // Fallback: keep player's Y if no ground hit
        float y = (player != null) ? player.position.y : start.y;
        return new Vector3(start.x, y + spawnUpOffset, start.z);
    }

    EnemySO PickByWeight()
    {
        float sum = 0f;
        foreach (var so in enemyTypes) if (so) sum += Mathf.Max(0f, so.weight);
        if (sum <= 0f) return enemyTypes[Random.Range(0, enemyTypes.Length)];

        float r = Random.value * sum;
        float c = 0f;
        foreach (var so in enemyTypes)
        {
            if (!so) continue;
            c += Mathf.Max(0f, so.weight);
            if (r <= c) return so;
        }
        return enemyTypes[enemyTypes.Length - 1];
    }

    Enemy GetFromPool(EnemySO so, Vector3 pos, Quaternion rot)
    {
        if (!pool.TryGetValue(so, out var q))
        {
            q = new Queue<Enemy>();
            pool[so] = q;
        }

        Enemy e = null;
        while (q.Count > 0 && e == null)
            e = q.Dequeue();

        if (e == null)
        {
            var go = Instantiate(so.prefab, pos, rot);
            e = go.GetComponent<Enemy>();
            if (!e) e = go.AddComponent<Enemy>();
        }

        e.transform.SetPositionAndRotation(pos, rot);
        e.gameObject.SetActive(true);
        return e;
    }

    public void Despawn(Enemy e)
    {
        if (e == null || e.Data == null) return;

        e.gameObject.SetActive(false);
        alive.Remove(e);

        if (!pool.ContainsKey(e.Data))
            pool[e.Data] = new Queue<Enemy>();

        pool[e.Data].Enqueue(e);
    }
}