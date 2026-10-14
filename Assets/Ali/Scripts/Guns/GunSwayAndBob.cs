using Unity.Mathematics;
using UnityEngine;

public class GunSwayAndBob : MonoBehaviour
{
    [Header("Settings")]
    public bool sway = true;
    public bool swayRotation = true;
    public bool bobOffset = true;
    public bool bobSway = true;

    private Movement movement;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector3 initialPos;
    private Quaternion initialRot;

    // values we will use in more then one place
    private float smooth = 10f;
    private float smoothRot = 12f;

    [Header("Sway Seetings")]
    [SerializeField] private float step = 0.01f;// we mutiply here by the vaule from the mouse for 1 frame 
    [SerializeField] private float maxStepDistance = 0.06f; // max distance from the local origin 
    private Vector3 swayPos; // store the vaule for later

    [Header("SwayRotation Settings")]
    [SerializeField] private float rotationStep = 4f;
    [SerializeField] private float maxRotationStep = 5f;
    private Vector3 swayEulerRotation;

    private void Awake()
    {
        initialPos = transform.localPosition;
        initialRot = transform.localRotation;
    }
    private void Start()
    {
        movement = Movement.instance;
    }

    private void Update()
    {
        GetInput();

        Sway();
        SwayRotation();

        CompositePosititionRotation();


    }

    private void GetInput()
    {
       moveInput = movement.GetMoveInput();
       lookInput = movement.GetLookInput();
    }
    private void Sway()
    {
        if(sway == false) { swayPos = Vector3.zero; return; }

        Vector3 invertLook = lookInput * -step;
        invertLook.x = Mathf.Clamp(invertLook.x, -maxStepDistance, maxStepDistance);
        invertLook.y = Mathf.Clamp(invertLook.y, -maxStepDistance, maxStepDistance);

        swayPos = invertLook + initialPos;
    }

    private void SwayRotation()
    {
        if(swayRotation == false) { swayEulerRotation = Vector3.zero; return; }

        Vector2 invertLook = lookInput * -rotationStep;
        invertLook.x = Mathf.Clamp(invertLook.x , -maxRotationStep, maxRotationStep);
        invertLook.y = Mathf.Clamp(invertLook.y, -maxRotationStep, maxRotationStep);

        swayEulerRotation = new Vector3 (invertLook.y , invertLook.x , invertLook.x) + initialRot.eulerAngles;

    }

    private void CompositePosititionRotation()
    {
        // position 
        transform.localPosition =
            Vector3.Lerp(transform.localPosition,
            swayPos,
            smooth * Time.deltaTime);

        //rotation 
        transform.localRotation = 
            Quaternion.Slerp(transform.localRotation,
            Quaternion.Euler(swayEulerRotation),
            smoothRot * Time.deltaTime);
            
    }
}
