using UnityEngine;

public class TransformInventoryScript : MonoBehaviour
{
   public static TransformInventoryScript instance {  get; private set; }


    private void Awake()
    {
        if(instance != null)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }
}
