using System;
using UnityEngine;
using UnityEngine.InputSystem;

// looks for an IInteractable in front of the camera and fires Interact once per button press.
// Movement.cs still has its old raycast but nothing triggers it as long as the Interact action is wired here
public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float range = 3f;
    [SerializeField] private LayerMask interactLayers = ~0;

    // the UI listens to this. null target / null prompt = looking at nothing useful
    public event Action<IInteractable, string> OnTargetChanged;

    public IInteractable Current { get; private set; }
    private string currentPrompt;

    private readonly RaycastHit[] hits = new RaycastHit[16];

    private void Awake()
    {
        if (playerCamera == null) playerCamera = GetComponentInChildren<Camera>();
    }

    private void Update()
    {
        FindTarget();
    }

    private void FindTarget()
    {
        IInteractable found = null;
        string prompt = null;

        Collider hit = ClosestHit();
        if (hit != null)
        {
            // the collider we hit can be on a child (the product model sitting in a slot for example)
            found = hit.GetComponentInParent<IInteractable>();
            if (found != null)
            {
                IInteractionPrompt p = hit.GetComponentInParent<IInteractionPrompt>();
                if (p != null) prompt = p.GetPrompt();
            }
        }

        // the prompt can change while looking at the same thing (slot got filled etc) so compare both
        if (found != Current || prompt != currentPrompt)
        {
            Current = found;
            currentPrompt = prompt;
            OnTargetChanged?.Invoke(found, prompt);
        }
    }

    // nearest thing in front of the camera that isn't part of the player (the gun and arms sit right in the ray)
    private Collider ClosestHit()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        int count = Physics.RaycastNonAlloc(ray, hits, range, interactLayers, QueryTriggerInteraction.Collide);

        Collider best = null;
        float bestDistance = float.MaxValue;
        for (int i = 0; i < count; i++)
        {
            if (hits[i].collider.transform.IsChildOf(transform)) continue;
            if (hits[i].distance >= bestDistance) continue;

            bestDistance = hits[i].distance;
            best = hits[i].collider;
        }
        return best;
    }

    public void Interact()
    {
        if (Current == null) return;
        if (Current is Component c && c == null) return; // got destroyed this frame
        Current.Interact();
    }

    // hook this to the Interact action on the PlayerInput component
    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed) Interact();
    }
}
