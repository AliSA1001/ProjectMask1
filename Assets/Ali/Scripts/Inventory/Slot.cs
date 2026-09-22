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
    private int ammoAmount;

    private Image iconImage;
    private TextMeshProUGUI amountText;
    private TextMeshProUGUI ammoAmountText;

    private void Awake()
    {
        // here we just say hey get the image from child 0 and text from child 1
        iconImage = transform.GetChild(0).GetComponent<Image>();
        amountText = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        ammoAmountText = transform.GetChild(2).GetComponent<TextMeshProUGUI>();
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

    public int GetAmmoAmount()
    {
        return ammoAmount;
    }
    // here we need way so the inventory can tell the slot to carry this item
    public void SetItem(ItemSO item , int amount , int _ammoAmount)
    {
        heldItem = item;
        itemAmount = amount;
        ammoAmount = _ammoAmount;

        UpdateSlot();
    }

    public  void UpdateSlot()
    {
        if(iconImage == null)
        {
            iconImage = transform.GetChild(0).GetComponent<Image>();
            amountText = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            ammoAmountText = transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        }


        if (heldItem != null)
        {
            iconImage.enabled = true;
            iconImage.sprite = heldItem.icon;
            amountText.text = itemAmount.ToString();

            if (heldItem.isFpsRealHandObject)
            {
                ammoAmountText.text = ammoAmount.ToString() + "X";
            }
            else
            {
                ammoAmountText.text= "";
            }

            
        }
        else
        {
            iconImage.enabled = false;
            amountText.text = "";
            if(ammoAmountText != null)
            {
                ammoAmountText.text = "";
            }
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

    public int AddAmmo( int amountToAdd )
    {
        ammoAmount += amountToAdd;
        UpdateSlot ();
        return ammoAmount;
    }
    public int RemoveAmmo( int amountToRemove )
    {
        ammoAmount -= amountToRemove;
        UpdateSlot();
        return ammoAmount;
    }

    public void ClearSlot()
    {
        heldItem = null;
        itemAmount = 0; 
        ammoAmount = 0;
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
