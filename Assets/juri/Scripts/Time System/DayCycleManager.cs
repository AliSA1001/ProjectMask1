using System;
using UnityEngine;

public class DayCycleManager : MonoBehaviour
{
    public static DayCycleManager Instance { get; private set; }

    [Header("Morning")]
    [SerializeField] private float morningLength = 600f;
    [SerializeField] private int morningStartHour = 6;
    [SerializeField] private int morningEndHour = 18;

    [Header("Testing")]
    [SerializeField] private float testSpeed = 1f;

    [Header("Talking")]
    [SerializeField] private float noTalkingLastSeconds = 120f;

    public int Day { get; private set; } = 1;
    public bool IsNight { get; private set; }
    public bool IsMorning => !IsNight;
    public float MorningTimer { get; private set; }

    public event Action<int> OnMorningStarted;
    public event Action<int> OnNightStarted;

    private int pauseCount;
    public bool IsPaused => pauseCount > 0;

    public bool CanTalkToCharacters => IsMorning && morningLength - MorningTimer > noTalkingLastSeconds;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        StartMorning();
    }

    private void Update()
    {
        if (IsNight || IsPaused) return;

        MorningTimer += Time.deltaTime * testSpeed;
        if (MorningTimer >= morningLength)
        {
            EndMorning();
        }
    }

    public void EndMorning()
    {
        if (IsNight) return;
        IsNight = true;
        OnNightStarted?.Invoke(Day);
    }

    public void EndNight()
    {
        if (IsMorning) return;
        Day++;
        StartMorning();
    }

    private void StartMorning()
    {
        IsNight = false;
        MorningTimer = 0f;
        OnMorningStarted?.Invoke(Day);
    }

    public void LoadDay(int day)
    {
        Day = day;
        StartMorning();
    }

    public void Pause()
    {
        pauseCount++;
    }

    public void Resume()
    {
        pauseCount = Mathf.Max(0, pauseCount - 1);
    }

    public string GetClockText()
    {
        if (IsNight) return "Night";

        float progress = MorningTimer / morningLength;
        float hours = morningStartHour + progress * (morningEndHour - morningStartHour);

        int hour = Mathf.FloorToInt(hours);
        int minute = Mathf.FloorToInt((hours - hour) * 60f);
        return hour.ToString("00") + ":" + minute.ToString("00");
    }

    [ContextMenu("End Morning")]
    private void TestEndMorning()
    {
        EndMorning();
    }

    [ContextMenu("End Night")]
    private void TestEndNight()
    {
        EndNight();
    }
}