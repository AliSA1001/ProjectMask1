using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class OtherInventory : MonoBehaviour
{


    public List<Item> Items = new List<Item>();

    private void Awake()
    {
    }
   

     public List<Item> GetItems()
    {
        return Items;
    }

}
