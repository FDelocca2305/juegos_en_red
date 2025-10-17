using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class RoomLightGroup : MonoBehaviour
{
    [Serializable]
    public class LightEntry
    {
        public Light light;
    }

    [SerializeField] private List<LightEntry> entries = new();
    [SerializeField] private bool startOn = true;

    public bool IsOn { get; private set; }
    public event Action<bool> StateChanged;

    void Awake()
    {
        IsOn = startOn;
        ApplyState(IsOn);
    }

    public void Toggle() => Set(!IsOn);

    public void Set(bool on)
    {
        if (IsOn == on) return;
        IsOn = on;
        ApplyState(IsOn);
        StateChanged?.Invoke(IsOn);
    }

    private void ApplyState(bool on)
    {
        foreach (var e in entries)
        {
            if (e.light) e.light.enabled = on;
        }
    }
}