using System;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.UI;

public class STBar : MonoBehaviour
{
    [SerializeField] private float maxST;
    [SerializeField] private float playerSt;
    [SerializeField] private Slider StSlider;
    [SerializeField] private bool didConsumST;

    public Action<bool> OnST_StateChange;

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

        if (movement.isSprinting && playerSt > 0)
        {
            playerSt -= 15 * Time.deltaTime;
            didConsumST = true;
        }

        else if(playerSt <= 0 && movement.isSprinting)
        {
            playerSt = 0;
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



        if(playerSt <= 0)
        {
            OnST_StateChange?.Invoke(false);
        }
        else
        {
            OnST_StateChange?.Invoke(true);
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
