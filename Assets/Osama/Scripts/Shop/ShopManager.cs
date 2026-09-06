using System;
using System.Collections.Generic;
using UnityEngine;

// central place for money, open/closed and "where can a customer find something to buy".
// Shelves register themselves (Shelf.All) so the only thing to wire here is the settings asset
public class ShopManager : MonoBehaviour
{
    public static ShopManager instance { get; private set; }

    [SerializeField] private ShopSettingsSO settings;
    [Tooltip("Found automatically if empty")]
    [SerializeField] private CheckoutCounter checkout;
    [Tooltip("Used when there is no DayManager in the scene")]
    [SerializeField] private bool openAtStart = true;
    [Tooltip("Follow the DayManager: open during the day, closed at night")]
    [SerializeField] private bool closeAtNight = true;

    public ShopSettingsSO Settings => settings;
    public CheckoutCounter Checkout => checkout;
    public int Money { get; private set; }
    public bool IsOpen { get; private set; }

    public event Action<int> OnMoneyChanged;
    public event Action<Customer, int> OnSaleCompleted;
    public event Action<bool> OnOpenChanged;

    private readonly List<ShelfSlot> candidates = new List<ShelfSlot>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        if (settings == null)
        {
            Debug.LogWarning("ShopManager has no ShopSettings asset, using defaults", this);
            settings = ScriptableObject.CreateInstance<ShopSettingsSO>();
        }
        if (checkout == null) checkout = FindFirstObjectByType<CheckoutCounter>();

        Money = settings.startingMoney;
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
        if (DayManager.instance != null) DayManager.instance.OnPhaseChanged -= HandlePhaseChanged;
    }

    private void Start()
    {
        if (closeAtNight && DayManager.instance != null)
        {
            DayManager.instance.OnPhaseChanged += HandlePhaseChanged;
            SetOpen(!DayManager.instance.IsNight);
        }
        else if (openAtStart)
        {
            SetOpen(true);
        }
    }

    private void HandlePhaseChanged(DayPhase phase)
    {
        SetOpen(phase != DayPhase.Night);
    }

    // closed just means no new customers show up, the ones inside finish what they were doing
    public void SetOpen(bool open)
    {
        if (IsOpen == open) return;
        IsOpen = open;
        OnOpenChanged?.Invoke(open);
    }

    public int GetSellPrice(ProductSO product)
    {
        return settings.GetSellPrice(product);
    }

    public static string FormatMoney(int amount)
    {
        if (instance != null && instance.settings != null) return instance.settings.FormatMoney(amount);
        return "$" + amount;
    }

    public void AddMoney(int amount)
    {
        if (amount == 0) return;
        Money += amount;
        OnMoneyChanged?.Invoke(Money);
    }

    // for buying stuff from the other shops in town later on
    public bool TrySpend(int amount)
    {
        if (amount > Money) return false;
        AddMoney(-amount);
        return true;
    }

    public void CompleteSale(Customer customer, int total)
    {
        AddMoney(total);
        OnSaleCompleted?.Invoke(customer, total);
    }

    // picks a stocked slot for the customer. Random on purpose so they spread over the shelves,
    // if you want smarter customers (closest shelf, budget, ...) this is the place
    public ShelfSlot FindSlotFor(Customer customer)
    {
        candidates.Clear();

        foreach (Shelf shelf in Shelf.All)
        {
            foreach (ShelfSlot slot in shelf.Slots)
            {
                if (slot.IsEmpty) continue;
                if (slot.IsReserved && slot.ReservedBy != customer) continue;
                if (!customer.Wants(slot.Product)) continue;
                candidates.Add(slot);
            }
        }

        if (candidates.Count == 0) return null;
        return candidates[UnityEngine.Random.Range(0, candidates.Count)];
    }
}
