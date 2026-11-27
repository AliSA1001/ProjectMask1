using UnityEngine;

public class NightExit : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (DayCycleManager.Instance == null) return;

        DayCycleManager.Instance.EndNight();
    }
}