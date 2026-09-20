using UnityEngine;

// put on an object with a trigger collider: when the player walks in, the cutscene plays
public class CutsceneTrigger : MonoBehaviour
{
    [SerializeField] private Cutscene cutscene;
    [Tooltip("Ticked = the cutscene can't be triggered again by this zone after it played")]
    [SerializeField] private bool onlyOnce = true;

    private bool used;

    private void Reset()
    {
        Collider zone = GetComponent<Collider>();
        if (zone == null) zone = gameObject.AddComponent<BoxCollider>();
        zone.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (cutscene == null || (onlyOnce && used)) return;

        // only the player (the collider can be on a child)
        Movement player = other.GetComponentInParent<Movement>();
        if (player == null || player != Movement.instance) return;

        if (cutscene.Play()) used = true;
    }

    private void OnDrawGizmos()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        if (box == null) return;

        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = new Color(0.9f, 0.7f, 0.2f, 0.25f);
        Gizmos.DrawCube(box.center, box.size);
        Gizmos.color = new Color(0.9f, 0.7f, 0.2f, 1f);
        Gizmos.DrawWireCube(box.center, box.size);
    }
}
