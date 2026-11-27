using System.Collections.Generic;
using UnityEngine;

// where customers line up and where the player rings them up.
// Interacting with the counter serves whoever is first in line, interacting with the customer works too
public class CheckoutCounter : MonoBehaviour, IInteractable, IInteractionPrompt
{
    [Tooltip("Index 0 is the spot right at the counter")]
    [SerializeField] private Transform[] queuePoints;
    [Tooltip("Customers past the last point keep lining up with this spacing")]
    [SerializeField] private float overflowSpacing = 1.2f;

    private readonly List<Customer> queue = new List<Customer>();

    public int QueueLength => queue.Count;
    public Customer Front => queue.Count > 0 ? queue[0] : null;

    // returns the spot the customer should walk to
    public Vector3 Join(Customer customer)
    {
        if (!queue.Contains(customer)) queue.Add(customer);
        return GetQueuePosition(queue.IndexOf(customer));
    }

    public void Leave(Customer customer)
    {
        int index = queue.IndexOf(customer);
        if (index < 0) return;

        queue.RemoveAt(index);

        // everybody behind moves up one spot
        for (int i = index; i < queue.Count; i++)
            queue[i].MoveToQueueSpot(GetQueuePosition(i));
    }

    public Vector3 GetQueuePosition(int index)
    {
        if (queuePoints == null || queuePoints.Length == 0) return transform.position;
        if (index < queuePoints.Length) return queuePoints[index].position;

        // past the last point, just keep the line going in the same direction
        Transform last = queuePoints[queuePoints.Length - 1];
        Vector3 dir = queuePoints.Length > 1
            ? (last.position - queuePoints[queuePoints.Length - 2].position).normalized
            : -last.forward;
        return last.position + dir * overflowSpacing * (index - queuePoints.Length + 1);
    }

    // the actual sale. Only the customer at the front who already arrived can be served
    public bool TryServe(Customer customer)
    {
        if (customer == null || Front != customer || !customer.IsWaitingForCheckout) return false;

        int total = customer.BasketTotal;
        if (ShopManager.instance != null) ShopManager.instance.CompleteSale(customer, total);
        customer.CompletePurchase(); // removes itself from the queue and walks out

        return true;
    }

    public string GetPrompt()
    {
        Customer c = Front;
        if (c == null) return "Checkout\nNo customers";
        if (!c.IsWaitingForCheckout) return "Checkout\nCustomer on the way";

        return $"Checkout - {c.BasketCount} item(s) - {ShopManager.FormatMoney(c.BasketTotal)}\n>Complete sale";
    }

    public void Interact()
    {
        TryServe(Front);
    }

    private void OnDrawGizmos()
    {
        if (queuePoints == null) return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < queuePoints.Length; i++)
        {
            if (queuePoints[i] == null) continue;
            Gizmos.DrawWireSphere(queuePoints[i].position, 0.25f);
            if (i > 0 && queuePoints[i - 1] != null)
                Gizmos.DrawLine(queuePoints[i - 1].position, queuePoints[i].position);
        }
    }
}
