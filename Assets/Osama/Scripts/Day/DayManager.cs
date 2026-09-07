using System;
using UnityEngine;

// the game clock: day counter, hour of the day and the current phase (morning / day / evening / night).
// Other systems (shop, dialogue, quests) read the properties or subscribe to the events. Nothing in here knows about them.
//
//   DayManager.instance.Day            -> 1, 2, 3 ...
//   DayManager.instance.Hour           -> 0..24 (13.5 = 13:30)
//   DayManager.instance.Phase          -> DayPhase.Morning / Day / Evening / Night
//   DayManager.instance.AdvanceMinutes(30)   -> an action that costs time
//   DayManager.instance.SkipToNight() / SkipToNextDay()
//   OnDayChanged / OnPhaseChanged / OnMinuteChanged
public class DayManager : MonoBehaviour
{
    public static DayManager instance { get; private set; }

    [SerializeField] private DaySettingsSO settings;
    [Tooltip("Untick to freeze the clock (menus, cutscenes, dialogue)")]
    [SerializeField] private bool runClock = true;
    [Tooltip("Survives scene loads so the day count carries over to the night / dungeon scene")]
    [SerializeField] private bool keepBetweenScenes = true;

    public DaySettingsSO Settings => settings;
    public int Day { get; private set; }
    public float Hour { get; private set; } // 0..24
    public DayPhase Phase { get; private set; }
    public bool IsNight => Phase == DayPhase.Night;
    public int HourInt => Mathf.FloorToInt(Hour);
    public int MinuteInt => Mathf.FloorToInt((Hour - HourInt) * 60f);
    public string TimeText => HourInt.ToString("00") + ":" + MinuteInt.ToString("00");
    public float DaylightProgress => settings.GetDaylightProgress(Hour);
    public float NightProgress => settings.GetNightProgress(Hour);

    // dialogue / pause menu can freeze time with this
    public bool RunClock
    {
        get => runClock;
        set => runClock = value;
    }

    public event Action<int> OnDayChanged;        // a new day started (fires at morningStart)
    public event Action<DayPhase> OnPhaseChanged; // morning -> day -> evening -> night
    public event Action OnMinuteChanged;          // for clocks and HUDs

    // hours since the game started, everything else is derived from this
    private double totalHours;
    private int dayOffset;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        if (keepBetweenScenes) DontDestroyOnLoad(gameObject);

        if (settings == null)
        {
            Debug.LogWarning("DayManager has no DaySettings asset, using defaults", this);
            settings = ScriptableObject.CreateInstance<DaySettingsSO>();
        }

        totalHours = 0.0;
        dayOffset = DayIndex(settings.startHour);
        Recalculate(false);
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    private void Update()
    {
        if (!runClock) return;
        Advance(Time.deltaTime / settings.realSecondsPerHour);
    }

    // "every action costs time"
    public void AdvanceMinutes(float minutes)
    {
        if (minutes > 0f) Advance(minutes / 60f);
    }

    public void AdvanceHours(float hours)
    {
        if (hours > 0f) Advance(hours);
    }

    // GDD: the player can skip the day, the night still has to be played
    public void SkipToNight()
    {
        if (IsNight) return;
        Advance(HoursUntil(settings.nightStart));
    }

    // slept / survived the night -> next morning at wakeUpHour
    public void SkipToNextDay()
    {
        Advance(HoursUntil(settings.wakeUpHour));
    }

    private float HoursUntil(float targetHour)
    {
        float delta = targetHour - Hour;
        if (delta <= 0.001f) delta += 24f;
        return delta;
    }

    private void Advance(double hours)
    {
        totalHours += hours;
        Recalculate(true);
    }

    private void Recalculate(bool fireEvents)
    {
        int oldDay = Day;
        int oldHour = HourInt;
        int oldMinute = MinuteInt;
        DayPhase oldPhase = Phase;

        double t = totalHours + settings.startHour;
        Hour = (float)(t % 24.0);
        Day = settings.startDay + DayIndex(t) - dayOffset;
        Phase = settings.GetPhase(Hour);

        if (!fireEvents) return;
        // note: a big skip only reports the final state, phases in between don't fire
        if (Day != oldDay) OnDayChanged?.Invoke(Day);
        if (Phase != oldPhase) OnPhaseChanged?.Invoke(Phase);
        if (MinuteInt != oldMinute || HourInt != oldHour) OnMinuteChanged?.Invoke();
    }

    // which day an absolute hour count falls in. Days flip at morningStart, not at midnight,
    // so the night belongs to the day it started in
    private int DayIndex(double absoluteHours)
    {
        return (int)Math.Floor((absoluteHours - settings.morningStart) / 24.0);
    }
}
