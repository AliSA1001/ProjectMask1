using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;

public class Inventory : MonoBehaviour
{
    public ItemSO Jar;
    public ItemSO Ammo;

    public GameObject hotbarObj;
    public GameObject inventorySlotParent;
    public GameObject container;

    public Image dragIcon;

    private List<Slot> inventorySlots = new List<Slot>();
    private List<Slot> hotbarSlots = new List<Slot>();
    private List<Slot> allSlots = new List<Slot>();

    private Slot draggedSlot = null;
    private bool isDraging = false;
    // new input system helper vaule 
    private bool isSelectingItem = false;

    private void Awake()
    {
        inventorySlots.AddRange(inventorySlotParent.GetComponentsInChildren<Slot>());
        hotbarSlots.AddRange(hotbarObj.GetComponentsInChildren<Slot>());

        allSlots.AddRange(inventorySlots);
        allSlots.AddRange(hotbarSlots);

    }

    private void Update()
    {
       /* if (Input.GetKeyDown(KeyCode.T))
        {
            AddItem(Jar,1);
           
        }
        else if (Input.GetKeyDown(KeyCode.Y))
        {
          AddItem(Ammo,5);
       */// }

        StartDrag();
        UpdateDragItemPosition();
        EndDrag();
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
                    int amountToAdd = Mathf.Min(spaceLeft, remanining);

                    slot.SetItem(itemToAdd,currentAmount +  amountToAdd);
                    remanining -= amountToAdd;
                    
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
    private void StartDrag()
    {

        if(isDraging) return;


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

            if(hoverd != null)
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
    private void HandleDrop(Slot from,Slot to)
    {
        if(from == to) return;

        //Stacking
        if(to.HasItem() && to.GetItem() == from.GetItem ()) 
        {
            int max = to.GetItem().maxStacksSize;
            int space = max - to.GetAmount();

            if(space > 0)
            {
                int move = Mathf.Min(space, from.GetAmount());
                
                to.SetItem(to.GetItem(), to.GetAmount() + move);
                from.SetItem(from.GetItem(), from.GetAmount() - move);

                if(from.GetAmount() <= 0)
                {
                    from.ClearSlot();

                    return;
                }

            }
            return;
        }
        
        //Diffrent Item
        if(to.HasItem() && to.GetItem() != from.GetItem()) 
        {
         ItemSO tempItem = to.GetItem();
            int tempAmount = to.GetAmount();

            to.SetItem(from.GetItem(), from.GetAmount());
            from.SetItem(tempItem, tempAmount);
            return;
        }

        //Empty Slot
        to.SetItem(from.GetItem(), from.GetAmount());
        from.ClearSlot();
    }

    private void UpdateDragItemPosition()
    {
        dragIcon.transform.position = Input.mousePosition;
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
    public void OnOpenInventory(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            container.SetActive(!container.activeInHierarchy);
            // if it is Locked then we open the cursor and if not we locked it 
            Cursor.lockState = CursorLockMode.Locked == CursorLockMode.Locked? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = !Cursor.visible;
        }
    }



}
