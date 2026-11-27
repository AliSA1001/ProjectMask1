using UnityEngine;

// a bed. During the day it skips to the night (GDD: you can skip the day), at night it sleeps until morning.
// If the night must always be played, drop the IsNight branch
public class SleepSpot : MonoBehaviour, IInteractable, IInteractionPrompt
{
    [SerializeField] private string label = "Bed";

    public string GetPrompt()
    {
        DayManager day = DayManager.instance;
        if (day == null) return label;

        return day.IsNight ? label + "\n>Sleep until morning" : label + "\n>Skip to night";
    }

    public void Interact()
    {
        DayManager day = DayManager.instance;
        if (day == null) return;

        if (day.IsNight) day.SkipToNextDay();
        else day.SkipToNight();
    }
}
