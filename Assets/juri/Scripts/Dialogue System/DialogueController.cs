using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Yarn.Unity;

[RequireComponent(typeof(DialogueRunner))]
public class DialogueController : MonoBehaviour
{
    public static bool EndDayWhenFinished;

    private DialogueRunner runner;
    private bool isTalking;
    private List<Behaviour> switchedOff = new List<Behaviour>();

    private void Awake()
    {
        runner = GetComponent<DialogueRunner>();
    }

    private void OnEnable()
    {
        runner.onDialogueStart.AddListener(OnDialogueStart);
        runner.onDialogueComplete.AddListener(OnDialogueEnd);
    }

    private void OnDisable()
    {
        runner.onDialogueStart.RemoveListener(OnDialogueStart);
        runner.onDialogueComplete.RemoveListener(OnDialogueEnd);
        if (isTalking) OnDialogueEnd();
    }

    private void OnDialogueStart()
    {
        isTalking = true;
        EndDayWhenFinished = false;

        foreach (PlayerInput input in FindObjectsByType<PlayerInput>(FindObjectsSortMode.None))
        {
            SwitchOff(input);
        }
        if (Movement.instance != null) SwitchOff(Movement.instance);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (DayCycleManager.Instance != null) DayCycleManager.Instance.Pause();
    }

    private void OnDialogueEnd()
    {
        if (!isTalking) return;
        isTalking = false;

        foreach (Behaviour thing in switchedOff)
        {
            if (thing != null) thing.enabled = true;
        }
        switchedOff.Clear();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (DayCycleManager.Instance != null)
        {
            DayCycleManager.Instance.Resume();
            if (EndDayWhenFinished) DayCycleManager.Instance.EndMorning();
        }
        EndDayWhenFinished = false;
    }

    private void SwitchOff(Behaviour thing)
    {
        if (!thing.enabled) return;
        thing.enabled = false;
        switchedOff.Add(thing);
    }
}