using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;

public class WaveHUD : MonoBehaviour
{
    [Header("Links (assign if you want; auto-find will try)")]
    public EnemySpawner spawner;
    public WaveDirector director;
    public DayNightCycle cycle;

    [Header("UI")]
    public TextMeshProUGUI phaseText;
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI enemiesLeftText;
    public TextMeshProUGUI countdownText;

    // Reflection cache (so we don’t look up every frame)
    FieldInfo  fiSpawnerSpawningEnabled;
    MethodInfo miSpawnerResetWaveQuota;
    PropertyInfo piSpawnerAliveCount;
    FieldInfo fiSpawnerAliveList;           // private List<Enemy> alive
    FieldInfo fiSpawnerQuotaField;          // int spawnQuotaRemaining (field)
    PropertyInfo piSpawnerQuotaProp;        // int spawnQuotaRemaining (property)

    FieldInfo fiDirectorWave;               // private int wave

    void Awake()
    {
        if (!spawner)  spawner  = FindAnyObjectByType<EnemySpawner>();
        if (!director) director = FindAnyObjectByType<WaveDirector>();
        if (!cycle)    cycle    = FindAnyObjectByType<DayNightCycle>();

        // Cache reflection bits that might or might not exist in your versions
        if (spawner)
        {
            var t = spawner.GetType();
            piSpawnerAliveCount   = t.GetProperty("AliveCount", BindingFlags.Instance | BindingFlags.Public);
            fiSpawnerAliveList    = t.GetField("alive", BindingFlags.Instance | BindingFlags.NonPublic);
            fiSpawnerQuotaField   = t.GetField("spawnQuotaRemaining", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            piSpawnerQuotaProp    = t.GetProperty("spawnQuotaRemaining", BindingFlags.Instance | BindingFlags.Public);
            miSpawnerResetWaveQuota = t.GetMethod("ResetWaveQuota", BindingFlags.Instance | BindingFlags.Public);
        }

        if (director)
        {
            var td = director.GetType();
            fiDirectorWave = td.GetField("wave", BindingFlags.Instance | BindingFlags.NonPublic);
        }
    }

    void Update()
    {
        // Phase + countdown
        if (cycle)
        {
            SetSafe(phaseText, cycle.IsNight ? "NIGHT" : "DAY");
            SetSafe(countdownText, FormatTime(cycle.SecondsRemaining));
        }
        else
        {
            SetSafe(phaseText, "WAVE");
            SetSafe(countdownText, "--:--");
        }

        // Wave number
        int wave = GetWaveNumber();
        SetSafe(waveText, wave > 0 ? $"Wave {wave}" : "Wave ?");

        // Enemies left this night (quota left + alive)
        int enemiesLeft = ComputeEnemiesLeftThisNight();
        if (enemiesLeft >= 0)
            SetSafe(enemiesLeftText, $"Enemies Left: {enemiesLeft}");
        else
            SetSafe(enemiesLeftText, $"Enemies Left: ∞");
    }

    int GetWaveNumber()
    {
        if (fiDirectorWave != null && director != null)
        {
            object v = fiDirectorWave.GetValue(director);
            if (v is int i) return i;
        }
        return 0; // unknown
    }

    int ComputeEnemiesLeftThisNight()
    {
        if (spawner == null) return -1;

        // Quota remaining (if present)
        int quota = -1;
        if (piSpawnerQuotaProp != null)
        {
            object v = piSpawnerQuotaProp.GetValue(spawner);
            if (v is int qi) quota = qi;
        }
        else if (fiSpawnerQuotaField != null)
        {
            object v = fiSpawnerQuotaField.GetValue(spawner);
            if (v is int qi) quota = qi;
        }

        // Alive count (via property, else private list)
        int alive = 0;
        if (piSpawnerAliveCount != null)
        {
            object v = piSpawnerAliveCount.GetValue(spawner);
            if (v is int a) alive = a;
        }
        else if (fiSpawnerAliveList != null)
        {
            object v = fiSpawnerAliveList.GetValue(spawner);
            if (v is IList list) alive = list.Count;
        }

        if (quota < 0)
        {
            // No quota system → show alive only, and say ∞ in UI
            return alive == 0 ? 0 : -1; // -1 makes the UI print "∞"
        }
        else
        {
            int q = Mathf.Max(0, quota);
            return q + Mathf.Max(0, alive);
        }
    }

    static void SetSafe(TextMeshProUGUI tmp, string text)
    {
        if (tmp) tmp.SetText(text);
    }

    static string FormatTime(float seconds)
    {
        if (float.IsNaN(seconds) || float.IsInfinity(seconds)) return "--:--";
        seconds = Mathf.Max(0f, seconds);
        int m = Mathf.FloorToInt(seconds / 60f);
        int s = Mathf.FloorToInt(seconds % 60f);
        return $"{m:00}:{s:00}";
    }
}