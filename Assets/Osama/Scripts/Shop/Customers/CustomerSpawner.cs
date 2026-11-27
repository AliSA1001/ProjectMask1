using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// spawns customers outside while the shop is open. Right click the component -> "Spawn Now" to test
public class CustomerSpawner : MonoBehaviour
{
    [SerializeField] private Customer customerPrefab;
    [SerializeField] private CustomerTypeSO[] customerTypes;

    [Header("Points")]
    [SerializeField] private Transform spawnPoint;
    [Tooltip("Where they walk to first, usually just inside the door")]
    [SerializeField] private Transform entrancePoint;
    [Tooltip("Where they go when done. Empty = back to the spawn point")]
    [SerializeField] private Transform exitPoint;

    [Header("Timing")]
    [SerializeField] private Vector2 spawnInterval = new Vector2(6f, 12f);
    [SerializeField, Min(1)] private int maxCustomers = 3;
    [SerializeField] private bool onlyWhileShopOpen = true;

    private readonly List<Customer> alive = new List<Customer>();
    private float nextSpawnTime;

    public int AliveCount
    {
        get
        {
            alive.RemoveAll(c => c == null);
            return alive.Count;
        }
    }

    private void Start()
    {
        ScheduleNext();
    }

    private void Update()
    {
        if (onlyWhileShopOpen && (ShopManager.instance == null || !ShopManager.instance.IsOpen)) return;
        if (Time.time < nextSpawnTime) return;

        // customers that got destroyed some other way (scene change, killed, ...) never fire OnLeft
        alive.RemoveAll(c => c == null);
        if (alive.Count >= maxCustomers) return;

        Spawn();
        ScheduleNext();
    }

    [ContextMenu("Spawn Now")]
    private void SpawnNow()
    {
        if (Application.isPlaying) Spawn();
    }

    public Customer Spawn(CustomerTypeSO type = null)
    {
        if (type == null) type = PickType();
        if (type == null || customerPrefab == null || spawnPoint == null) return null;

        // snap to the navmesh so the agent doesn't complain if the point is a bit off
        Vector3 pos = spawnPoint.position;
        if (NavMesh.SamplePosition(pos, out NavMeshHit hit, 2f, NavMesh.AllAreas)) pos = hit.position;

        Customer customer = Instantiate(customerPrefab, pos, spawnPoint.rotation);
        customer.name = type.displayName;
        customer.Setup(type, entrancePoint, exitPoint != null ? exitPoint : spawnPoint,
            ShopManager.instance != null ? ShopManager.instance.Checkout : null);
        customer.OnLeft += HandleLeft;

        alive.Add(customer);
        return customer;
    }

    private void HandleLeft(Customer customer)
    {
        alive.Remove(customer);
    }

    private CustomerTypeSO PickType()
    {
        if (customerTypes == null || customerTypes.Length == 0) return null;

        float total = 0f;
        foreach (CustomerTypeSO t in customerTypes)
            if (t != null) total += t.spawnWeight;

        if (total <= 0f) return customerTypes[0];

        float roll = Random.Range(0f, total);
        foreach (CustomerTypeSO t in customerTypes)
        {
            if (t == null) continue;
            roll -= t.spawnWeight;
            if (roll <= 0f) return t;
        }
        return customerTypes[customerTypes.Length - 1];
    }

    private void ScheduleNext()
    {
        nextSpawnTime = Time.time + Random.Range(spawnInterval.x, spawnInterval.y);
    }
}
