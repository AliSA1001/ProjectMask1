using Yarn.Unity;

public static class YarnCommands
{
    [YarnFunction("quest_status")]
    public static string GetQuestStatus(string quest)
    {
        if (QuestManager.Instance == null) return "NotStarted";
        return QuestManager.Instance.GetStatus(quest).ToString();
    }

    [YarnFunction("quest_ready")]
    public static bool QuestReady(string quest)
    {
        if (QuestManager.Instance == null) return false;
        return QuestManager.Instance.IsReady(quest);
    }

    [YarnFunction("days_left")]
    public static int DaysLeft(string quest)
    {
        if (QuestManager.Instance == null) return -1;
        return QuestManager.Instance.DaysLeft(quest);
    }

    [YarnFunction("is_upset")]
    public static bool IsUpset(string character)
    {
        if (QuestManager.Instance == null) return false;
        return QuestManager.Instance.IsUpset(character);
    }

    [YarnCommand("start_quest")]
    public static void StartQuest(string quest)
    {
        if (QuestManager.Instance == null) return;
        QuestManager.Instance.StartQuest(quest);
    }

    [YarnCommand("complete_quest")]
    public static void CompleteQuest(string quest)
    {
        if (QuestManager.Instance == null) return;
        QuestManager.Instance.CompleteQuest(quest);
    }

    [YarnCommand("fail_quest")]
    public static void FailQuest(string quest)
    {
        if (QuestManager.Instance == null) return;
        QuestManager.Instance.FailQuest(quest);
    }

    [YarnFunction("has_item")]
    public static bool HasItem(string item, int amount)
    {
        if (QuestManager.Instance == null) return false;
        return QuestManager.Instance.GetItemCount(item) >= amount;
    }

    [YarnCommand("give_item")]
    public static void GiveItem(string item, int amount)
    {
        if (QuestManager.Instance == null) return;
        QuestManager.Instance.AddItem(item, amount);
    }

    [YarnCommand("take_item")]
    public static void TakeItem(string item, int amount)
    {
        if (QuestManager.Instance == null) return;
        QuestManager.Instance.RemoveItem(item, amount);
    }

    [YarnCommand("give_gold")]
    public static void GiveGold(int amount)
    {
        Wallet.Add(amount);
    }

    [YarnFunction("day")]
    public static int Day()
    {
        if (DayCycleManager.Instance == null) return 1;
        return DayCycleManager.Instance.Day;
    }

    [YarnFunction("is_night")]
    public static bool IsNight()
    {
        if (DayCycleManager.Instance == null) return false;
        return DayCycleManager.Instance.IsNight;
    }

    [YarnCommand("end_day")]
    public static void EndDay()
    {
        DialogueController.EndDayWhenFinished = true;
    }
}