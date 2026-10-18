using System;
using UnityEngine;

public class ReloadEvent : MonoBehaviour
{
    public event Action OnReload;
    public event Action OnMagRemove;
    public event Action OnMagAdd;

    private void OnEndReload()
    {
        OnReload?.Invoke();
    }
    private void OnMagRemoveStart()
    {
        OnMagRemove?.Invoke();
    }
    private void OnMagAddStart()
    {
        OnMagAdd?.Invoke();
    }
}
