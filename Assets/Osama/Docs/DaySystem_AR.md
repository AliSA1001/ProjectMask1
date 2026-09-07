# سيستم الأيام

هذا الملف للي بيربط الديالوق (أو أي شي ثاني) بالوقت داخل اللعبة.

الفكرة ببساطة: عندنا ساعة وحدة للعبة كلها، فيها رقم اليوم والساعة والمرحلة (صبح / نهار / مسا / ليل). المحل يقرأ منها عشان يفتح ويقفل، والديالوق والمهمات يقرون منها عشان يعرفون احنا في أي يوم وأي وقت.

## وين الملفات

كلها في `Assets/Osama/Scripts/Day`:

- `DayManager` هو الساعة نفسها. كائن واحد بس في اللعبة كلها، وما ينحذف لما تنتقل بين السينات عشان رقم اليوم يوصل لسين الليل.
- `DaySettingsSO` فيه الأرقام كلها: سرعة الوقت، متى تبدأ كل مرحلة، ساعة الصحيان، ألوان السما في الواجهة. الملف الجاهز موجود في `Assets/Osama/Data/DaySettings`.
- `DayTimeHUD` الواجهة اللي فوق يسار الشاشة.
- `SleepSpot` السرير. بالنهار يوديك للليل، وبالليل ينومك لين الصبح.

## الأرقام الحالية

- الساعة الوحدة في اللعبة = 30 ثانية حقيقية في سين التجربة. لو ما حطيت ملف إعدادات الكود يستخدم 60.
- الصبح يبدأ 6:00، النهار 10:00، المسا 17:00، الليل 20:00.
- اللعبة تبدأ يوم 1 الساعة 8:00، ولما تنام تصحى 8:00.
- ترى العداد يزيد الساعة 6 الصبح مو 12 بالليل. يعني لو الساعة 2 بالليل احنا لسا في نفس اليوم. سويتها كذا عشان الليل يكون محسوب على اليوم اللي بدأ فيه وما تصير لخبطة في المهمات.

## كيف تقرأ منه

من أي سكربت:

```csharp
DayManager day = DayManager.instance;

day.Day        // 1, 2, 3 ...
day.Hour       // 0 to 24, 13.5 means 13:30
day.TimeText   // "13:30"
day.Phase      // DayPhase.Morning / Day / Evening / Night
day.IsNight    // true at night
```

## لو تبغى تسمع للتغييرات

فيه ثلاث أحداث:

```csharp
DayManager.instance.OnDayChanged += HandleNewDay;      // void HandleNewDay(int day)
DayManager.instance.OnPhaseChanged += HandlePhase;     // void HandlePhase(DayPhase phase)
DayManager.instance.OnMinuteChanged += HandleMinute;   // void HandleMinute()
```

الأول يشتغل مع بداية كل يوم (الساعة 6)، الثاني لما تتغير المرحلة، والثالث كل دقيقة لعبة وهذا للواجهات بس.

اشترك في `Start()` ولا تنسى تلغي الاشتراك في `OnDestroy()` بـ `-=` وإلا تطلع لك أخطاء لما ينحذف الكائن.

نقطة مهمة: لو الوقت قفز قفزة كبيرة زي النوم، ما يطلع إلا حدث واحد بالحالة الأخيرة. يعني لو نمت من 14:00 لين 8:00 الصبح ما راح يجيك حدث "صار ليل" في النص.

## تحريك الوقت

```csharp
DayManager.instance.AdvanceMinutes(30);   // an action that costs time
DayManager.instance.SkipToNight();        // skip the day, the night still has to be played
DayManager.instance.SkipToNextDay();      // sleep / survive the night -> next morning
DayManager.instance.RunClock = false;     // freeze the clock (menu, dialogue, cutscene)
```

`AdvanceMinutes` هي اللي تستخدمها لو تبغى فعل يكلف وقت زي ما مكتوب في الـ GDD: استكشاف، شراء، حوار طويل، أي شي.

## علاقته بالمحل

`ShopManager` فيه خانة اسمها `closeAtNight`. لو فيه `DayManager` بالسين المحل يفتح ويقفل لحاله. مقفل يعني ما يدخل زباين جدد بس، اللي جوا يكملون ويطلعون عادي.

## الربط مع Yarn Spinner

هذا سكربت جاهز، انسخه في الفرع اللي فيه Yarn Spinner. عندي ما راح يشتغل لأن الحزمة مو موجودة في فرعي. يعطيك دوال وأوامر تستخدمها داخل ملفات `.yarn` على طول:

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

وفي ملف الحوار تستخدمها كذا:

```
title: Nurse
---
<<if day() >= 3>>
    Nurse: The snake... did you find it?
<<else>>
    Nurse: Come back in a few days.
<<endif>>

<<if phase() == "Evening">>
    Nurse: It's getting late.
<<endif>>

<<advance_time 20>>
===
```

آخر شي، لو تبغى الوقت يوقف وقت الحوار: في `DialogueRunner` اربط `onDialogueStart` بدالة تسوي `DayManager.instance.RunClock = false` و`onDialogueComplete` بدالة ترجعه `true`. شغل دقيقتين.

## أفكار للمهمات

- مهمة تفتح من يوم معين: اقرأ `Day` أول ما يبدأ الحوار.
- شخصية تظهر بوقت معين: اشترك في `OnPhaseChanged` وفعّلها أو اخفها.
- الرجل العجوز يجي يوم معين: سكربت يسمع `OnDayChanged` ويستدعي `CustomerSpawner.Spawn(type)`.

لو شي مو واضح كلمني.
