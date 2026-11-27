using System;
using System.Collections.Generic;
using UnityEngine;

// the customer brain. Walks in -> grabs stuff off the shelves -> lines up at the checkout -> leaves.
// Movement goes through ICustomerMover so the NavMesh can be swapped without touching this file
public class Customer : MonoBehaviour, IInteractable, IInteractionPrompt
{
    public enum State
    {
        Entering,
        Browsing, // shelves are empty, hang around a bit and look again
        WalkingToShelf,
        Taking,
        WalkingToCheckout,
        WaitingForCheckout,
        Leaving
    }

    [SerializeField] private CustomerTypeSO customerType;
    [SerializeField] private Transform handPoint;

    [Header("Placeholder look")]
    [SerializeField] private Transform visualRoot;
    [SerializeField] private Renderer bodyRenderer;
    [SerializeField] private Renderer maskRenderer;

    [Header("Tuning")]
    [SerializeField] private float browseTime = 3f;
    [SerializeField] private int maxBrowseTries = 3;
    [SerializeField] private float turnSpeed = 360f;

    public CustomerTypeSO Type => customerType;
    public State CurrentState { get; private set; }
    public IReadOnlyList<ProductSO> Basket => basket;
    public int BasketCount => basket.Count;
    public int BasketTotal { get; private set; }
    public bool Paid { get; private set; }
    public bool IsWaitingForCheckout => CurrentState == State.WaitingForCheckout;

    public event Action<Customer, State> OnStateChanged;
    public event Action<Customer> OnLeft; // fired right before the object gets destroyed

    private ICustomerMover mover;
    private CheckoutCounter checkout;
    private Transform entrancePoint;
    private Transform exitPoint;

    private readonly List<ProductSO> basket = new List<ProductSO>();
    private ShelfSlot targetSlot;
    private int itemsWanted;
    private int browseTries;
    private float stateTimer;
    private float queueJoinTime;
    private GameObject handVisual;

    private float PickDelay => customerType != null ? customerType.pickDelay : 1f;
    private float Patience => customerType != null ? customerType.patience : 0f;

    private void Awake()
    {
        mover = GetComponent<ICustomerMover>();
        if (mover == null)
            Debug.LogError("Customer has no ICustomerMover, add a NavMeshCustomerMover", this);
    }

    // the spawner calls this right after Instantiate. Everything is optional so a customer
    // dropped into the scene by hand still works
    public void Setup(CustomerTypeSO type, Transform entrance, Transform exit, CheckoutCounter counter)
    {
        customerType = type;
        entrancePoint = entrance;
        exitPoint = exit;
        checkout = counter;
    }

    private void Start()
    {
        if (checkout == null)
            checkout = ShopManager.instance != null ? ShopManager.instance.Checkout : FindFirstObjectByType<CheckoutCounter>();

        if (customerType != null)
        {
            ApplyLook();
            mover.SetSpeed(customerType.moveSpeed);
            itemsWanted = customerType.RollItemCount();
        }
        else
        {
            itemsWanted = 1;
        }

        if (entrancePoint != null)
        {
            mover.MoveTo(entrancePoint.position);
            SetState(State.Entering);
        }
        else
        {
            LookForProduct();
        }
    }

    private void Update()
    {
        if (mover == null) return;
        stateTimer += Time.deltaTime;

        switch (CurrentState)
        {
            case State.Entering:
                if (mover.HasArrived) LookForProduct();
                break;

            case State.Browsing:
                if (stateTimer >= browseTime) LookForProduct();
                break;

            case State.WalkingToShelf:
                if (targetSlot == null || targetSlot.IsEmpty)
                {
                    // somebody (probably the player) grabbed it before we got there
                    LookForProduct();
                }
                else if (mover.HasArrived)
                {
                    mover.Stop();
                    SetState(State.Taking);
                }
                break;

            case State.Taking:
                if (targetSlot != null) FaceTowards(targetSlot.transform.position);
                if (stateTimer >= PickDelay) GrabProduct();
                break;

            case State.WalkingToCheckout:
                if (mover.HasArrived)
                {
                    mover.Stop();
                    SetState(State.WaitingForCheckout);
                }
                break;

            case State.WaitingForCheckout:
                if (checkout != null) FaceTowards(checkout.transform.position);
                if (Patience > 0f && Time.time - queueJoinTime > Patience) GiveUp();
                break;

            case State.Leaving:
                if (mover.HasArrived) Despawn();
                break;
        }
    }

    // ---------- shopping ----------

    private void LookForProduct()
    {
        ClearTarget();

        if (basket.Count < itemsWanted && ShopManager.instance != null)
            targetSlot = ShopManager.instance.FindSlotFor(this);

        if (targetSlot != null)
        {
            targetSlot.TryReserve(this);
            mover.MoveTo(targetSlot.Shelf.CustomerStandPoint.position);
            SetState(State.WalkingToShelf);
        }
        else if (basket.Count > 0)
        {
            GoToCheckout();
        }
        else if (browseTries < maxBrowseTries)
        {
            browseTries++;
            SetState(State.Browsing);
        }
        else
        {
            Leave(); // nothing to buy
        }
    }

    public bool Wants(ProductSO product)
    {
        return customerType == null || customerType.Wants(product);
    }

    private void GrabProduct()
    {
        if (targetSlot != null && !targetSlot.IsEmpty)
        {
            int price = targetSlot.Price; // read it before Take() clears it
            ProductSO product = targetSlot.Take();
            if (product != null)
            {
                basket.Add(product);
                BasketTotal += price;
                ShowInHand(product);
            }
        }

        targetSlot = null;
        LookForProduct(); // keeps going until the basket is full
    }

    private void ClearTarget()
    {
        if (targetSlot != null) targetSlot.ReleaseReservation(this);
        targetSlot = null;
    }

    private void ShowInHand(ProductSO product)
    {
        if (handPoint == null || product.displayPrefab == null) return;

        if (handVisual != null) Destroy(handVisual);
        handVisual = Instantiate(product.displayPrefab, handPoint.position, handPoint.rotation, handPoint);
        foreach (Collider c in handVisual.GetComponentsInChildren<Collider>()) c.enabled = false;
    }

    // ---------- checkout ----------

    private void GoToCheckout()
    {
        if (checkout == null)
        {
            Leave();
            return;
        }

        queueJoinTime = Time.time;
        mover.MoveTo(checkout.Join(this));
        SetState(State.WalkingToCheckout);
    }

    // the counter calls this when the line moves forward
    public void MoveToQueueSpot(Vector3 position)
    {
        if (CurrentState != State.WalkingToCheckout && CurrentState != State.WaitingForCheckout) return;

        mover.MoveTo(position);
        SetState(State.WalkingToCheckout);
    }

    // the counter calls this once the player rang them up
    public void CompletePurchase()
    {
        Paid = true;
        if (checkout != null) checkout.Leave(this);
        Leave();
    }

    private void GiveUp()
    {
        // ran out of patience. Right now they just walk off with the stuff,
        // change this if you'd rather have them put it back or drop it
        if (checkout != null) checkout.Leave(this);
        Leave();
    }

    private void Leave()
    {
        ClearTarget();
        SetState(State.Leaving);

        if (exitPoint != null) mover.MoveTo(exitPoint.position);
        else Despawn();
    }

    private void Despawn()
    {
        OnLeft?.Invoke(this);
        Destroy(gameObject);
    }

    // ---------- helpers ----------

    private void SetState(State newState)
    {
        CurrentState = newState;
        stateTimer = 0f;
        OnStateChanged?.Invoke(this, newState);
    }

    private void FaceTowards(Vector3 point)
    {
        Vector3 dir = point - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion target = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, target, turnSpeed * Time.deltaTime);
    }

    private void ApplyLook()
    {
        if (customerType.visualPrefab != null && visualRoot != null)
        {
            // real character instead of the placeholder capsule
            Destroy(visualRoot.gameObject);
            visualRoot = Instantiate(customerType.visualPrefab, transform).transform;
            return;
        }

        if (bodyRenderer != null) bodyRenderer.material.color = customerType.bodyColor;
        if (maskRenderer != null) maskRenderer.material.color = customerType.maskColor;
    }

    // ---------- player interaction (looking at the customer while he waits) ----------

    public string GetPrompt()
    {
        string label = customerType != null ? customerType.displayName : "Customer";

        if (IsWaitingForCheckout && checkout != null && checkout.Front == this)
            return $"{label} - {BasketCount} item(s) - {ShopManager.FormatMoney(BasketTotal)}\n>Complete sale";

        if (CurrentState == State.WalkingToCheckout || CurrentState == State.WaitingForCheckout)
            return $"{label}\nWaiting in line";

        return label;
    }

    public void Interact()
    {
        if (checkout != null) checkout.TryServe(this);
    }
}
