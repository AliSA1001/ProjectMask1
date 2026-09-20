using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

// one line of dialogue on the Subtitle track. Length of the clip = how long the line stays on screen
[Serializable]
public class SubtitleClip : PlayableAsset, ITimelineClipAsset
{
    [Tooltip("Who is talking. Leave empty for narration")]
    public string speaker;
    [TextArea(2, 5)] public string text;

    // Blending = the clip edges can be dragged over each other / eased to fade the box in and out
    public ClipCaps clipCaps => ClipCaps.Blending;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        ScriptPlayable<SubtitleBehaviour> playable = ScriptPlayable<SubtitleBehaviour>.Create(graph);
        SubtitleBehaviour behaviour = playable.GetBehaviour();
        behaviour.speaker = speaker;
        behaviour.text = text;
        return playable;
    }
}

// runtime copy of the clip's data
public class SubtitleBehaviour : PlayableBehaviour
{
    public string speaker;
    public string text;
}
