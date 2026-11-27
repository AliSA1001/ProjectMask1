using UnityEngine;

public class Item : MonoBehaviour
{
    public ItemSO item;
    public int amount = 1;

    // if it use ammo
    public int ammoAmount = 0;

    // if it us melee based
    public float durability = 100;
}
