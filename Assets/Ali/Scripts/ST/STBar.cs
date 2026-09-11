using UnityEngine;
using UnityEngine.UI;

public class STBar : MonoBehaviour
{
    [SerializeField] private float maxST;
    [SerializeField] private float playerSt;
    [SerializeField] private Slider StSlider;
    [SerializeField] private bool didConsumST;

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
            playerSt -= 15 * Time.deltaTime;
            didConsumST = true;
        }
        else
        {
            if(didConsumST)
            {
                Invoke("GainST", 2);
                
            }
            else
            {
                GainST();
            }
        }
    }

    private void GainST()
    {
        if (playerSt < maxST)
        {
            playerSt += Time.deltaTime * 10;
        }
       didConsumST = false;
    }
}
