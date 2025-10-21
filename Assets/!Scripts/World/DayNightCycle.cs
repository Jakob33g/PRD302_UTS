using UnityEngine;
using UnityEngine.Events;

public class DayNightCycle : MonoBehaviour
{
    public enum Phase { Day, Night }

    [Header("Lengths (seconds)")]
    public float dayLength   = 90f;
    public float nightLength = 60f;

    [Header("Lighting (optional)")]
    public Light sun;
    [Range(0f, 2f)] public float dayIntensity   = 1.0f;
    [Range(0f, 2f)] public float nightIntensity = 0.2f;
    public Color dayColor   = new Color(1f, 0.956f, 0.84f);
    public Color nightColor = new Color(0.6f, 0.7f, 1.0f);

    [Header("Start Phase")]
    public Phase startPhase = Phase.Day;

    [Header("Events")]
    public UnityEvent onDayStarted;
    public UnityEvent onNightStarted;

    public Phase CurrentPhase { get; private set; }
    float phaseTimer;

    public bool IsNight => CurrentPhase == Phase.Night;
    public float SecondsRemaining => Mathf.Max(0f, phaseTimer);
    public float Normalized01
    {
        get
        {
            float len = CurrentPhase == Phase.Day ? dayLength : nightLength;
            if (len <= 0.0001f) return 1f;
            return 1f - (phaseTimer / len);
        }
    }

    void Start()
    {
        CurrentPhase = startPhase;
        phaseTimer   = (CurrentPhase == Phase.Day) ? dayLength : nightLength;
        ApplyLightingInstant();
        InvokeStartEvent();
    }

    void Update()
    {
        phaseTimer -= Time.deltaTime;
        if (phaseTimer <= 0f)
            SwitchPhase();

        UpdateLightingSmooth();
    }

    void SwitchPhase()
    {
        CurrentPhase = (CurrentPhase == Phase.Day) ? Phase.Night : Phase.Day;
        phaseTimer   = (CurrentPhase == Phase.Day) ? dayLength : nightLength;
        InvokeStartEvent();
    }

    void InvokeStartEvent()
    {
        if (CurrentPhase == Phase.Day) onDayStarted?.Invoke();
        else                           onNightStarted?.Invoke();
    }

    void ApplyLightingInstant()
    {
        if (!sun) return;
        if (CurrentPhase == Phase.Day)
        {
            sun.color = dayColor;
            sun.intensity = dayIntensity;
        }
        else
        {
            sun.color = nightColor;
            sun.intensity = nightIntensity;
        }
    }

    void UpdateLightingSmooth()
    {
        if (!sun) return;
        float target = (CurrentPhase == Phase.Day) ? dayIntensity : nightIntensity;
        Color tColor = (CurrentPhase == Phase.Day) ? dayColor : nightColor;

        sun.intensity = Mathf.Lerp(sun.intensity, target, 1f - Mathf.Exp(-5f * Time.deltaTime));
        sun.color     = Color.Lerp(sun.color, tColor, 1f - Mathf.Exp(-3f * Time.deltaTime));
    }

    // Optional external control:
    public void ForceStartDay()
    {
        CurrentPhase = Phase.Day;
        phaseTimer   = dayLength;
        ApplyLightingInstant();
        onDayStarted?.Invoke();
    }
    public void ForceStartNight()
    {
        CurrentPhase = Phase.Night;
        phaseTimer   = nightLength;
        ApplyLightingInstant();
        onNightStarted?.Invoke();
    }
}