# نظام الأيام

ساعة وحدة للعبة كلها: رقم اليوم، الساعة، والمرحلة (صبح / نهار / مسا / ليل). أي سكربت يقدر يقرأ منها أو يسمع تغيراتها.

الملفات في `Assets/Osama/Scripts/Day`:

| الملف | وظيفته |
|---|---|
| `DayManager` | الساعة. كائن واحد بالسين، ما ينحذف بين السينات |
| `DaySettingsSO` | الأرقام. الملف الجاهز في `Assets/Osama/Data/DaySettings` |
| `DayTimeHUD` | واجهة الساعة فوق يسار الشاشة |
| `SleepSpot` | السرير: نهاراً يقفز لليل، ليلاً ينام لين الصبح |

## الإعدادات الحالية

- الساعة الوحدة باللعبة = 30 ثانية حقيقية.
- الصبح 6:00، النهار 10:00، المسا 17:00، الليل 20:00.
- البداية: يوم 1 الساعة 8:00. الصحيان بعد النوم 8:00.
- العداد يزيد الساعة 6 الصبح، مو منتصف الليل. يعني الساعة 2 بالليل لسا نفس اليوم.

## القراءة

```csharp
DayManager day = DayManager.instance;

day.Day        // 1, 2, 3 ...
day.Hour       // 0 to 24, 13.5 means 13:30
day.TimeText   // "13:30"
day.Phase      // DayPhase.Morning / Day / Evening / Night
day.IsNight    // true at night
```

## الأحداث

```csharp
private void Start()
{
    DayManager.instance.OnDayChanged += HandleNewDay;      // void HandleNewDay(int day)
    DayManager.instance.OnPhaseChanged += HandlePhase;     // void HandlePhase(DayPhase phase)
    DayManager.instance.OnMinuteChanged += HandleMinute;   // void HandleMinute()
}

private void OnDestroy()
{
    if (DayManager.instance == null) return;
    DayManager.instance.OnDayChanged -= HandleNewDay;
    DayManager.instance.OnPhaseChanged -= HandlePhase;
    DayManager.instance.OnMinuteChanged -= HandleMinute;
}
```

- `OnDayChanged` مع بداية كل يوم (الساعة 6).
- `OnPhaseChanged` لما تتغير المرحلة.
- `OnMinuteChanged` كل دقيقة لعبة، للواجهات.

لا تنسى `-=` في `OnDestroy()`.

## تحريك الوقت

```csharp
DayManager.instance.AdvanceMinutes(30);   // an action that costs time
DayManager.instance.AdvanceHours(2);
DayManager.instance.SkipToNight();        // skip the rest of the day
DayManager.instance.SkipToNextDay();      // sleep, wake up next morning
DayManager.instance.RunClock = false;     // freeze, true to resume
```

## تجميد الوقت أثناء الحوار

في `DialogueRunner`:

```csharp
public void OnDialogueStart()
{
    if (DayManager.instance != null) DayManager.instance.RunClock = false;
}

public void OnDialogueEnd()
{
    if (DayManager.instance != null) DayManager.instance.RunClock = true;
}
```

اربط `OnDialogueStart` بحدث `onDialogueStart` و `OnDialogueEnd` بـ `onDialogueComplete`.

## الربط مع Yarn Spinner

هذا السكربت ينحط في الفرع اللي فيه يارن (مو موجود بفرعي). حطه على أي كائن بالسين:

```csharp
using UnityEngine;
using Yarn.Unity;

public class DayYarnBridge : MonoBehaviour
{
    [YarnFunction("day")]
    public static int Day()
    {
        return DayManager.instance != null ? DayManager.instance.Day : 1;
    }

    [YarnFunction("hour")]
    public static float Hour()
    {
        return DayManager.instance != null ? DayManager.instance.Hour : 0f;
    }

    [YarnFunction("phase")]
    public static string Phase()
    {
        return DayManager.instance != null ? DayManager.instance.Phase.ToString() : "Day";
    }

    [YarnFunction("is_night")]
    public static bool IsNight()
    {
        return DayManager.instance != null && DayManager.instance.IsNight;
    }

    [YarnCommand("advance_time")]
    public static void AdvanceTime(float minutes)
    {
        if (DayManager.instance != null) DayManager.instance.AdvanceMinutes(minutes);
    }

    [YarnCommand("skip_to_night")]
    public static void SkipToNight()
    {
        if (DayManager.instance != null) DayManager.instance.SkipToNight();
    }
}
```

`phase()` ترجع وحدة من: `Morning` أو `Day` أو `Evening` أو `Night`.

داخل ملف الحوار:

```
title: Nurse
---
<<if day() >= 3>>
    Nurse: The snake... did you find it?
<<else>>
    Nurse: Come back in a few days.
<<endif>>

<<if phase() == "Evening">>
    Nurse: It is getting late.
<<endif>>

<<advance_time 20>>
===
```

## ملاحظات

- القفزة الكبيرة (النوم) تطلق الحدث بالحالة الأخيرة بس. لو نمت من 14:00 لين 8:00 ما يجيك حدث "صار ليل" بالنص.
- `Hour` رقم عشري، 13.5 تعني 13:30. لو تبي نص جاهز استخدم `TimeText`.
- المحل يقفل ويفتح مع المراحل لحاله، ما يحتاج ربط من عندك.
