using UnityEngine;

public class Knife_Hitbox : MonoBehaviour
{
    [SerializeField] private Knife Knife;
    
    private void OnTriggerEnter(Collider other)
    {
       
        if (other.TryGetComponent(out IDamgeable damgeable))
        {
            damgeable.TakeDamage(Knife.attackDps);
            Knife.DurabilityLose();
        }

    }
}
