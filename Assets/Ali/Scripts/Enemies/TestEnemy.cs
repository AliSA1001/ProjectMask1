using UnityEngine;

public class TestEnemy : MonoBehaviour, IDamgeable
{
    public ParticleSystem GetHitParticleSystem()
    {
        return null;
    }

    public void TakeDamage(float damage)
    {
        Debug.Log("DPS Delt =" +  damage);
    }
}
