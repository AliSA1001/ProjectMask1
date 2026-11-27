using UnityEngine;

public enum QuestType
{
    Story,
    Character,
    World
}

public enum QuestStatus
{
    NotStarted,
    Active,
    Completed,
    Failed
}

[CreateAssetMenu(fileName = "Quest", menuName = "Quests/Quest")]
public class QuestData : ScriptableObject
{
    [Header("Info")]
    public string id;
    public string title;
    [TextArea] public string description;
    public QuestType type;
    public string giver;

    [Header("How it starts")]
    [Min(0)] public int startDay;
    public QuestData startAfter;

    [Header("What the player has to bring")]
    public string requiredItem;
    [Min(1)] public int requiredAmount = 1;

    [Header("Deadline")]
    public bool hasDeadline;
    [Min(1)] public int daysToComplete = 3;

    [Header("Reward")]
    [Min(0)] public int rewardGold;
    public string rewardItem;
    [Min(1)] public int rewardItemAmount = 1;

    [Header("Fail penalty (World quests)")]
    [Min(0)] public int failGoldPenalty;
    public string failItemLost;
    [Min(1)] public int failItemAmount = 1;
}