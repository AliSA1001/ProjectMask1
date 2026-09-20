using UnityEngine;

// the "hold Space to skip" label. Shows only while a skippable cutscene plays and fills a bar while the button is held
public class CutsceneSkipHint : MonoBehaviour
{
    [SerializeField] private CanvasGroup group;
    [Tooltip("A bar image, it grows from the left while the skip button is held")]
    [SerializeField] private RectTransform fillBar;

    private void Awake()
    {
        if (group == null) group = GetComponent<CanvasGroup>();
        SetVisible(false);
    }

    private void Update()
    {
        CutsceneManager manager = CutsceneManager.instance;
        bool show = manager != null && manager.IsPlaying && manager.Current.CanSkip;
        SetVisible(show);

        if (show && fillBar != null)
            fillBar.anchorMax = new Vector2(Mathf.Clamp01(manager.SkipProgress), fillBar.anchorMax.y);
    }

    private void SetVisible(bool visible)
    {
        if (group != null) group.alpha = visible ? 1f : 0f;
    }
}
