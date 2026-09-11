using UnityEngine;
using UnityEngine.UI;

public class STBar : MonoBehaviour
{
    [SerializeField] private float maxST;
    [SerializeField] private float playerSt;
    [SerializeField] private Slider StSlider;

    //ref 
    private Movement movement;


    private void Awake()
    {
        playerSt = maxST;
    }
    private void Start()
    {
        movement = Movement.instance;
    }

    private void Update()
    {
        StSlider.value = playerSt;

        if (movement.isSprinting)
        {
            playerSt -= 1 * Time.deltaTime;
        }
    }
}
