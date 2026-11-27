using UnityEngine;

public enum ProductCategory
{
    Junk,
    Medicine,
    Ammo,
    Weapon,
    Material,
    Relic
}

// rarity is what actually drives the final price, see ShopSettingsSO for the multipliers
public enum ProductRarity
{
    Common,
    Uncommon,
    Rare,
    Legendary
}

[CreateAssetMenu(fileName = "Product", menuName = "Shop/Product")]
public class ProductSO : ScriptableObject
{
    public string productName;
    public ProductCategory category;
    public ProductRarity rarity;
    [Min(0)] public int basePrice = 10;
    public Sprite icon;

    [Tooltip("Visual only. Gets spawned on the ground, on the shelf and in the customer's hand")]
    public GameObject displayPrefab;

    // NOTE: when the inventory (ItemSO) gets merged this is where the two get linked,
    // either add an ItemSO reference here or make ProductSO inherit from ItemSO

    public string DisplayName => string.IsNullOrEmpty(productName) ? name : productName;
}
