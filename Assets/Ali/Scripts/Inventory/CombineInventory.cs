using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CombineInventory : MonoBehaviour
{
    public static CombineInventory Instance;
    [SerializeField] private Inventory inventory_player;


    private GameObject Continer;

    public GameObject inventorySlotParent_Player;
    public GameObject inventorySlotParent_OtherInventory;
    public GameObject HotBarObjet;

    private List<Slot> inventorySlots_Player = new List<Slot>();
    private List<Slot> inventorySlots_OtherInventory = new List<Slot>();
    private List<Slot> hotBarSlots = new List<Slot>();




    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }


        inventorySlots_Player.AddRange(inventorySlotParent_Player.GetComponentsInChildren<Slot>());
        inventorySlots_OtherInventory.AddRange(inventorySlotParent_OtherInventory.GetComponentsInChildren<Slot>());
        hotBarSlots.AddRange(HotBarObjet.GetComponentsInChildren<Slot>());


    }

    private void Start()
    {
        Continer = transform.GetChild(0).gameObject;
    }
    public void OpenOtherInventory(List<Item> otherInvenotryItems)
    {
        Continer.SetActive(true);// we need to get our inventory info and the other npc inventory inventory

        HandleOtherInventoryData(otherInvenotryItems);

        HandlePlayerInventoryData(inventory_player.GetMainInventoryItemData());// we ask the main inventory for the data that it has

        HandlePlayerHotbarData(inventory_player.GetHotbarItemData());
       
        HandleCursorLock();
       
    
    }

   

    private void HandlePlayerHotbarData((List<Item>, List<int>) value)
    {
        List<Item> hotbaritems = new List<Item>();
        List<int> index = new List<int>();

        hotbaritems = value.Item1;
        index = value.Item2;

        foreach (int n in index)
        {
            Item itemData = hotbaritems[n];
            hotBarSlots[n].SetItem(itemData.item, itemData.amount, itemData.ammoAmount, itemData.durability);
        }
    }

    private void HandlePlayerInventoryData((List<Item>, List<int>) value)
    {
        List<Item> playerInventoryItems = new List<Item>();
        List<int> index = new List<int>();

        playerInventoryItems = value.Item1;
        index = value.Item2;

       foreach(int n in index)
        {
            Item itemData = playerInventoryItems[n];
            inventorySlots_Player[n].SetItem(itemData.item, itemData.amount, itemData.ammoAmount, itemData.durability);
        }
    }


    private void HandleOtherInventoryData(List<Item> otherInvenotryItems)
    {
        for (int i = 0; i < inventorySlots_OtherInventory.Count; i++)
        {
            if (i < otherInvenotryItems.Count)
            {
                Item itemData = otherInvenotryItems[i];
                inventorySlots_OtherInventory[i].SetItem(itemData.item, itemData.amount, itemData.ammoAmount, itemData.durability);

            }
            else
            {
                inventorySlots_OtherInventory[i].ClearSlot();

            }
        }
    }


    public void AddItem(ItemSO itemToAdd, int amount, int ammoAmount, float durability)
    {
        int remanining = amount;
        // first we check if we have the same item alredy and if we can stack it 
        foreach (Slot slot in inventorySlots_OtherInventory)
        {
            if (slot.HasItem() && slot.GetItem() == itemToAdd)
            {
                int currentAmount = slot.GetAmount();
                int maxStack = itemToAdd.maxStacksSize;
                if (currentAmount < maxStack)
                {
                    int spaceLeft = maxStack - currentAmount;
                    int amountToAdd = Mathf.Min(spaceLeft, remanining);

                    slot.SetItem(itemToAdd, currentAmount + amountToAdd, ammoAmount, durability);
                    remanining -= amountToAdd;

                    if (remanining <= 0)
                    {
                        // if remaining is 0 or less we just return and end it here 
                        // if not we got down more
                        return;
                    }
                }
            }
        }
        foreach (Slot slot in inventorySlots_OtherInventory)
        {
            if (!slot.HasItem())
            {
                // soooooooo we take the smallest of maxsize or remaning and use it in the new empty slot 
                int amountToPlace = Mathf.Min(itemToAdd.maxStacksSize, remanining);
                slot.SetItem(itemToAdd, amountToPlace, ammoAmount, durability);
                remanining -= amountToPlace;

                if (remanining <= 0)
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


    private void HandleCursorLock()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        Movement.instance.UpdatingRotation = !Movement.instance.UpdatingRotation;
    }
}

