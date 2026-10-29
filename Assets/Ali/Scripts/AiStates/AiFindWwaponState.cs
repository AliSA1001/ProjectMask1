using UnityEngine;

public class AiFindWwaponState : AiState
{

    public AiStateId GetId()
    {
        return AiStateId.FindWeapon;
    }
    public void Enter(AiAgent agent)
    {
       Item pickUpItem = FindClosestWeapon(agent);
        agent.navMeshAgent.destination = pickUpItem.transform.position;
    }


    public void Update(AiAgent agent)
    {
    }
    public void Exit(AiAgent agent)
    {
    }

    private Item FindClosestWeapon(AiAgent agent)
    {
        Item[] items = Item.FindObjectsByType<Item>(FindObjectsSortMode.None);
        Item closestItem = null;
        float closestDistance = float.MaxValue;

        foreach (Item item in items)
        {
            if (item.item.canBePickedByEnemy)
            {
                float distanceToItem = Vector3.Distance(agent.transform.position , item.transform.position);
                if(closestDistance > distanceToItem)
                {
                    closestDistance = distanceToItem;
                    closestItem = item;
                }
            }

        }
        return closestItem;
    }
}
