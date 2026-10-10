using System.Collections.Generic;
using UnityEngine;

public class Knife_Hitbox : MonoBehaviour
{
    [SerializeField] private Knife Knife;
    public List<EnemyHP> enemiesList;
    

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<EnemyHP>() != null)
        {
            EnemyHP enemyHP = other.GetComponentInParent<EnemyHP>();


            if (other.TryGetComponent(out IDamgeable damgeable) && !enemiesList.Contains(enemyHP))
            {
                enemiesList.Add(enemyHP);
                damgeable.TakeDamage(Knife.attackDps);
                Knife.DurabilityLose();

            }

        }
    }
    

    public void ResetList()
    {
        enemiesList.Clear();
    }
}
