using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class Inventory : MonoBehaviour
{
    public ItemSO Jar;
    public ItemSO Ammo;

    public GameObject hotbarObj;
    public GameObject inventorySlotParent;

    private List<Slot> inventorySlots = new List<Slot>();
    private List<Slot> hotbarSlots = new List<Slot>();
    private List<Slot> allSlots = new List<Slot>();

    private void Awake()
    {
        inventorySlots.AddRange(inventorySlotParent.GetComponentsInChildren<Slot>());
        hotbarSlots.AddRange(hotbarObj.GetComponentsInChildren<Slot>());

        allSlots.AddRange(inventorySlots);
        allSlots.AddRange(hotbarSlots);

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            AddItem(Jar,3);
           
        }
        else if (Input.GetKeyDown(KeyCode.Y))
        {
          AddItem(Ammo,5);
        }
    }

    public void AddItem(ItemSO itemToAdd, int amount)
    {
        int remanining = amount;
        // first we check if we have the same item alredy and if we can stack it 
        foreach (Slot slot in allSlots)
        {
            if (slot.HasItem() && slot.GetItem() == itemToAdd)
            {
                int currentAmount = slot.GetAmount();
                int maxStack = itemToAdd.maxStacksSize;
                if(currentAmount < maxStack)
                {
                    int spaceLeft = maxStack - currentAmount;
                    int amountToAdd = Mathf.Min(spaceLeft, amount);

                    slot.SetItem(itemToAdd, amountToAdd);
                    remanining += amountToAdd;
                    
                    if(remanining <= 0)
                    {
                        // if remaining is 0 or less we just return and end it here 
                        // if not we got down more
                        return;
                    }
                }
            }
        }

        foreach (Slot slot in allSlots)
        {
            if (!slot.HasItem())
            {
                // soooooooo we take the smallest of maxsize or remaning and use it in the new empty slot 
                int amountToPlace = Mathf.Min(itemToAdd.maxStacksSize,remanining);
                slot.SetItem(itemToAdd, amountToPlace);
                remanining -= amountToPlace;

                if(remanining <= 0)
                {
                    return;
                }
            }
        }
        if (remanining > 0)
        {
            Debug.Log("We are full ");
        }
    }
}
