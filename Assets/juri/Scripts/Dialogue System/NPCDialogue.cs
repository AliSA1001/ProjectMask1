using UnityEngine;
using Yarn.Unity;

public class NPCDialogue : MonoBehaviour, IInteractable
{
    [SerializeField] private string startNode = "Nurse";

    private DialogueRunner runner;
    private float canTalkAgainAt;

    private void Start()
    {
        runner = FindFirstObjectByType<DialogueRunner>();
        if (runner == null) Debug.LogWarning("No DialogueRunner in the scene", this);
    }

    public void Interact()
    {
        if (runner == null || runner.IsDialogueRunning) return;

        if (Time.time < canTalkAgainAt) return;
        canTalkAgainAt = Time.time + 0.5f;

        if (DayCycleManager.Instance != null && !DayCycleManager.Instance.CanTalkToCharacters)
        {
            Debug.Log("Too late to talk today");
            return;
        }

        runner.StartDialogue(startNode);
    }
}