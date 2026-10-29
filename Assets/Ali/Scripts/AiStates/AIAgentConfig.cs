using UnityEngine;

[CreateAssetMenu()]
public class AIAgentConfig : ScriptableObject
{
     public float maxTime = 1;
     public float maxDistance = 1;

     public float dieForce = 10;

     public float maxSightDistance = 5.05f;

}
