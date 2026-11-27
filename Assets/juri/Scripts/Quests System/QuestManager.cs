using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [SerializeField] private List<QuestData> allQuests = new List<QuestData>();

    public event Action<QuestData, QuestStatus> OnQuestStatusChanged;

    private Dictionary<string, QuestStatus> statuses = new Dictionary<string, QuestStatus>();
    private Dictionary<string, int> startDays = new Dictionary<string, int>();
    private Dictionary<string, int> items = new Dictionary<string, int>();
    private List<string> upsetCharacters = new List<string>();

    private bool storyFailed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (DayCycleManager.Instance == null) return;

        DayCycleManager.Instance.OnMorningStarted += OnNewMorning;
        StartQuestsForDay(DayCycleManager.Instance.Day);
    }

    private void OnDestroy()
    {
        if (DayCycleManager.Instance != null)
        {
            DayCycleManager.Instance.OnMorningStarted -= OnNewMorning;
        }
    }

    private void Update()
    {
        if (storyFailed)
        {
            storyFailed = false;
            GameOver();
        }
    }

    private void OnNewMorning(int day)
    {
        CheckDeadlines();
        StartQuestsForDay(day);
    }

    private void CheckDeadlines()
    {
        foreach (QuestData quest in allQuests)
        {
            if (quest == null || !quest.hasDeadline) continue;
            if (GetStatus(quest.id) != QuestStatus.Active) continue;

            if (DaysLeft(quest.id) <= 0)
            {
                Debug.Log("Missed the deadline of " + quest.title);
                FailQuest(quest.id);
                if (storyFailed) return;
            }
        }
    }

    private void StartQuestsForDay(int day)
    {
        foreach (QuestData quest in allQuests)
        {
            if (quest != null && quest.startDay > 0 && quest.startDay == day)
            {
                StartQuest(quest.id);
            }
        }
    }

    public QuestData FindQuest(string id)
    {
        foreach (QuestData quest in allQuests)
        {
            if (quest != null && quest.id == id) return quest;
        }
        Debug.LogWarning("No quest with the id '" + id + "' in the QuestManager list");
        return null;
    }

    public QuestStatus GetStatus(string id)
    {
        if (statuses.ContainsKey(id)) return statuses[id];
        return QuestStatus.NotStarted;
    }

    public void StartQuest(string id)
    {
        QuestData quest = FindQuest(id);
        if (quest == null) return;
        if (GetStatus(id) != QuestStatus.NotStarted) return;

        if (IsUpset(quest.giver))
        {
            Debug.Log(quest.giver + " is upset, no more quests from them");
            return;
        }

        startDays[id] = CurrentDay();
        SetStatus(quest, QuestStatus.Active);
    }

    public bool IsReady(string id)
    {
        QuestData quest = FindQuest(id);
        if (quest == null) return false;
        if (string.IsNullOrEmpty(quest.requiredItem)) return true;
        return GetItemCount(quest.requiredItem) >= quest.requiredAmount;
    }

    public void CompleteQuest(string id)
    {
        QuestData quest = FindQuest(id);
        if (quest == null) return;
        if (GetStatus(id) != QuestStatus.Active) return;

        if (!IsReady(id))
        {
            Debug.Log("Can't complete " + quest.title + ", missing " + quest.requiredItem);
            return;
        }

        if (!string.IsNullOrEmpty(quest.requiredItem)) RemoveItem(quest.requiredItem, quest.requiredAmount);
        if (quest.rewardGold > 0) Wallet.Add(quest.rewardGold);
        if (!string.IsNullOrEmpty(quest.rewardItem)) AddItem(quest.rewardItem, quest.rewardItemAmount);

        SetStatus(quest, QuestStatus.Completed);

        foreach (QuestData next in allQuests)
        {
            if (next != null && next.startAfter == quest) StartQuest(next.id);
        }
    }

    public void FailQuest(string id)
    {
        QuestData quest = FindQuest(id);
        if (quest == null) return;
        if (GetStatus(id) != QuestStatus.Active) return;

        SetStatus(quest, QuestStatus.Failed);

        if (quest.type == QuestType.Character)
        {
            if (!string.IsNullOrEmpty(quest.giver) && !IsUpset(quest.giver))
            {
                upsetCharacters.Add(quest.giver);
                Debug.Log(quest.giver + " is now upset");
            }
        }
        else if (quest.type == QuestType.World)
        {
            if (quest.failGoldPenalty > 0) Wallet.Take(quest.failGoldPenalty);
            if (!string.IsNullOrEmpty(quest.failItemLost)) RemoveItem(quest.failItemLost, quest.failItemAmount);
        }
        else if (quest.type == QuestType.Story)
        {
            Debug.Log("Story quest failed, you die");
            storyFailed = true;
        }
    }

    public int DaysLeft(string id)
    {
        QuestData quest = FindQuest(id);
        if (quest == null) return -1;
        if (!quest.hasDeadline) return -1;
        if (GetStatus(id) != QuestStatus.Active) return -1;

        int deadlineDay = startDays[id] + quest.daysToComplete;
        return deadlineDay - CurrentDay();
    }

    public bool IsUpset(string character)
    {
        if (string.IsNullOrEmpty(character)) return false;
        return upsetCharacters.Contains(character);
    }

    private void SetStatus(QuestData quest, QuestStatus status)
    {
        statuses[quest.id] = status;
        Debug.Log("Quest '" + quest.title + "' is now " + status);
        OnQuestStatusChanged?.Invoke(quest, status);
    }

    private void GameOver()
    {
        Debug.Log("Game over, starting over from day 1");
        ResetAll();
        if (DayCycleManager.Instance != null) DayCycleManager.Instance.LoadDay(1);
    }

    private int CurrentDay()
    {
        if (DayCycleManager.Instance == null) return 1;
        return DayCycleManager.Instance.Day;
    }

    public int GetItemCount(string item)
    {
        if (items.ContainsKey(item)) return items[item];
        return 0;
    }

    public void AddItem(string item, int amount)
    {
        items[item] = GetItemCount(item) + amount;
        Debug.Log("Got " + amount + " " + item + ", now has " + items[item]);
    }

    public void RemoveItem(string item, int amount)
    {
        items[item] = Mathf.Max(0, GetItemCount(item) - amount);
        Debug.Log("Lost " + amount + " " + item + ", now has " + items[item]);
    }

    public void ResetAll()
    {
        statuses.Clear();
        startDays.Clear();
        items.Clear();
        upsetCharacters.Clear();
        Wallet.Set(0);
    }

    [ContextMenu("Test: Give snake and amethyst")]
    private void TestGiveItems()
    {
        AddItem("five_headed_snake", 1);
        AddItem("amethyst", 1);
    }
}