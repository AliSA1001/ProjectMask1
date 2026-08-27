using System;
using UnityEngine;

public class ReloadEvent : MonoBehaviour
{
    public event Action OnReload;

    private void OnEndReload()
    {
        OnReload?.Invoke();
    }
}
