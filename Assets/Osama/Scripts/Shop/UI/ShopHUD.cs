using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// money, what the player is carrying, open/closed and a little "+$17" when a sale goes through.
// All fields are optional, leave one empty and it gets skipped
public class ShopHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text carryText;
    [SerializeField] private TMP_Text shopStateText;
    [SerializeField] private TMP_Text saleText;
    [SerializeField] private float salePopupTime = 2.5f;

    private Coroutine saleRoutine;

    private void Start()
    {
        if (ShopManager.instance != null)
        {
            ShopManager.instance.OnMoneyChanged += UpdateMoney;
            ShopManager.instance.OnOpenChanged += UpdateShopState;
            ShopManager.instance.OnSaleCompleted += ShowSale;
            UpdateMoney(ShopManager.instance.Money);
            UpdateShopState(ShopManager.instance.IsOpen);
        }

        if (PlayerCarry.instance != null)
            PlayerCarry.instance.OnChanged += UpdateCarry;

        if (saleText != null) saleText.gameObject.SetActive(false);
        UpdateCarry();
    }

    private void OnDestroy()
    {
        if (ShopManager.instance != null)
        {
            ShopManager.instance.OnMoneyChanged -= UpdateMoney;
            ShopManager.instance.OnOpenChanged -= UpdateShopState;
            ShopManager.instance.OnSaleCompleted -= ShowSale;
        }
        if (PlayerCarry.instance != null) PlayerCarry.instance.OnChanged -= UpdateCarry;
    }

    private void UpdateMoney(int money)
    {
        if (moneyText != null) moneyText.text = ShopManager.FormatMoney(money);
    }

    private void UpdateShopState(bool open)
    {
        if (shopStateText != null) shopStateText.text = open ? "OPEN" : "CLOSED";
    }

    private void ShowSale(Customer customer, int total)
    {
        if (saleText == null) return;

        string who = customer != null && customer.Type != null ? customer.Type.displayName : "Customer";
        saleText.text = $"+{ShopManager.FormatMoney(total)}   {who}";
        saleText.gameObject.SetActive(true);

        if (saleRoutine != null) StopCoroutine(saleRoutine);
        saleRoutine = StartCoroutine(HideSaleLater());
    }

    private IEnumerator HideSaleLater()
    {
        yield return new WaitForSeconds(salePopupTime);
        saleText.gameObject.SetActive(false);
        saleRoutine = null;
    }

    private void UpdateCarry()
    {
        if (carryText == null) return;

        PlayerCarry carry = PlayerCarry.instance;
        if (carry == null || carry.IsEmpty)
        {
            carryText.text = "Hands: empty";
            return;
        }

        // group the same products together: "Old Jar x2, Pistol Ammo"
        Dictionary<ProductSO, int> counts = new Dictionary<ProductSO, int>();
        foreach (ProductSO p in carry.Items)
            counts[p] = counts.TryGetValue(p, out int n) ? n + 1 : 1;

        List<string> parts = new List<string>();
        foreach (KeyValuePair<ProductSO, int> kv in counts)
            parts.Add(kv.Value > 1 ? $"{kv.Key.DisplayName} x{kv.Value}" : kv.Key.DisplayName);

        carryText.text = $"Hands ({carry.Count}/{carry.Capacity}): {string.Join(", ", parts)}\nNext to place: {carry.Selected.DisplayName}";
    }
}
