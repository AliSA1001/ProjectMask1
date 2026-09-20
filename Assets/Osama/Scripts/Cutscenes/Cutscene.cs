using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

// one cutscene = one GameObject with this + a PlayableDirector that holds the Timeline.
// Everything about how the game behaves while it plays (lock the player, hide the HUD, pause the clock) is set here in the inspector,
// the shots / animation / subtitles / fades live in the Timeline.
//
//   cutscene.Play()      -> from a UnityEvent, a button, a trigger, or code
//   cutscene.Skip()
//   onStarted / onFinished are UnityEvents, use them for "after the cutscene do X" without writing code
//   (onFinished also fires when the player skips)
[RequireComponent(typeof(PlayableDirector))]
public class Cutscene : MonoBehaviour
{
    [Tooltip("Used to remember that a Play Once cutscene already played. Empty = the GameObject name")]
    [SerializeField] private string cutsceneId;
    [Tooltip("Plays one time per run of the game, later Play() calls are ignored")]
    [SerializeField] private bool playOnce;
    [SerializeField] private bool canSkip = true;

    [Header("While it plays")]
    [Tooltip("Turns off movement, shooting and interaction until the cutscene is over")]
    [SerializeField] private bool lockPlayer = true;
    [Tooltip("Switches off the player's camera and shows the cameras that are children of this object. Needs a Camera under this object that the Timeline turns on at 0:00")]
    [SerializeField] private bool useOwnCamera = true;
    [Tooltip("Freezes the day/night clock")]
    [SerializeField] private bool pauseDayClock = true;
    [Tooltip("Hidden while playing and brought back after (day HUD, shop HUD, crosshair...)")]
    [SerializeField] private GameObject[] hideWhilePlaying;

    [Header("Events")]
    public UnityEvent onStarted;
    public UnityEvent onFinished;

    private PlayableDirector director;
    // looked up on demand so it also works if the cutscene object is disabled until it's needed
    public PlayableDirector Director => director != null ? director : (director = GetComponent<PlayableDirector>());

    public string Id => string.IsNullOrEmpty(cutsceneId) ? name : cutsceneId;
    public bool PlayOnce => playOnce;
    public bool CanSkip => canSkip;
    public bool LockPlayer => lockPlayer;
    public bool UseOwnCamera => useOwnCamera;
    public bool PauseDayClock => pauseDayClock;
    public GameObject[] HideWhilePlaying => hideWhilePlaying;
    public bool IsPlaying => CutsceneManager.instance != null && CutsceneManager.instance.Current == this;

    private void Reset()
    {
        // the director must not start on its own, the manager starts it so the game state gets locked first
        GetComponent<PlayableDirector>().playOnAwake = false;
    }

    private void Awake()
    {
        Director.playOnAwake = false;
    }

    // returns false if it didn't start (another cutscene is playing, or Play Once and it already played)
    public bool Play()
    {
        return CutsceneManager.GetOrCreate().Play(this);
    }

    public void Skip()
    {
        if (CutsceneManager.instance != null && CutsceneManager.instance.Current == this)
            CutsceneManager.instance.Skip();
    }
}
