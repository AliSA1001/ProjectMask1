using System;
using System.Collections.Generic;
using UnityEngine;

// what the player is holding right now. Kept dead simple on purpose, when the inventory is ready
// this is the only class that has to change (make it read/write the inventory instead of this list)
public class PlayerCarry : MonoBehaviour
{
    public static PlayerCarry instance { get; private set; }

    [SerializeField, Min(1)] private int capacity = 6;

    private readonly List<ProductSO> items = new List<ProductSO>();

    public event Action OnChanged;

    public int Count => items.Count;
    public int Capacity => capacity;
    public bool IsFull => items.Count >= capacity;
    public bool IsEmpty => items.Count == 0;
    public IReadOnlyList<ProductSO> Items => items;

    // the one that gets placed next. Last picked up = first placed, like a stack in your arms
    public ProductSO Selected => items.Count > 0 ? items[items.Count - 1] : null;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    public bool TryAdd(ProductSO product)
    {
        if (product == null || IsFull) return false;

        items.Add(product);
        OnChanged?.Invoke();
        return true;
    }

    public ProductSO TakeSelected()
    {
        if (items.Count == 0) return null;

        ProductSO product = items[items.Count - 1];
        items.RemoveAt(items.Count - 1);
        OnChanged?.Invoke();
        return product;
    }
}
