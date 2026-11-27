using UnityEngine;

public static class Wallet
{
    public static int Gold { get; private set; }

    public static void Add(int amount)
    {
        Gold += amount;
        Debug.Log("Gold +" + amount + ", now " + Gold);
    }

    public static void Take(int amount)
    {
        Gold = Mathf.Max(0, Gold - amount);
        Debug.Log("Gold -" + amount + ", now " + Gold);
    }

    public static void Set(int amount)
    {
        Gold = amount;
    }
}