using UnityEngine;

public class HitBox : MonoBehaviour, IDamgeable
{
    // this class calls the function in the health component at the main enemy
    private EnemyHP enemyHP;

    

    private void Start()
    {
        enemyHP = GetComponentInParent<EnemyHP>();
    }
    public void TakeDamage(float damage, Vector3 diraction)
    {
      enemyHP.TakeDamage(damage , diraction);
    }

    public ParticleSystem GetHitParticleSystem()
    {
        return enemyHP.hitParticle;
    }
}
