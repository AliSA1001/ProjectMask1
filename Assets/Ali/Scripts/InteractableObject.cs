using UnityEngine;

public class InteractableObject : MonoBehaviour, IInteractable
{
    private CombineInventory combineInventory;
    private OtherInventory otherInventory;
    

    private void Start()
    {
        combineInventory = CombineInventory.Instance;
        otherInventory = GetComponent<OtherInventory>();

    }
    public void Interact()
    {
        Debug.Log("interact with box");
        combineInventory.OpenOtherInventory(otherInventory.GetItems());
    }
}
