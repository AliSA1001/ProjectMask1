using TMPro;
using UnityEngine;
using UnityEngine.UI;

// top-left clock: day number, time, phase name and a little sky window with the sun / moon moving over an arc.
// Every field is optional. The placeholder sprites on the images get swapped for the real art later
public class DayTimeHUD : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text phaseText;
    [SerializeField] private string dayFormat = "DAY {0}";
    // upper case on purpose, the Ghost Shadow font has no lower case letters
    [SerializeField] private string[] phaseNames = { "MORNING", "AFTERNOON", "EVENING", "NIGHT" };

    [Header("Sky window")]
    [Tooltip("Gets tinted with the phase colour from the DaySettings")]
    [SerializeField] private Image sky;
    [SerializeField] private RectTransform sun;
    [SerializeField] private RectTransform moon;
    [Tooltip("Centre of the arc, relative to the centre of the sky image")]
    [SerializeField] private Vector2 arcCenter = new Vector2(0f, -18f);
    [SerializeField] private float arcRadius = 34f;
    [SerializeField] private float skyFadeSpeed = 2f;

    [Header("Optional: one icon per phase (Morning, Day, Evening, Night)")]
    [SerializeField] private bool showPhaseIcon = false;
    [SerializeField] private Image phaseIcon;
    [SerializeField] private Sprite[] phaseSprites;

    private DayManager day;

    private void Start()
    {
        day = DayManager.instance;
        if (day == null)
        {
            Debug.LogWarning("DayTimeHUD found no DayManager in the scene", this);
            enabled = false;
            return;
        }

        day.OnMinuteChanged += RefreshText;
        day.OnDayChanged += HandleDay;
        day.OnPhaseChanged += HandlePhase;

        RefreshText();
        HandlePhase(day.Phase);
        if (sky != null) sky.color = day.Settings.GetSkyColor(day.Phase);
    }

    private void OnDestroy()
    {
        if (day == null) return;
        day.OnMinuteChanged -= RefreshText;
        day.OnDayChanged -= HandleDay;
        day.OnPhaseChanged -= HandlePhase;
    }

    private void LateUpdate()
    {
        // the smooth stuff runs every frame: sun / moon position and the sky tint
        MoveOverArc(sun, day.DaylightProgress);
        MoveOverArc(moon, day.NightProgress);

        if (sky != null)
            sky.color = Color.Lerp(sky.color, day.Settings.GetSkyColor(day.Phase), Time.deltaTime * skyFadeSpeed);
    }

    // progress 0 = left horizon, 1 = right horizon, negative = hidden
    private void MoveOverArc(RectTransform icon, float progress)
    {
        if (icon == null) return;

        bool visible = progress >= 0f;
        if (icon.gameObject.activeSelf != visible) icon.gameObject.SetActive(visible);
        if (!visible) return;

        float angle = Mathf.Lerp(Mathf.PI, 0f, progress);
        icon.anchoredPosition = arcCenter + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * arcRadius;
    }

    private void RefreshText()
    {
        if (dayText != null) dayText.text = string.Format(dayFormat, day.Day);
        if (timeText != null) timeText.text = day.TimeText;
    }

    private void HandleDay(int newDay)
    {
        RefreshText();
    }

    private void HandlePhase(DayPhase phase)
    {
        int i = (int)phase;
        if (phaseText != null && phaseNames != null && i < phaseNames.Length) phaseText.text = phaseNames[i];
        if (phaseIcon != null)
        {
            // stays hidden until there's real art for it
            Sprite sprite = phaseSprites != null && i < phaseSprites.Length ? phaseSprites[i] : null;
            phaseIcon.sprite = sprite;
            phaseIcon.enabled = showPhaseIcon && sprite != null;
        }
        RefreshText();
    }
}
