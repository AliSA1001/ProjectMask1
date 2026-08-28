using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class Slot : MonoBehaviour ,IPointerEnterHandler , IPointerExitHandler
{
    public bool hovering;

    private ItemSO heldItem;
    private int itemAmount;

    private Image iconImage;
    private TextMeshProUGUI amountText;

    private void Awake()
    {
        // here we just say hey get the image from child 0 and text from child 1
        iconImage = transform.GetChild(0).GetComponent<Image>();
        amountText = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
    }
    // we need way to tell inventory about our slot information 
    public ItemSO GetItem()
    {
        return heldItem;
    }
    public int GetAmount()
    {
        return itemAmount;
    }
    // here we need way so the inventory can tell the slot to carry this item
    public void SetItem(ItemSO item , int amount)
    {
        heldItem = item;
        itemAmount = amount;

        UpdateSlot();
    }

    public  void UpdateSlot()
    {
       if (heldItem != null)
        {
            iconImage.enabled = true;
            iconImage.sprite = heldItem.icon;
            amountText.text = itemAmount.ToString();
        }
        else
        {
            iconImage.enabled = false;
            amountText.text = "";
        }
    }

    public int AddAmount ( int amountToAdd)
    {
        itemAmount += amountToAdd;
        UpdateSlot ();
        return itemAmount;
    }
    public int RemoveAmount ( int amountToRemove)
    {
        itemAmount -= amountToRemove;
        if(itemAmount <= 0)
        {
            ClearSlot();
        }
        else
        {
            UpdateSlot();
        }
        return itemAmount;
    }

    private void ClearSlot()
    {
        heldItem = null;
        itemAmount = 0; 
        UpdateSlot() ;
    }

    public bool HasItem()
    {
        return heldItem != null;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
    }
}
