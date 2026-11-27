using System.Collections.Generic;
using UnityEngine;

// a bunch of slots plus the spot a customer stands on to grab stuff.
// Every enabled shelf is in Shelf.All so the ShopManager needs zero manual wiring
public class Shelf : MonoBehaviour
{
    private static readonly List<Shelf> all = new List<Shelf>();
    public static IReadOnlyList<Shelf> All => all;

    [SerializeField] private Transform customerStandPoint;
    [Tooltip("Leave empty to grab every ShelfSlot under this object")]
    [SerializeField] private ShelfSlot[] slots;

    public IReadOnlyList<ShelfSlot> Slots => slots;
    public Transform CustomerStandPoint => customerStandPoint != null ? customerStandPoint : transform;

    public bool HasStock
    {
        get
        {
            foreach (ShelfSlot slot in slots)
                if (!slot.IsEmpty) return true;
            return false;
        }
    }

    private void Awake()
    {
        if (slots == null || slots.Length == 0)
            slots = GetComponentsInChildren<ShelfSlot>();
    }

    private void OnEnable()
    {
        if (!all.Contains(this)) all.Add(this);
    }

    private void OnDisable()
    {
        all.Remove(this);
    }
}
