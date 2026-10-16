using UnityEngine;

public class Recoil : MonoBehaviour
{
    [SerializeField] private Transform pivotPoint;
    [Header("Kick")]
    [SerializeField] private float pitchKick = 1.2f;
    [SerializeField] private float yawKick = 0.4f;
    [SerializeField] private float yawRandomness = 1.0f;

    [Header("Spring Settings")]
    [SerializeField] private float returnStrength = 20f;
    [SerializeField] private float daming = 18f;

    [Header("Limts")]
    [SerializeField] private float maxPitch = 20f;

    private Vector2 recoilOffset;
    private Vector2 recoilVelocity;


    private void LateUpdate()
    {
        float dt = Time.deltaTime;

        Vector2 accel = (-returnStrength * recoilOffset) - (daming* recoilVelocity);
        recoilVelocity += accel * dt;
        recoilOffset += recoilVelocity * dt;

        recoilOffset.y = Mathf.Clamp(recoilOffset.y,-maxPitch, maxPitch);

        pivotPoint.localRotation = Quaternion.Euler(-recoilOffset.y , recoilOffset .x , 0);
    }
    public void AddRecoil(float recoilMultiplier = 1.0f)
    {
        float yaw = Random.Range(-yawKick,yawKick) * yawRandomness;

        recoilOffset.y += pitchKick * recoilMultiplier;
        recoilOffset.x += yaw * recoilMultiplier;
    }


    public void ResetRecoil()
    {
        recoilOffset = Vector2.zero;
        recoilVelocity = Vector2.zero;
        pivotPoint.localRotation = Quaternion.identity;
    }
}
