using TMPro;
using UnityEngine;

public class ClockUI : MonoBehaviour
{
    [SerializeField] private TMP_Text label;

    private void Awake()
    {
        if (label == null) label = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        DayCycleManager dayCycle = DayCycleManager.Instance;
        if (dayCycle == null) return;

        string text = "Day " + dayCycle.Day + " - " + dayCycle.GetClockText();
        if (dayCycle.IsMorning && !dayCycle.CanTalkToCharacters) text += " (too late to talk)";
        if (dayCycle.IsPaused) text += " (paused)";

        if (label.text != text) label.text = text;
    }
}