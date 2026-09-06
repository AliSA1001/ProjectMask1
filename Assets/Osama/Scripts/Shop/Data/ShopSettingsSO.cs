using UnityEngine;

// global shop tuning. One asset for the whole game so nobody has to dig through scene objects
[CreateAssetMenu(fileName = "ShopSettings", menuName = "Shop/Shop Settings")]
public class ShopSettingsSO : ScriptableObject
{
    public string currencySymbol = "$";
    [Min(0)] public int startingMoney = 0;

    [Header("Price multiplier per rarity (Common, Uncommon, Rare, Legendary)")]
    public float[] rarityMultipliers = { 1f, 1.5f, 2.5f, 5f };

    public float GetRarityMultiplier(ProductRarity rarity)
    {
        int i = (int)rarity;
        if (rarityMultipliers == null || i >= rarityMultipliers.Length) return 1f;
        return rarityMultipliers[i];
    }

    // the final selling price. If we ever let the player set his own prices, start here
    public int GetSellPrice(ProductSO product)
    {
        if (product == null) return 0;
        return Mathf.Max(1, Mathf.RoundToInt(product.basePrice * GetRarityMultiplier(product.rarity)));
    }

    public string FormatMoney(int amount)
    {
        return currencySymbol + amount;
    }
}
