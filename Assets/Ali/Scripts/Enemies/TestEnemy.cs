using UnityEngine;

public class TestEnemy : MonoBehaviour, IDamgeable
{
    public ParticleSystem GetHitParticleSystem()
    {
        return null;
    }

    public void TakeDamage(float damage, Vector3 direction)
    {
        Debug.Log("DPS Delt =" +  damage);
    }
}
