using System;
using System.Reflection;
using UnityEngine;

public class WaveDirector : MonoBehaviour
{
    [Header("Links")]
    public EnemySpawner spawner;
    public DayNightCycle cycle;   // optional; if null, we use internal timers

    [Header("Wave scaling")]
    public int   startMaxAlive       = 10;
    public float spawnIntervalStart  = 1.5f;
    public float spawnIntervalMin    = 0.4f;
    [Tooltip("Multiply spawn interval each wave (e.g., 0.9 = 10% faster).")]
    public float spawnIntervalDecay  = 0.9f;
    public int   maxAliveIncrease    = 4;

    [Header("Quota per wave (optional)")]
    [Tooltip("Enemies to spawn per wave (night). Ignored if your spawner doesn't support quotas.")]
    public int   baseQuota    = 12;
    public int   quotaPerWave = 4;
    public float quotaGrowth  = 1.0f; // 1.0 = linear; 1.1 = +10% per wave

    [Header("Fallback timers (used if no DayNightCycle)")]
    public float waveDuration      = 30f;
    public float timeBetweenWaves  = 8f;

    int wave = 0;

    void Start()
    {
        if (!spawner) spawner = FindAnyObjectByType<EnemySpawner>();
        if (!cycle)   Debug.Log("[WaveDirector] No DayNightCycle assigned; using internal wave timers.");

        if (cycle)
        {
            cycle.onNightStarted.AddListener(OnNightStarted);
            cycle.onDayStarted.AddListener(OnDayStarted);
            // Begin according to current phase
            if (cycle.IsNight) OnNightStarted();
            else               OnDayStarted();
        }
        else
        {
            // Fallback: start coroutine loop
            StartCoroutine(FallbackWaveLoop());
        }
    }

    void OnDestroy()
    {
        if (cycle)
        {
            cycle.onNightStarted.RemoveListener(OnNightStarted);
            cycle.onDayStarted.RemoveListener(OnDayStarted);
        }
    }

    void OnNightStarted()
    {
        wave++;
        ApplyWaveTuning(wave);
        EnableSpawning(true);

        // Set nightly quota if supported
        int quota = ComputeQuotaForWave(wave);
        TryResetWaveQuota(quota);

        Debug.Log($"[Waves] NIGHT start → Wave {wave}. quota={quota} interval={spawner?.spawnInterval:0.00} maxAlive={spawner?.maxAlive}");
    }

    void OnDayStarted()
    {
        EnableSpawning(false);
        Debug.Log("[Waves] DAY start → Spawning paused.");
    }

    System.Collections.IEnumerator FallbackWaveLoop()
    {
        while (true)
        {
            // Start wave
            OnNightStarted();
            float t = waveDuration;
            while (t > 0f) { t -= Time.deltaTime; yield return null; }

            // Lull
            OnDayStarted();
            t = timeBetweenWaves;
            while (t > 0f) { t -= Time.deltaTime; yield return null; }
        }
    }

    // ---------------- helpers ----------------

    void ApplyWaveTuning(int waveIndex)
    {
        if (!spawner) return;

        if (waveIndex == 1)
        {
            spawner.maxAlive      = startMaxAlive;
            spawner.spawnInterval = spawnIntervalStart;
        }
        else
        {
            spawner.maxAlive     += maxAliveIncrease;
            spawner.spawnInterval  = Mathf.Max(spawnIntervalMin, spawner.spawnInterval * spawnIntervalDecay);
        }
    }

    int ComputeQuotaForWave(int waveIndex)
    {
        float growth = Mathf.Pow(Mathf.Max(1f, quotaGrowth), Mathf.Max(0, waveIndex - 1));
        return Mathf.RoundToInt((baseQuota + quotaPerWave * Mathf.Max(0, waveIndex - 1)) * growth);
    }

    void EnableSpawning(bool enabled)
    {
        if (!spawner) return;

        // Try field: public bool spawningEnabled
        var f = spawner.GetType().GetField("spawningEnabled", BindingFlags.Instance | BindingFlags.Public);
        if (f != null && f.FieldType == typeof(bool))
        {
            f.SetValue(spawner, enabled);
            return;
        }

        // Try property: public bool spawningEnabled { get; set; }
        var p = spawner.GetType().GetProperty("spawningEnabled", BindingFlags.Instance | BindingFlags.Public);
        if (p != null && p.PropertyType == typeof(bool) && p.CanWrite)
        {
            p.SetValue(spawner, enabled);
        }
        // If neither exists, we just don't pause/resume (older spawner keeps spawning)
    }

    void TryResetWaveQuota(int quota)
    {
        if (!spawner) return;

        // public void ResetWaveQuota(int quota)
        var m = spawner.GetType().GetMethod("ResetWaveQuota", BindingFlags.Instance | BindingFlags.Public, null, new Type[]{ typeof(int) }, null);
        if (m != null)
        {
            m.Invoke(spawner, new object[] { quota });
        }
        // If method isn't there, we silently skip quotas.
    }
}