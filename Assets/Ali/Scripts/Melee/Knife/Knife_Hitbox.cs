using UnityEngine;

public class Knife_Hitbox : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
       
        if (other.TryGetComponent(out IDamgeable damgeable))
        {
            damgeable.TakeDamage(50);
        }
    }
}
