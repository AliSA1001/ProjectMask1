using UnityEngine;
using UnityEngine.AI;

public class RagDoll : MonoBehaviour
{
   private Rigidbody[] rigidbodies;
    private Animator animator;

    private void Start()
    {
        rigidbodies = GetComponentsInChildren<Rigidbody>();
        animator = GetComponent<Animator>();

        DeactivetRagdoll();
    }

    public void DeactivetRagdoll()
    {
        foreach (var rigidbody in rigidbodies)
        {
            rigidbody.isKinematic = true;
            animator.enabled = true;
         
        }
    }
    public void ActivateRagdoll()
    {
        foreach(var rigidbody in rigidbodies)
        {
            rigidbody.isKinematic = false;
            animator.enabled = false;
        }
    }


    public void ApplyForce(Vector3 force)
    {
        Rigidbody rigidbody = animator.GetBoneTransform(HumanBodyBones.Hips).GetComponent<Rigidbody>();
        rigidbody.AddForce(force , ForceMode.VelocityChange);
    }
}
