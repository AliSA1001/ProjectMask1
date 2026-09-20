using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

// Timeline track for screen fades. Bind the CanvasGroup of the black FadePanel
[TrackColor(0.25f, 0.25f, 0.3f)]
[TrackClipType(typeof(FadeClip))]
[TrackBindingType(typeof(CanvasGroup))]
public class FadeTrack : TrackAsset
{
    [Tooltip("Ticked = when the cutscene ends the screen goes back to how it was (clear). Untick to stay black, e.g. before loading a scene")]
    public bool restoreOnEnd = true;

    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        ScriptPlayable<FadeMixerBehaviour> mixer = ScriptPlayable<FadeMixerBehaviour>.Create(graph, inputCount);
        mixer.GetBehaviour().restoreOnEnd = restoreOnEnd;
        return mixer;
    }
}

// between clips it keeps the last value, so a fade to black stays black until the next fade clip
public class FadeMixerBehaviour : PlayableBehaviour
{
    public bool restoreOnEnd = true;

    private CanvasGroup group;
    private float originalAlpha;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        CanvasGroup target = playerData as CanvasGroup;
        if (target == null) return;

        if (group == null)
        {
            group = target;
            originalAlpha = target.alpha;
        }

        float alpha = 0f;
        float totalWeight = 0f;
        int count = playable.GetInputCount();
        for (int i = 0; i < count; i++)
        {
            float weight = playable.GetInputWeight(i);
            if (weight <= 0f) continue;

            ScriptPlayable<FadeBehaviour> input = (ScriptPlayable<FadeBehaviour>)playable.GetInput(i);
            FadeBehaviour clip = input.GetBehaviour();
            double duration = input.GetDuration();
            float t = duration > 0.0 ? Mathf.Clamp01((float)(input.GetTime() / duration)) : 1f;

            alpha += Mathf.Lerp(clip.startAlpha, clip.endAlpha, clip.curve.Evaluate(t)) * weight;
            totalWeight += weight;
        }

        if (totalWeight > 0f) group.alpha = alpha;
    }

    public override void OnPlayableDestroy(Playable playable)
    {
        if (restoreOnEnd && group != null) group.alpha = originalAlpha;
    }
}
