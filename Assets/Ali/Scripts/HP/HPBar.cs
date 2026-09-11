using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    [SerializeField] private float playerHp = 100;
    [SerializeField] private float maxHp = 100;
    [SerializeField] private Slider HpBarSlider;
    [SerializeField] private Slider easeHpBarSlider;
    private float lerpSpeed = 0.01f;


    [SerializeField] private GameObject HighHpMask;
    [SerializeField] private GameObject MidHpMask;
    [SerializeField] private GameObject LowHpMask;

    private void Awake()
    {
        playerHp = maxHp;
        easeHpBarSlider.value = playerHp;
    }



    private void Update()
    {
        HpBarSlider.value = playerHp;


        if (playerHp != easeHpBarSlider.value)
        {
            easeHpBarSlider.value = Mathf.Lerp(easeHpBarSlider.value, playerHp, lerpSpeed);
        }


        if (Input.GetKeyUp(KeyCode.M))
        {
            playerHp -= 25;
        }

        MaskUiChange();
    }

    private void MaskUiChange()
    {
        if(playerHp < (maxHp * 0.33f))
        {
            LowHpMask.SetActive(true);

            HighHpMask.SetActive(false);
            MidHpMask.SetActive(false);
        }
       
        else if (playerHp < (maxHp * 0.67))
        {
            MidHpMask.SetActive(true);

            LowHpMask.SetActive(false);
            HighHpMask.SetActive(true);

        }
        else
        {
            HighHpMask.SetActive(true);

            MidHpMask.SetActive(false);
            LowHpMask.SetActive(false);
        }
    }
}



