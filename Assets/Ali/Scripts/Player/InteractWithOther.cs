using UnityEngine;
using UnityEngine.InputSystem;

public class InteractWithOther : MonoBehaviour
{
    // All items that can be interacted with and inventory based item will not be used by this code to interact with 
    // other interacable things like doors and boxes etc....

    private Camera mainCamera;
    private bool canInteract = true;
    [SerializeField] private float interactDistance = 7;

    private CombineInventory combineInventory; // we need this in case we opened the combine inventory so we dont interact with others interactable things


    private void OnEnable()
    {
        Inventory.OnOpenANDCloseMainInventoryEvent += OnOpenANDCloseMainInventoryEvent;
        CombineInventory.OnIsInCombainInventory += OnIsInCombainInventory;
    }
    private void OnDisable()
    {
        Inventory.OnOpenANDCloseMainInventoryEvent -= OnOpenANDCloseMainInventoryEvent;
        CombineInventory.OnIsInCombainInventory -= OnIsInCombainInventory;

    }
    private void Awake()
    {

    }
    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {

    }

    private void OnIsInCombainInventory(bool isCombineInventoryOpen)
    {
       canInteract = !isCombineInventoryOpen;
    }
    private void OnOpenANDCloseMainInventoryEvent(bool isInventoryOpen)
    {
        canInteract = !isInventoryOpen;
    }

    private void HandleInteract()
    {
        if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out RaycastHit hit, interactDistance))
        {
            if (hit.collider.TryGetComponent<IInteractable>(out IInteractable target))
            {
               
                    target.Interact();
            }
            else
            {

            }
        }

    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.started && canInteract)
        {
            HandleInteract();
        }
        
    }
}
