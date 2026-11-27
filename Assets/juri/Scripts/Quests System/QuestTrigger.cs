using UnityEngine;

public class QuestTrigger : MonoBehaviour, IInteractable
{
    [SerializeField] private string questId;
    [SerializeField] private bool completeOnEnter;

    public void Interact()
    {
        if (!completeOnEnter) Complete();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (completeOnEnter && other.CompareTag("Player")) Complete();
    }

    private void Complete()
    {
        if (QuestManager.Instance == null) return;
        if (QuestManager.Instance.GetStatus(questId) != QuestStatus.Active) return;

        QuestManager.Instance.CompleteQuest(questId);
    }
}