using UnityEngine;

// a product lying around in the world. Look at it to see what it is, press interact to grab it
[RequireComponent(typeof(BoxCollider))]
public class PickupItem : MonoBehaviour, IInteractable, IInteractionPrompt
{
    [SerializeField] private ProductSO product;
    [Tooltip("Optional. If empty the model gets spawned from product.displayPrefab on Start")]
    [SerializeField] private Transform visual;
    [SerializeField] private float minColliderSize = 0.35f; // small stuff is a pain to aim at otherwise

    public ProductSO Product => product;

    private void Start()
    {
        if (visual == null) BuildVisual();
        else FitCollider();
    }

    public void SetProduct(ProductSO newProduct)
    {
        product = newProduct;
        BuildVisual();
    }

    // spawns the model and fits the box collider around it. The editor scene builder calls this too
    public void BuildVisual()
    {
        if (visual != null)
        {
            if (Application.isPlaying) Destroy(visual.gameObject);
            else DestroyImmediate(visual.gameObject);
            visual = null;
        }

        if (product == null || product.displayPrefab == null) return;

        GameObject go = Instantiate(product.displayPrefab, transform);
        go.name = "Visual";
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        visual = go.transform;

        // the model's own colliders would block the raycast / fight with ours
        foreach (Collider c in go.GetComponentsInChildren<Collider>()) c.enabled = false;

        FitCollider();
    }

    private void FitCollider()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        Bounds b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);

        Vector3 s = transform.lossyScale;
        Vector3 size = new Vector3(
            Mathf.Max(minColliderSize, b.size.x / Mathf.Max(0.0001f, Mathf.Abs(s.x))),
            Mathf.Max(minColliderSize, b.size.y / Mathf.Max(0.0001f, Mathf.Abs(s.y))),
            Mathf.Max(minColliderSize, b.size.z / Mathf.Max(0.0001f, Mathf.Abs(s.z))));

        BoxCollider box = GetComponent<BoxCollider>();
        box.center = transform.InverseTransformPoint(b.center);
        box.size = size;
    }

    public string GetPrompt()
    {
        if (product == null) return null;

        int price = ShopManager.instance != null ? ShopManager.instance.GetSellPrice(product) : product.basePrice;
        string info = $"{product.DisplayName} ({product.category}) - {ShopManager.FormatMoney(price)}";

        PlayerCarry carry = PlayerCarry.instance;
        if (carry != null && carry.IsFull) return info + "\nHands full";

        return info + "\n>Pick up";
    }

    public void Interact()
    {
        PlayerCarry carry = PlayerCarry.instance;
        if (carry == null || product == null) return;

        if (carry.TryAdd(product))
            Destroy(gameObject);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (product != null)
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, product.DisplayName);
    }
#endif
}
