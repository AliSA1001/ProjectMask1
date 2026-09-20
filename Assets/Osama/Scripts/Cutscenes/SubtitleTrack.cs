using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

// Timeline track for dialogue lines. Drag the SubtitleUI object from the scene into the track's binding slot
[TrackColor(0.95f, 0.75f, 0.2f)]
[TrackClipType(typeof(SubtitleClip))]
[TrackBindingType(typeof(SubtitleUI))]
public class SubtitleTrack : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        return ScriptPlayable<SubtitleMixerBehaviour>.Create(graph, inputCount);
    }
}

// runs every frame of the track: shows the clip that is playing (the strongest one if two overlap), hides the box when none is
public class SubtitleMixerBehaviour : PlayableBehaviour
{
    private SubtitleUI ui;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        SubtitleUI target = playerData as SubtitleUI;
        if (target == null) return;
        ui = target;

        float bestWeight = 0f;
        SubtitleBehaviour best = null;
        int count = playable.GetInputCount();
        for (int i = 0; i < count; i++)
        {
            float weight = playable.GetInputWeight(i);
            if (weight <= bestWeight) continue;

            bestWeight = weight;
            best = ((ScriptPlayable<SubtitleBehaviour>)playable.GetInput(i)).GetBehaviour();
        }

        if (best != null) ui.Show(best.speaker, best.text, bestWeight);
        else ui.Hide();
    }

    // cutscene ended / skipped / stopped in the editor: never leave a line stuck on screen
    public override void OnPlayableDestroy(Playable playable)
    {
        if (ui != null) ui.Hide();
    }
}
