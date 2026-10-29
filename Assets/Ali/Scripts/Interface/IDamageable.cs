using UnityEngine;

public interface IDamgeable
{
    public void TakeDamage(float damage , Vector3 diraction );

    public ParticleSystem GetHitParticleSystem();
}
