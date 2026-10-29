using UnityEngine;

public class InteractableObject : MonoBehaviour, IInteractable
{
    private TransformInventoryScript TransformInventory;

    private void Start()
    {
        TransformInventory = TransformInventoryScript.instance;

    }
    public void Interact()
    {
        Debug.Log("interact with box");
        TransformInventory.transform.GetChild(0).gameObject.SetActive(true);
    }
}
