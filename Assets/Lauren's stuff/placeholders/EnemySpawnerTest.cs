using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnerTest : MonoBehaviour
{
    [Header("Targets")]
    public Transform player;
    public Transform[] additionalTargets;

    [Header("EnemyTest Types")]
    public EnemySO[] EnemyTestTypes;

    [Header("Spawn Ring")]
    public LayerMask groundMask;
    public float minRadius = 18f;
    public float maxRadius = 24f;
    public float spawnInterval = 1.5f;
    public int maxAlive = 20;
    public float spawnHeight = 0.12f;

    [Header("Control")]
    public bool spawningEnabled = true;

    [Header("Quota")]
    public int spawnQuotaRemaining = -1;
    public int spawnedThisWave = 0;
    public int totalSpawned = 0;

    public int AliveCount => alive.Count;

    [Header("Rewards / XP")]
    public int xpPerKill = 5;
    public PlayerXP playerXP;

    readonly List<EnemyTest> alive = new();
    readonly Dictionary<EnemySO, Queue<EnemyTest>> pool = new();

    float timer;

    void Awake()
    {
        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }

        if (!playerXP && player != null)
            playerXP = player.GetComponent<PlayerXP>();

        if (EnemyTestTypes != null)
        {
            foreach (var so in EnemyTestTypes)
                if (so != null && !pool.ContainsKey(so))
                    pool[so] = new Queue<EnemyTest>();
        }
    }

    void Update()
    {
        if (!spawningEnabled || (spawnQuotaRemaining == 0)) return;
        if ((player == null && (additionalTargets == null || additionalTargets.Length == 0)) ||
            EnemyTestTypes == null || EnemyTestTypes.Length == 0) return;

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
            Debug.LogWarning("EnemyTestSpawner: EnemyTestSO missing prefab.");
            return;
        }

        Vector3 pos = PickSpawnPosition();
        Quaternion rot = Quaternion.identity; // this shouldnt be random -Lauren

        EnemyTest EnemyTest = GetFromPool(so, pos, rot);
            EnemyTest.Init(so, BuildTargetsArray(), this); //changing
        alive.Add(EnemyTest);

        spawnedThisWave++;
        totalSpawned++;
        if (spawnQuotaRemaining > 0) spawnQuotaRemaining--;
    }

    Transform[] BuildTargetsArray()
    {
        int extraCount = 0;
        if (additionalTargets != null)
            foreach (var t in additionalTargets)
                if (t != null) extraCount++;

        int count = (player != null ? 1 : 0) + extraCount;
        Transform[] arr = new Transform[count];

        int idx = 0;
        if (player != null) arr[idx++] = player;
        if (additionalTargets != null)
            foreach (var t in additionalTargets)
                if (t != null) arr[idx++] = t;

        return arr;
    }

    Vector3 PickSpawnPosition()
    {
        Transform around = player ?? (additionalTargets != null && additionalTargets.Length > 0 ? additionalTargets[0] : null);
        Vector3 center = around != null ? around.position : Vector3.zero;

        float angle = Random.Range(0f, Mathf.PI * 2f);
        float radius = Random.Range(minRadius, maxRadius);
        Vector3 flat = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;

        Vector3 spawnPos = center + flat;
        spawnPos.y = spawnHeight;
        return spawnPos;
    }

    EnemySO PickByWeight()
    {
        float sum = 0f;
        foreach (var so in EnemyTestTypes) if (so) sum += Mathf.Max(0f, so.weight);
        if (sum <= 0f) return EnemyTestTypes[Random.Range(0, EnemyTestTypes.Length)];

        float r = Random.value * sum;
        float c = 0f;
        foreach (var so in EnemyTestTypes)
        {
            if (!so) continue;
            c += Mathf.Max(0f, so.weight);
            if (r <= c) return so;
        }
        return EnemyTestTypes[EnemyTestTypes.Length - 1];
    }

    EnemyTest GetFromPool(EnemySO so, Vector3 pos, Quaternion rot)
    {
        if (!pool.TryGetValue(so, out var q))
        {
            q = new Queue<EnemyTest>();
            pool[so] = q;
        }

        EnemyTest e = null;
        while (q.Count > 0 && e == null)
            e = q.Dequeue();

        if (e == null)
        {
            var go = Instantiate(so.prefab, pos, rot);
            e = go.GetComponent<EnemyTest>();
            if (!e) e = go.AddComponent<EnemyTest>();
        }

        Vector3 fixedPos = pos;
        fixedPos.y = spawnHeight;   // 0.12f
        e.transform.SetPositionAndRotation(fixedPos, Quaternion.identity);

        e.gameObject.SetActive(true);
        return e;
    }


    public void Despawn(EnemyTest e)
    {
        if (e == null || e.Data == null) return;

        e.gameObject.SetActive(false);
        alive.Remove(e);

        if (!pool.ContainsKey(e.Data))
            pool[e.Data] = new Queue<EnemyTest>();

        pool[e.Data].Enqueue(e);
    }

    public void ResetWaveQuota(int quota)
    {
        spawnQuotaRemaining = quota;
        spawnedThisWave = 0;
    }
}
