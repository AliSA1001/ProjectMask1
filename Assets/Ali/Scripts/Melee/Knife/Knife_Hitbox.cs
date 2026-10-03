using UnityEngine;

public class Knife_HitScan : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<IDamgeable>() != null)
        {
            other.GetComponent<IDamgeable>().TakeDamage(50);
        }
    }
}
