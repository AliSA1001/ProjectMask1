using System;
using TMPro;
using UnityEngine;

// one spot on a shelf. Holds a single product, shows the model and a little price tag under it
public class ShelfSlot : MonoBehaviour, IInteractable, IInteractionPrompt
{
    [Tooltip("Where the model gets spawned. Defaults to this transform")]
    [SerializeField] private Transform anchor;
    [SerializeField] private TMP_Text priceLabel; // optional

    public Shelf Shelf { get; private set; }
    public ProductSO Product { get; private set; }
    public int Price { get; private set; } // locked in when the product is placed
    public Customer ReservedBy { get; private set; }

    public bool IsEmpty => Product == null;
    public bool IsReserved => ReservedBy != null;

    public event Action<ShelfSlot> OnChanged;

    private GameObject visual;

    private void Awake()
    {
        Shelf = GetComponentInParent<Shelf>();
        if (anchor == null) anchor = transform;
        RefreshLabel();
    }

    public bool TryPlace(ProductSO product)
    {
        if (product == null || !IsEmpty) return false;

        Product = product;
        Price = ShopManager.instance != null ? ShopManager.instance.GetSellPrice(product) : product.basePrice;

        SpawnVisual();
        RefreshLabel();
        OnChanged?.Invoke(this);
        return true;
    }

    // removes the product and hands it back. Used by the player and by customers
    public ProductSO Take()
    {
        if (IsEmpty) return null;

        ProductSO taken = Product;
        Product = null;
        Price = 0;
        ReservedBy = null;

        if (visual != null) Destroy(visual);
        visual = null;

        RefreshLabel();
        OnChanged?.Invoke(this);
        return taken;
    }

    // a customer reserves the slot while walking to it so two of them don't go for the same product
    public bool TryReserve(Customer customer)
    {
        if (IsEmpty) return false;
        if (IsReserved && ReservedBy != customer) return false;

        ReservedBy = customer;
        return true;
    }

    public void ReleaseReservation(Customer customer)
    {
        if (ReservedBy == customer) ReservedBy = null;
    }

    private void SpawnVisual()
    {
        if (Product.displayPrefab == null) return;

        visual = Instantiate(Product.displayPrefab, anchor.position, anchor.rotation, anchor);
        foreach (Collider c in visual.GetComponentsInChildren<Collider>()) c.enabled = false;
    }

    private void RefreshLabel()
    {
        if (priceLabel == null) return;
        priceLabel.text = IsEmpty ? "" : ShopManager.FormatMoney(Price);
    }

    // ---------- player ----------

    public string GetPrompt()
    {
        PlayerCarry carry = PlayerCarry.instance;

        if (IsEmpty)
        {
            if (carry == null || carry.Selected == null) return "Empty slot";

            int price = ShopManager.instance != null ? ShopManager.instance.GetSellPrice(carry.Selected) : carry.Selected.basePrice;
            return $"Empty slot\n>Place {carry.Selected.DisplayName} ({ShopManager.FormatMoney(price)})";
        }

        string info = $"{Product.DisplayName} - {ShopManager.FormatMoney(Price)}";
        if (carry == null || carry.IsFull) return info;

        return info + "\n>Take back";
    }

    public void Interact()
    {
        PlayerCarry carry = PlayerCarry.instance;
        if (carry == null) return;

        if (IsEmpty)
        {
            // put down whatever we are holding
            if (carry.Selected != null && TryPlace(carry.Selected))
                carry.TakeSelected();
        }
        else if (!carry.IsFull)
        {
            // take it back. If a customer was on its way here it just looks for another slot
            carry.TryAdd(Take());
        }
    }
}
