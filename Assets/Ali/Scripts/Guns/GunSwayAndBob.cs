using Unity.Mathematics;
using UnityEngine;

public class GunSwayAndBob : MonoBehaviour
{
    [SerializeField] private MasterGun gunScript;

    [Header("Settings")]
    public bool sway = true;
    public bool swayRotation = true;
    public bool bobOffset = true;
    public bool bobSway = true;

    private Movement movement;
    private CharacterController characterController;

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

    [Header("Bobbing")]
    [SerializeField] private float speedCurve;
    [SerializeField] private Vector3 traveLimit = Vector3.one * 0.025f;
    [SerializeField] private Vector3 bobLimit = Vector3.one * 0.01f;
    private Vector3 bobPosition;

    private float curveSin { get => Mathf.Sin(speedCurve); }
    private float curveCos {  get => Mathf.Cos(speedCurve); }

    [Header("Bob Rotation")]
    [SerializeField] private Vector3 multiplier;
    private Vector3 bobEulerRotation;
    private void Awake()
    {
        initialPos = transform.localPosition;
        initialRot = transform.localRotation;
    }
    private void Start()
    {
        movement = Movement.instance;
        characterController = movement.GetCharacterController();
    }

    private void LateUpdate()
    {
        if (gunScript.isAiming)
        {
            HandleStopSwayAndBob();
        }
        else
        {
            GetInput();

            Sway();
            SwayRotation();
            BobOffset();
            BobRotation();

            CompositePosititionRotation();
        }


    }

    private void GetInput()
    {
       moveInput = movement.GetMoveInput();
       lookInput = movement.GetLookInput();
    }

    private void HandleStopSwayAndBob()
    {
        transform.localPosition = initialPos;
        transform.localRotation = initialRot;
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


    private void BobOffset()
    {
        speedCurve += Time.deltaTime * (movement.isGrounded() ? characterController.velocity.magnitude : 1f) + 0.01f;

        if(bobOffset == false) { bobPosition = Vector3.zero; return; }

        bobPosition.x = 
            (curveCos * bobLimit.x * (movement.isGrounded() ? 1 : 0))
            - (moveInput.x * traveLimit.x);

        bobPosition.y = 
            (curveSin * bobLimit.y )
            - (characterController.velocity.y * traveLimit.y);

        bobPosition.z = - (moveInput.y * traveLimit.z );
    }

    private void BobRotation()
    {
        if (bobSway == false) { bobEulerRotation = Vector3.zero; return; }

        bobEulerRotation.x = (moveInput != Vector2.zero ? multiplier.x * (Mathf.Sin(2 * speedCurve)) :
                                                          multiplier.x * (Mathf.Sin (2 * speedCurve) / 2));
        bobEulerRotation.y = (moveInput != Vector2.zero ? multiplier.y * curveCos : 0);
        bobEulerRotation.z = (moveInput != Vector2.zero ? multiplier.z * curveCos * moveInput.x : 0);
    }


    private void CompositePosititionRotation()
    {
        // position 
        transform.localPosition =
            Vector3.Lerp(transform.localPosition,
            swayPos + bobPosition,
            smooth * Time.deltaTime);

        //rotation 
        transform.localRotation = 
            Quaternion.Slerp(transform.localRotation,
            Quaternion.Euler(swayEulerRotation) * Quaternion.Euler(bobEulerRotation),
            smoothRot * Time.deltaTime);
            
    }


}
