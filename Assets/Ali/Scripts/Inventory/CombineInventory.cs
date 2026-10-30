using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class CombineInventory : MonoBehaviour
{
    public static CombineInventory Instance;
    [SerializeField] private Inventory inventory_player;

    private GameObject Continer;

    public GameObject inventorySlotParent_Player;
    public GameObject inventorySlotParent_OtherInventory;

    private List<Slot> inventorySlots_Player = new List<Slot>();
    private List<Slot> inventorySlots_OtherInventory = new List<Slot>();




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


        inventorySlots_Player.AddRange(inventory_player.inventorySlots);
        inventorySlots_OtherInventory.AddRange(inventorySlotParent_OtherInventory.GetComponentsInChildren<Slot>());


    }

    private void Start()
    {
        Continer = transform.GetChild(0).gameObject;
    }
    public void OpenOtherInventory(List<Item> otherInvenotryItems)
    {
        Continer.SetActive(true);// we need to get our inventory info and the other npc inventory inventory
        
    }
}
