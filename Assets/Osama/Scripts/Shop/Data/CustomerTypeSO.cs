using UnityEngine;

// one asset per kind of customer. Normal villagers are random shoppers,
// special ones (nurse, kid, old man...) can come in looking for specific items
[CreateAssetMenu(fileName = "CustomerType", menuName = "Shop/Customer Type")]
public class CustomerTypeSO : ScriptableObject
{
    public string displayName = "Villager";
    [Tooltip("Story NPCs. Nothing special happens yet, this is the hook for dialogue/quests")]
    public bool isSpecial;

    [Header("Spawning")]
    [Min(0f)] public float spawnWeight = 1f;

    [Header("Behaviour")]
    [Min(0.1f)] public float moveSpeed = 2.5f;
    [Min(1)] public int minItems = 1;
    [Min(1)] public int maxItems = 2;
    [Tooltip("Seconds spent standing at the shelf before grabbing the product")]
    [Min(0f)] public float pickDelay = 1f;
    [Tooltip("Seconds they wait in the checkout line before giving up. 0 = wait forever")]
    [Min(0f)] public float patience = 60f;

    [Header("What they want")]
    [Tooltip("Leave empty for 'anything on the shelves'. Fill it for quest items etc")]
    public ProductSO[] wantedProducts;

    [Header("Look (placeholder until we get real characters)")]
    [Tooltip("Optional. Replaces the capsule on the customer prefab")]
    public GameObject visualPrefab;
    public Color bodyColor = Color.gray;
    public Color maskColor = Color.white;

    public int RollItemCount()
    {
        return Random.Range(minItems, Mathf.Max(minItems, maxItems) + 1);
    }

    public bool Wants(ProductSO product)
    {
        if (wantedProducts == null || wantedProducts.Length == 0) return true;
        return System.Array.IndexOf(wantedProducts, product) >= 0;
    }
}
