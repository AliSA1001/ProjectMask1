using UnityEngine;

// Customer.cs only talks to this. Swap the NavMesh for anything else by writing another implementation
public interface ICustomerMover
{
    bool HasArrived { get; }
    void MoveTo(Vector3 destination);
    void Stop();
    void SetSpeed(float speed);
}
