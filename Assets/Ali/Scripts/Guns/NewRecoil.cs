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

    [Header("Settings")] //Settings
    [SerializeField] private float snappiness;
    [SerializeField] private float returnSpeed;



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
