using UnityEngine;
using UnityEngine.InputSystem;

public class InteractWithOther : MonoBehaviour
{
    // All items that can be interacted with and inventory based item will not be used by this code to interact with 
    // other interacable things like doors and boxes etc....

    private Camera mainCamera;
    private bool canInteract;
    [SerializeField] private float interactDistance = 7;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        HandleInteract();
    }

    private void HandleInteract()
    {
        if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out RaycastHit hit, interactDistance))
        {
            if (hit.collider.TryGetComponent<IInteractable>(out IInteractable target))
            {
                if (canInteract)
                    target.Interact();
            }
            else
            {

            }
        }

    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            canInteract = true;
        }
        else if (context.canceled)
        {
            canInteract = false;
        }
    }
}
