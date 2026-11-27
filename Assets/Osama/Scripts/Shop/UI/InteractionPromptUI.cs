using TMPro;
using UnityEngine;

// shows the "[E] Pick up" text near the middle of the screen
public class InteractionPromptUI : MonoBehaviour
{
    [Tooltip("Found automatically if empty")]
    [SerializeField] private PlayerInteraction interaction;
    [SerializeField] private GameObject panel; // gets toggled, falls back to the text object
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private string keyHint = "E";

    private void Start()
    {
        if (interaction == null) interaction = FindFirstObjectByType<PlayerInteraction>();
        if (interaction != null) interaction.OnTargetChanged += HandleTargetChanged;

        HandleTargetChanged(null, null);
    }

    private void OnDestroy()
    {
        if (interaction != null) interaction.OnTargetChanged -= HandleTargetChanged;
    }

    private void HandleTargetChanged(IInteractable target, string prompt)
    {
        bool show = !string.IsNullOrEmpty(prompt);

        GameObject toggle = panel != null ? panel : (promptText != null ? promptText.gameObject : null);
        if (toggle != null) toggle.SetActive(show);

        if (!show || promptText == null) return;

        // lines starting with '>' are actions, they get the key in front
        string[] lines = prompt.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].StartsWith(">"))
                lines[i] = $"[{keyHint}] " + lines[i].Substring(1);
        }
        promptText.text = string.Join("\n", lines);
    }
}
