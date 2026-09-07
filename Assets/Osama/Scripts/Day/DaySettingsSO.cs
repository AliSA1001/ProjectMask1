using UnityEngine;

public enum DayPhase
{
    Morning,
    Day,
    Evening,
    Night
}

// all the numbers for the clock. One asset, referenced by the DayManager
[CreateAssetMenu(fileName = "DaySettings", menuName = "Shop/Day Settings")]
public class DaySettingsSO : ScriptableObject
{
    [Header("Clock")]
    [Tooltip("Real seconds for one in-game hour. 60 = a full day takes 24 real minutes")]
    [Min(1f)] public float realSecondsPerHour = 60f;
    [Min(1)] public int startDay = 1;
    [Range(0f, 24f)] public float startHour = 8f;
    [Tooltip("Where the clock lands after sleeping through the night")]
    [Range(0f, 24f)] public float wakeUpHour = 8f;

    [Header("When each phase starts (hour of the day). The day counter goes up at morningStart, not at midnight")]
    [Range(0f, 24f)] public float morningStart = 6f;
    [Range(0f, 24f)] public float dayStart = 10f;
    [Range(0f, 24f)] public float eveningStart = 17f;
    [Range(0f, 24f)] public float nightStart = 20f;

    [Header("HUD sky colour per phase")]
    public Color morningSky = new Color(0.95f, 0.75f, 0.55f);
    public Color daySky = new Color(0.45f, 0.70f, 0.95f);
    public Color eveningSky = new Color(0.85f, 0.45f, 0.35f);
    public Color nightSky = new Color(0.08f, 0.10f, 0.20f);

    public DayPhase GetPhase(float hour)
    {
        if (hour >= nightStart || hour < morningStart) return DayPhase.Night;
        if (hour >= eveningStart) return DayPhase.Evening;
        if (hour >= dayStart) return DayPhase.Day;
        return DayPhase.Morning;
    }

    public Color GetSkyColor(DayPhase phase)
    {
        switch (phase)
        {
            case DayPhase.Morning: return morningSky;
            case DayPhase.Evening: return eveningSky;
            case DayPhase.Night: return nightSky;
            default: return daySky;
        }
    }

    // 0 at sunrise, 1 at nightfall. Negative while it's night
    public float GetDaylightProgress(float hour)
    {
        if (GetPhase(hour) == DayPhase.Night) return -1f;
        return Mathf.InverseLerp(morningStart, nightStart, hour);
    }

    // 0 at nightfall, 1 at sunrise. Negative while it's day
    public float GetNightProgress(float hour)
    {
        if (GetPhase(hour) != DayPhase.Night) return -1f;

        float nightLength = 24f - nightStart + morningStart;
        float sinceNightfall = hour >= nightStart ? hour - nightStart : hour + 24f - nightStart;
        return Mathf.Clamp01(sinceNightfall / nightLength);
    }
}
