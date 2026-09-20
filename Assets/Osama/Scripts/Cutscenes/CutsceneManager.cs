using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

// runs a Cutscene: locks the player, swaps the camera, hides the HUD, pauses the day clock, handles skip,
// and puts every one of those things back when the cutscene ends (or is skipped).
// You never have to place this in the scene, Cutscene.Play() creates it when it's needed.
//
//   CutsceneManager.instance.IsPlaying
//   CutsceneManager.instance.OnCutsceneStarted / OnCutsceneEnded
public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager instance { get; private set; }

    [Tooltip("How long the skip button has to be held. 0 = instant")]
    [SerializeField, Min(0f)] private float holdToSkipSeconds = 0.75f;

    public Cutscene Current { get; private set; }
    public bool IsPlaying => Current != null;
    // 0..1 while the skip button is held, the skip hint reads this
    public float SkipProgress { get; private set; }

    public event Action<Cutscene> OnCutsceneStarted;
    public event Action<Cutscene> OnCutsceneEnded;

    // Play Once memory. Only lives as long as the game is running, when the save system exists this is what it should write
    private static readonly HashSet<string> played = new HashSet<string>();

    // one entry per thing we changed, run backwards when the cutscene ends so the game goes back exactly how it was
    private readonly List<Action> undo = new List<Action>();
    private InputAction skipAction;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        played.Clear();
    }

    public static bool HasPlayed(string cutsceneId) => played.Contains(cutsceneId);

    public static CutsceneManager GetOrCreate()
    {
        if (instance == null)
        {
            GameObject go = new GameObject("CutsceneManager");
            go.AddComponent<CutsceneManager>(); // Awake sets instance
        }
        return instance;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        skipAction = new InputAction("CutsceneSkip", InputActionType.Button);
        skipAction.AddBinding("<Keyboard>/space");
        skipAction.AddBinding("<Keyboard>/escape");
        skipAction.AddBinding("<Gamepad>/buttonSouth");
    }

    private void OnDestroy()
    {
        if (instance != this) return;
        instance = null;

        if (Current != null)
        {
            Current.Director.stopped -= OnDirectorStopped;
            Current = null;
            RunUndo();
        }
        skipAction?.Dispose();
    }

    private void Update()
    {
        if (Current == null || !Current.CanSkip)
        {
            SkipProgress = 0f;
            return;
        }

        if (skipAction.IsPressed())
        {
            SkipProgress += Time.unscaledDeltaTime / Mathf.Max(holdToSkipSeconds, 0.01f);
            if (SkipProgress >= 1f) Skip();
        }
        else
        {
            SkipProgress = 0f;
        }
    }

    public bool Play(Cutscene cutscene)
    {
        if (cutscene == null) return false;
        if (Current != null)
        {
            Debug.LogWarning("Cutscene '" + cutscene.name + "' ignored, '" + Current.name + "' is still playing", cutscene);
            return false;
        }
        if (cutscene.PlayOnce && played.Contains(cutscene.Id)) return false;

        PlayableDirector director = cutscene.Director;
        if (director == null || director.playableAsset == null)
        {
            Debug.LogWarning("Cutscene '" + cutscene.name + "' has no Timeline assigned on its PlayableDirector", cutscene);
            return false;
        }

        Current = cutscene;
        played.Add(cutscene.Id);
        SkipProgress = 0f;

        if (cutscene.LockPlayer) LockPlayer();
        if (cutscene.PauseDayClock) PauseDayClock();
        HideObjects(cutscene.HideWhilePlaying);

        director.extrapolationMode = DirectorWrapMode.None; // stop at the end so the stopped event fires
        director.stopped += OnDirectorStopped;
        director.time = 0.0;
        director.Play();
        // run the first frame now so the cutscene camera is already on before we switch the player camera off (no black flash)
        director.Evaluate();

        if (cutscene.UseOwnCamera) SwapCamera(cutscene);
        skipAction.Enable();

        cutscene.onStarted.Invoke();
        OnCutsceneStarted?.Invoke(cutscene);
        return true;
    }

    // jumps to the end (so anything the Timeline sets at the end still happens) and closes it
    public void Skip()
    {
        if (Current == null || !Current.CanSkip) return;

        PlayableDirector director = Current.Director;
        director.time = director.duration;
        director.Evaluate();
        director.Stop(); // raises stopped -> Finish
    }

    private void OnDirectorStopped(PlayableDirector director)
    {
        if (Current == null || director != Current.Director) return;
        Finish();
    }

    private void Finish()
    {
        Cutscene done = Current;
        done.Director.stopped -= OnDirectorStopped;
        Current = null; // cleared first so onFinished can start the next cutscene
        SkipProgress = 0f;
        skipAction.Disable();

        RunUndo();

        done.onFinished.Invoke();
        OnCutsceneEnded?.Invoke(done);
    }

    private void RunUndo()
    {
        for (int i = undo.Count - 1; i >= 0; i--) undo[i]();
        undo.Clear();
    }

    // ---- what a cutscene changes in the game ----

    // the player's input goes first so buttons that are held get released cleanly, then the scripts that act on it
    private void LockPlayer()
    {
        Movement player = Movement.instance;
        if (player == null)
        {
            Debug.LogWarning("Cutscene: no player found (Movement.instance is null), nothing was locked");
            return;
        }

        Transform root = player.transform;
        PlayerInput input = root.GetComponentInParent<PlayerInput>();
        if (input == null) input = root.GetComponentInChildren<PlayerInput>(true);

        Disable(input);
        Disable(player);
        foreach (Pistol gun in root.GetComponentsInChildren<Pistol>(true)) Disable(gun);
        foreach (PlayerInteraction interaction in root.GetComponentsInChildren<PlayerInteraction>(true)) Disable(interaction);
        foreach (PlayerCarry carry in root.GetComponentsInChildren<PlayerCarry>(true)) Disable(carry);
    }

    private void Disable(Behaviour behaviour)
    {
        if (behaviour == null || !behaviour.enabled) return;
        behaviour.enabled = false;
        undo.Add(() => { if (behaviour != null) behaviour.enabled = true; });
    }

    private void PauseDayClock()
    {
        DayManager day = DayManager.instance;
        if (day == null) return;

        bool wasRunning = day.RunClock;
        day.RunClock = false;
        undo.Add(() => { if (DayManager.instance != null) DayManager.instance.RunClock = wasRunning; });
    }

    private void HideObjects(GameObject[] objects)
    {
        if (objects == null) return;
        foreach (GameObject go in objects)
        {
            if (go == null || !go.activeSelf) continue;
            go.SetActive(false);
            undo.Add(() => { if (go != null) go.SetActive(true); });
        }
    }

    private void SwapCamera(Cutscene cutscene)
    {
        Camera[] cutsceneCameras = cutscene.GetComponentsInChildren<Camera>(true);
        if (cutsceneCameras.Length == 0)
        {
            Debug.LogWarning("Cutscene '" + cutscene.name + "' has Use Own Camera ticked but no Camera under it", cutscene);
            return;
        }

        Camera playerCamera = Movement.instance != null ? Movement.instance.GetComponentInChildren<Camera>(true) : Camera.main;
        if (playerCamera != null && playerCamera.enabled)
        {
            playerCamera.enabled = false;
            undo.Add(() => { if (playerCamera != null) playerCamera.enabled = true; });

            // only one AudioListener can be active, hand it over if the cutscene brought its own
            AudioListener playerListener = playerCamera.GetComponent<AudioListener>();
            if (playerListener != null && playerListener.enabled && cutscene.GetComponentInChildren<AudioListener>(true) != null)
            {
                playerListener.enabled = false;
                undo.Add(() => { if (playerListener != null) playerListener.enabled = true; });
            }
        }
    }
}
