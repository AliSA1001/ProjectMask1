using UnityEngine;

[CreateAssetMenu()]
public class AIAgentConfig : ScriptableObject
{
     public float maxTime = 1;
     public float maxDistance = 1;

     public float dieForce = 10;

}
