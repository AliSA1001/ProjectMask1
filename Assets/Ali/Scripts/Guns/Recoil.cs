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


    
}
