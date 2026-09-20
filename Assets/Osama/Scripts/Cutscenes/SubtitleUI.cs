using TMPro;
using UnityEngine;

// the subtitle box. The Subtitle track in the Timeline drives it, you don't call this yourself
public class SubtitleUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup group;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text bodyText;

    private void Awake()
    {
        if (group == null) group = GetComponent<CanvasGroup>();
        Hide();
    }

    // alpha lets the clip's ease in / out fade the box
    public void Show(string speaker, string text, float alpha)
    {
        if (speakerText != null)
        {
            bool hasSpeaker = !string.IsNullOrEmpty(speaker);
            speakerText.gameObject.SetActive(hasSpeaker);
            speakerText.text = speaker;
        }
        if (bodyText != null) bodyText.text = text;
        if (group != null) group.alpha = Mathf.Clamp01(alpha);
    }

    public void Hide()
    {
        if (group != null) group.alpha = 0f;
    }
}
