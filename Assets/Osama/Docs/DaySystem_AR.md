# سيستم الأيام - شرح وربط مع الديالوق

## الفكرة
ساعة للعبة كلها: رقم اليوم، الساعة، والمرحلة (صباح، نهار، مساء، ليل). المتجر يقرأ منها ليفتح ويقفل، والديالوق والمهمات تقرأ منها لتعرف أي يوم وأي وقت.

الملفات في `Assets/Osama/Scripts/Day`:

| الملف | وظيفته |
|---|---|
| `DaySettingsSO` | الأرقام: سرعة الوقت، متى تبدأ كل مرحلة، ساعة الاستيقاظ، ألوان السماء في الواجهة |
| `DayManager` | الساعة نفسها. واحد في اللعبة، يبقى بين السينات |
| `DayTimeHUD` | الواجهة أعلى اليسار |
| `SleepSpot` | السرير: نهاراً يقفز إلى الليل، ليلاً ينام حتى الصباح |

## الأرقام الافتراضية
في `Assets/Osama/Data/DaySettings`:
- الساعة الواحدة في اللعبة = 30 ثانية حقيقية في سين التجربة (60 في الإعداد الافتراضي للكود).
- الصباح من 6:00، النهار من 10:00، المساء من 17:00، الليل من 20:00.
- تبدأ اللعبة يوم 1 الساعة 8:00، والاستيقاظ بعد النوم 8:00.
- **العدّاد يزيد عند بداية الصباح (6:00) وليس عند منتصف الليل**، يعني الليل يتبع اليوم اللي بدأ فيه. الساعة 2:00 بالليل ما زلنا في نفس اليوم.

## كيف تقرأ منه من أي سكربت
```csharp
DayManager day = DayManager.instance;
day.Day        // 1, 2, 3 ...
day.Hour       // 0 إلى 24، مثلاً 13.5 يعني 13:30
day.TimeText   // "13:30"
day.Phase      // DayPhase.Morning / Day / Evening / Night
day.IsNight    // true بالليل
```

## الأحداث
```csharp
DayManager.instance.OnDayChanged   += (int newDay) => { ... };      // بداية يوم جديد (عند 6:00)
DayManager.instance.OnPhaseChanged += (DayPhase phase) => { ... };  // تغيرت المرحلة
DayManager.instance.OnMinuteChanged += () => { ... };               // كل دقيقة لعبة، للواجهات
```
اشترك في `Start()` وألغِ الاشتراك في `OnDestroy()`. ملاحظة: القفزة الكبيرة (النوم مثلاً) تطلق الحدث بالحالة النهائية فقط، المراحل اللي بينها ما تطلق أحداث.

## تحريك الوقت
```csharp
DayManager.instance.AdvanceMinutes(30);   // أي فعل يكلف وقت (استكشاف، شراء، حوار طويل)
DayManager.instance.SkipToNight();        // تخطي اليوم، الليل لازم يُلعب
DayManager.instance.SkipToNextDay();      // النوم أو النجاة من الليل، يوصلك لصباح اليوم التالي
DayManager.instance.RunClock = false;     // تجميد الوقت (قائمة، حوار، مشهد)
```

## علاقته بالمتجر
`ShopManager` فيه خيار `closeAtNight`. لو فيه `DayManager` في السين، المحل يفتح ويقفل معه لحاله. مقفل يعني ما يدخل زبائن جدد، والموجودين يكملون ويطلعون.

## الربط مع الديالوق (Yarn Spinner)
هذا السكربت ينحط في الفرع اللي فيه Yarn Spinner (ما ينفع في فرع بدونه لأن `Yarn.Unity` غير موجود). يعرّف دوال وأوامر تقدر تستخدمها داخل ملفات `.yarn` مباشرة:

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

وفي ملف الحوار:
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

تجميد الوقت أثناء الحوار: في `DialogueRunner` اربط `onDialogueStart` بدالة تسوي `DayManager.instance.RunClock = false` و`onDialogueComplete` بدالة ترجعه `true`.

## أمثلة استخدام للمهمات
- مهمة تفتح من يوم معين: اقرأ `Day` عند بداية الحوار.
- شخصية تظهر في وقت محدد: اشترك في `OnPhaseChanged` وشغّل أو أخفِ الشخصية.
- الزبون الخاص (الرجل العجوز) يجي في يوم معين: `CustomerSpawner.Spawn(type)` من سكربت يسمع `OnDayChanged`.
