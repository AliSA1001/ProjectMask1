using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

// a fade on the Fade track. Alpha 1 = the black panel fully visible (black screen), 0 = clear.
//   fade to black:    start 0, end 1
//   fade from black:  start 1, end 0
[Serializable]
public class FadeClip : PlayableAsset, ITimelineClipAsset
{
    [Range(0f, 1f)] public float startAlpha = 1f;
    [Range(0f, 1f)] public float endAlpha = 0f;
    public AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    public ClipCaps clipCaps => ClipCaps.None;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        ScriptPlayable<FadeBehaviour> playable = ScriptPlayable<FadeBehaviour>.Create(graph);
        FadeBehaviour behaviour = playable.GetBehaviour();
        behaviour.startAlpha = startAlpha;
        behaviour.endAlpha = endAlpha;
        behaviour.curve = curve;
        return playable;
    }
}

// runtime copy of the clip's data
public class FadeBehaviour : PlayableBehaviour
{
    public float startAlpha;
    public float endAlpha;
    public AnimationCurve curve;
}
