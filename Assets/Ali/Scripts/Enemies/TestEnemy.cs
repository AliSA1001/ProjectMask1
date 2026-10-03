using UnityEngine;

public class TestEnemy : MonoBehaviour, IDamgeable
{
    public void TakeDamage(float damage)
    {
        Debug.Log("DPS Delt =" +  damage);
    }
}
