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

    public float pickupRange = 3f;
    private Item lookedAtItem = null;
    public Material hightlightMaterial;
    private Material originalMaterial;
    private Renderer lookedAtRenderer = null;

    private int equippedHotBarIndex = 0; // from 0 to 5
    public float equippedOpacity = 0.9f;
    public float normalOpacity = 0.58f;
    public Transform hand;
    private GameObject currentHandItem;

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

        DetectLookedAtItem();


        StartDrag();
        UpdateDragItemPosition();
        EndDrag();

        UpdateHotBarOpacity();

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

    private void DetectLookedAtItem()
    {
        if(lookedAtRenderer != null)
        {
            lookedAtRenderer.material = originalMaterial;
            lookedAtRenderer = null;
            originalMaterial = null;
        }

        Ray ray = new Ray(Camera.main.transform.position ,Camera.main.transform.forward);
        if(Physics.Raycast(ray,out RaycastHit hit,pickupRange))
        {
            Item item = hit.collider.GetComponent<Item>();
            if(item != null)
            {
                Renderer rend = item.GetComponent<Renderer>();
                if(rend != null)
                {
                    originalMaterial = rend.material;
                    rend.material = hightlightMaterial;
                    lookedAtRenderer = rend;
                }
            }
        }
    }

    private void UpdateHotBarOpacity()
    {
        for(int i = 0; i <hotbarSlots.Count; i++)
        {
            Image icon = hotbarSlots[i].GetComponent<Image>();
            if(icon != null)
            {
                icon.color = (i == equippedHotBarIndex) ? new Color(1,1,1,equippedOpacity) : new Color(1,1,1,normalOpacity);
            }
        }
    }

    private void EquipHandItem()
    {
        if (currentHandItem != null)
        {
            Destroy(currentHandItem);
        }

            Slot equppedSlot = hotbarSlots[equippedHotBarIndex];
            if(!equppedSlot.HasItem()) return;// if we have no item we just return 

            ItemSO item = equppedSlot.GetItem();
            if (item.handItemPrefab == null) return; // so if our item dont have hand prefab like ammo box

            currentHandItem = Instantiate(item.handItemPrefab, hand);
            currentHandItem.transform.localPosition = Vector3.zero;
            currentHandItem.transform.localRotation = Quaternion.identity;
            
        
    }

    public void OnHotBarSelection1(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            int selectedValue = 1;

            if (selectedValue > 0)
            {
                equippedHotBarIndex = selectedValue - 1;
                EquipHandItem();
            }
        }
    }
    public void OnHotBarSelection2(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            int selectedValue = 2;

            if (selectedValue > 0)
            {
                equippedHotBarIndex = selectedValue - 1;
                EquipHandItem();

            }
        }
    }
    public void OnHotBarSelection3(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            int selectedValue = 3;

            if (selectedValue > 0)
            {
                equippedHotBarIndex = selectedValue - 1;
                EquipHandItem();

            }
        }
    }
    public void OnHotBarSelection4(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            int selectedValue = 4;

            if (selectedValue > 0)
            {
                equippedHotBarIndex = selectedValue - 1;
                EquipHandItem();

            }
        }
    }
    public void OnHotBarSelection5(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            int selectedValue = 5;

            if (selectedValue > 0)
            {
                equippedHotBarIndex = selectedValue - 1;
                EquipHandItem();

            }
        }
    }

    public void OnHotBarSelection6(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            int selectedValue = 6;

            if (selectedValue > 0)
            {
                equippedHotBarIndex = selectedValue - 1;
                EquipHandItem();

            }
        }
    }

    public void OnDrop(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            Slot equippedSlot = hotbarSlots[equippedHotBarIndex];

            if (!equippedSlot.HasItem())
            {
                return;
            }
            ItemSO itemSO = equippedSlot.GetItem();
            GameObject prefab = itemSO.itemPrefab;
          

            // in case that the item dont have prefab 
            if(prefab == null) return;

            GameObject drooped = Instantiate(prefab, Camera.main.transform.position + Camera.main.transform.forward,Quaternion.identity); 

            Item itemWeDrooped = drooped.GetComponent<Item>(); // we take the item Component from the drooped item 
            itemWeDrooped.item = itemSO;// we give it the blueprint of itself
            itemWeDrooped.amount = equippedSlot.GetAmount();// we drop the amount we have

            equippedSlot.ClearSlot();

            EquipHandItem();

        }
    }
    public void OnPickup(InputAction.CallbackContext context)
    {
        if(lookedAtRenderer != null && context.performed )
        {
            Item item = lookedAtRenderer.GetComponent<Item>();
            if( item != null )
            {
                AddItem(item.item, item.amount);
                Destroy(item.gameObject);
                EquipHandItem();

            }
        }
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
            Movement.instance.UpdatingRotation = !Movement.instance.UpdatingRotation;
        }
    }



}
