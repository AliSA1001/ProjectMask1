using UnityEngine;

public class NewRecoil : MonoBehaviour
{
    //Rotation 
    private Vector3 currentRotation;
    private Vector3 targetRotation;

    [Header("HipFire")]//HipFire
    [SerializeField] private float recoilX;
    [SerializeField] private float recoilY;
    [SerializeField] private float recoilZ;

    [Header("ADS")]//Aim down Sight
    [SerializeField] private float aimRecoilX;
    [SerializeField] private float aimRecoilY;
    [SerializeField] private float aimRecoilZ;

    [Header("Settings")] //Settings
    [SerializeField] private float snappiness;
    [SerializeField] private float returnSpeed;

    private void OnEnable()
    {
        MasterGun.OnEnableGun += OnEnableGun;
    }
    private void OnDisable()
    {
        MasterGun.OnEnableGun -= OnEnableGun;

    }

    private void OnEnableGun(float[] gunRecoilData)
    {
        recoilX = gunRecoilData[0];
        recoilY = gunRecoilData[1];
        recoilZ = gunRecoilData[2];
        aimRecoilX = gunRecoilData[3];
        aimRecoilY = gunRecoilData[4];
        aimRecoilZ = gunRecoilData[5];
        snappiness = gunRecoilData[6];
        returnSpeed = gunRecoilData[7];
    }

    private void Update()
    {
        targetRotation = Vector3.Lerp(targetRotation,Vector3.zero , returnSpeed * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation,targetRotation , snappiness * Time.fixedDeltaTime);
        transform.localRotation = Quaternion.Euler(currentRotation);
    }

    public void RecoilFire()
    {
        targetRotation += new Vector3(recoilX, Random.Range(-recoilY, recoilY), Random.Range(-recoilZ, recoilZ));
    }

}
