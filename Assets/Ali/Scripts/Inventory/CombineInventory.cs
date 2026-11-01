using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
    private List<Slot> allSlots = new List<Slot>();


    public Image dragIcon;
    private Slot draggedSlot = null;
    private bool isDraging = false;
    private bool isSelectingItem = false;


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

        CombainAllSlots();
       
        HandleCursorLock();
       
    
    }

    private void Update()
    {
        StartDrag();
        UpdateDragItemPosition();
        EndDrag();
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
    private void CombainAllSlots()
    {
        allSlots.AddRange(inventorySlots_OtherInventory);
        allSlots.AddRange(inventorySlots_Player);
        allSlots.AddRange(hotBarSlots);
    }

    private void StartDrag()
    {

        if (isDraging) return;


        if (isSelectingItem)
        {
            Slot hovered = GetHoverdSlot();

            if (hovered != null && hovered.HasItem())
            {
                draggedSlot = hovered;
                isDraging = true;

                //show the drag item 
                dragIcon.sprite = hovered.GetItem().icon;
                dragIcon.color = new Color(1, 1, 1, 0.5f);
                dragIcon.enabled = true;
            }
        }
    }
    private void EndDrag()
    {
        if (!isSelectingItem && isDraging)
        {
            Slot hoverd = GetHoverdSlot();

            if (hoverd != null)
            {
                HandleDrop(draggedSlot, hoverd);

                dragIcon.enabled = false;

                draggedSlot = null;
                isDraging = false;

            }
        }

    }
    private Slot GetHoverdSlot()
    {
        foreach (Slot slot in allSlots)
        {
            if (slot.hovering)
            {
                return slot;
            }
        }
        return null;
    }
    private void HandleDrop(Slot from, Slot to)
    {
        if (from == to) return;

        //Stacking
        if (to.HasItem() && to.GetItem() == from.GetItem() && to.GetItem().maxStacksSize > 1)
        {
            int max = to.GetItem().maxStacksSize;
            int space = max - to.GetAmount();

            if (space > 0)
            {
                int move = Mathf.Min(space, from.GetAmount());

                to.SetItem(to.GetItem(), to.GetAmount() + move, to.GetAmmoAmount(), to.GetDurability());
                from.SetItem(from.GetItem(), from.GetAmount() - move, from.GetAmmoAmount(), from.GetDurability());

                if (from.GetAmount() <= 0)
                {
                    from.ClearSlot();

                    return;
                }

            }
            return;
        }

        //Diffrent Item
        if (to.HasItem() && (to.GetItem() != from.GetItem() || (to.GetItem().maxStacksSize <= 1 && from.GetItem().maxStacksSize <= 1)))
        {
            ItemSO tempItem = to.GetItem();
            int tempAmount = to.GetAmount();
            int tempAmmoAmount = to.GetAmmoAmount();
            float tempDurability = to.GetDurability();

            to.SetItem(from.GetItem(), from.GetAmount(), from.GetAmmoAmount(), from.GetDurability());
            from.SetItem(tempItem, tempAmount, tempAmmoAmount, tempDurability);
            

            return;
        }

        //Empty Slot
        to.SetItem(from.GetItem(), from.GetAmount(), from.GetAmmoAmount(), from.GetDurability());
        from.ClearSlot();

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

    private void UpdateDragItemPosition()
    {
        dragIcon.transform.position = Input.mousePosition;
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
    public void OnSlectingItem(InputAction.CallbackContext context)
    {
        if (context.performed && Cursor.lockState != CursorLockMode.Locked)
        {
            isSelectingItem = true;
        }
        if (context.canceled)
        {
            isSelectingItem = false;
        }
    }
}

